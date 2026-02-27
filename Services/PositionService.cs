using System.Security.Claims;
using b1.Controllers;
using b1.DTOs;
using b1.Infrastructure;
using b1.Main;
using b1.Models;
using b1.Repositories;
using b1.Respositories;
using b1.Srevices;
using MongoDB.Bson;
using MongoDB.Driver;
using ScottPlot.Colormaps;

namespace b1.Services{
    public class PositionService
    {
        private IServiceProvider _sp;
        private IPositionVerificationRepository _posVerRepo;
        private IPortfolioRepository _ptfRepo;
        private IPositionRepository _positionRepo;
        private IUserRepository _usrRepo;
        private PriceContext _priceCtx;
        private ICommandRepository _cmdRepo;
        private ICommandChannel _cmdChannel;
        public PositionService(IPositionRepository positionRepo, IPortfolioRepository ptfRepo,
            IUserRepository userRepo, PriceContext priceCtx, IServiceProvider sp,
            ICommandRepository cmdRepo, IPositionVerificationRepository posVerRepo,
            ICommandChannel cmdChannel)
        {
            _sp = sp;
            _posVerRepo = posVerRepo;
            _positionRepo = positionRepo;
            _usrRepo = userRepo;
            _priceCtx = priceCtx;
            _ptfRepo = ptfRepo;
            _cmdRepo = cmdRepo;
            _cmdChannel = cmdChannel;
        }
        
        internal async Task<OpenPositionResult> RequestToOpenPosition(
            PositionCreationRequest request,ClaimsPrincipal principal, string portfolioId)
        {
            string symbol = request.Symbol;
            long qtty = request.Quantity;
            string? userId = principal.FindFirst("sub")?.Value;
            User? user = userId == null ? null : await _usrRepo.GetUserById(userId);
            if (userId == null || user == null)
            {
                return new OpenPositionResult("Internal Error");
            }
            TimedPrice? tp = _priceCtx.GetTimedPrice(symbol);
            if (tp == null)
            {
                throw new Exception("No price for symbol " + symbol);
            }
            if (request.PositionType == PositionDirection.LONG)
            {
                double? balanceRes = await _usrRepo.GetUserBalance(userId);
                if (balanceRes == null)
                {
                    return new OpenPositionResult("Internal Error");
                }
                double balance = balanceRes.Value;

                if (tp.Price * qtty > balance)
                {
                    return new OpenPositionResult("Not enough money");
                }
            }
            PositionVerification verification =
                    new PositionVerification(symbol, qtty, tp.Price, DateTime.UtcNow.ToString(),
                        portfolioId, request.PositionType);
            await _posVerRepo.TryAdd(verification.Id, verification);
            return new OpenPositionResult(verification);
        }

        internal async Task<PositionDTO?> OpenPositionFinalize(string posVerId,ClaimsPrincipal cp)
        {
            int state = 0;
            CommandCreatorBase? proc = null;
            PositionCommand? cmd = null;
            string ptfType;
            bool foundVer;

            string? userId = cp.FindFirst("sub")?.Value;
            User? user = userId == null ? null : await _usrRepo.GetUserById(userId);
            if (user == null)
            {
                throw new UserNotFoundException("No user");
            }

            foundVer = await _posVerRepo.TryGet(posVerId, out var verification);
            if (!foundVer || verification == null)
            {
                
                return null;
            }
            DateTime timeIssued = DateTime.SpecifyKind(
                    DateTime.Parse(verification.TimeIssued), DateTimeKind.Utc);

            DateTimeOffset issuedOffset = timeIssued;
            DateTimeOffset nowOffset = DateTime.SpecifyKind(DateTime.UtcNow,DateTimeKind.Utc);
            if (nowOffset.AddSeconds(-10) >= issuedOffset) {
                return null;
            }
            Position position = new Position(verification.PortfolioId, verification.Symbol,
                    verification.Quantity, timeIssued, verification.Price, verification.PositionType);
            if (verification.PositionType == PositionDirection.LONG &&
                user.Balance < verification.Price * verification.Quantity)
            {
                return null;
            }
            Portfolio? ptf = await _ptfRepo.GetPortfolioByIdAsync(verification.PortfolioId);
            if (ptf == null)
            {
                return null;
            }
            try
            {
                ptfType = ptf.PortfolioType;
                switch (ptfType)
                {
                    case "ADVANCED":
                        proc = _sp.GetRequiredService<AdvancedCommandCreator>();
                        break;
                    default:// "REGULAR"
                        proc = _sp.GetRequiredService<RegularCommandCreator>();
                        break;

                }
                await _positionRepo.AddPostion(position);
                state = 1;
                cmd = await proc.TryAddCommand(position, user);
                state = 2; //Position persisted
                if (cmd == null)
                    return null;
                if (position.PositionType == PositionDirection.LONG)
                    await _usrRepo.UpdateUserBalance(user.Id, user.Balance - verification.Price * verification.Quantity);
                else
                {   
                    await _usrRepo.UpdateUserBalance(user.Id, user.Balance + verification.Price * verification.Quantity);
                }
                PositionDTO dto = new PositionDTO(position);
                return dto;
            }
            catch (Exception)
            {
                switch (state)
                {
                    case 1:
                        await _positionRepo.DeletePosition(position.Id);
                        break;
                    case 2:
                        await _positionRepo.DeletePosition(position.Id);
                        if (cmd != null)
                            await _cmdRepo.DeleteCommand(cmd.Id);
                        break;
                }
            }            
            return null;
        }
        public async Task<Position?> ClosePositionAsync(string ptfId, string positionId, ClaimsPrincipal cp)
        {
            string? userId = cp?.FindFirst("sub")?.Value;
            User? user = userId == null ? null : await _usrRepo.GetUserById(userId);
            if (user == null)
            {
                throw new UserNotFoundException("No user");
            }
            Position? pos = await _positionRepo.GetPosition(positionId);
            if (pos == null)
            {
                return null;
            }
            if (pos.Closed == true)
                return null;
            long qtty = pos.Quantity;
            TimedPrice? tp = _priceCtx.GetTimedPrice(pos.AssetSymbol);
            if (tp == null)
            {
                throw new Exception("Couldn't get price for: " + pos.AssetSymbol);
            }
            double currPrice = tp.Price;
            if (pos.PositionType == PositionDirection.LONG)
            {
                await CloseLongPostionAsync(pos, currPrice, qtty, user);
            }
            else
            {
                await CloseShortPositionAsync(pos, currPrice, qtty, user);
            }
            return pos;
        }
        
        private async Task CloseShortPositionAsync(Position pos, double currPrice,
            long quantity, User user)
        {
            var cmd = await _cmdRepo.GetCommandForOwner(pos.Id);
            if (cmd == null)
            {
                throw new Exception("Failed to retrieve command");
            }
            try
            {
                await _cmdChannel.ExecuteCommand(cmd);
                var balance = user.Balance;
                balance -= currPrice * quantity;
                await _usrRepo.UpdateUserBalance(user.Id, balance);
                if (balance < 0)
                {
                    //here would be the place to deactivate the user
                }
                await _positionRepo.UpdatePositionStatus(pos.Id);
            }
            catch (Exception)
            {
                //retry if command execution failed
            }
        }

        private async Task CloseLongPostionAsync(Position pos,double currPrice,
            long quantity,User user)
        {
            Console.WriteLine("PositionID = " + pos.Id);
            var cmd = await _cmdRepo.GetCommandForOwner(pos.Id);
            if (cmd == null)
            {
                throw new Exception("Failed to retrieve command");
            }
            try
            {
                await _cmdChannel.ExecuteCommand(cmd);
                var balance = user.Balance;
                balance += currPrice * quantity;
                await _usrRepo.UpdateUserBalance(user.Id, balance);
                await _positionRepo.UpdatePositionStatus(pos.Id);
            }
            catch (Exception)
            {
                //retry if command execution failed
            }
        }
    }

    [Serializable]
    internal class UserNotFoundException : Exception
    {
        public UserNotFoundException(){}

        public UserNotFoundException(string? message) : base(message){}

        public UserNotFoundException(string? message, Exception? innerException) : base(message, innerException){}
    }

    public class OpenPositionResult
    {
        public PositionVerification? PosRef { get; set; }
        public string? Problem { get; set; }
        public OpenPositionResult(PositionVerification posRef)
        {
            PosRef = posRef;
        }
        public OpenPositionResult(string problem)
        {
            Problem = problem;
        }
    }
}