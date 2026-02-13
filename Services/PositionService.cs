using System.Security.Claims;
using b1.Controllers;
using b1.Main;
using b1.Models;
using b1.Repositories;
using b1.Srevices;
using MongoDB.Driver;

namespace b1.Services{
    public class PositionService
    {
        private IMongoCollection<Position> _positionCol;
        private IUserRepository _usrRepo;
        private PriceContext _priceCtx;
        public PositionService(IMongoDatabase db,
            IUserRepository userRepo, PriceContext priceCtx)
        {
            _positionCol = db.GetCollection<Position>("positions");
            _usrRepo = userRepo;
            _priceCtx = priceCtx;
        }
        public async Task<PositionVerification?> VerifyPosition(PositionCreationRequest request, ClaimsPrincipal principal)
        {
            string? userId = principal.FindFirst("sub")?.Value;
            if (userId == null)
            {
                throw new Exception("somehow userId is bad");
            }
            string symbol = request.Symbol;
            long qtty = request.Quantity;
            if (request.PositionType == PositionDirection.LONG)
            {
                double? balanceRes = await _usrRepo.GetUserBalance(userId);
                if (balanceRes == null)
                {
                    return null;
                }
                double balance = balanceRes.Value;
                TimedPrice? tp = _priceCtx.GetTimedPrice(symbol);
                if (tp == null)
                {
                    return null;
                }
                if (tp.Price * qtty > balance)
                {
                    return null;
                }
                PositionVerification confirmation =
                    new PositionVerification(symbol, qtty, tp.Price,DateTime.UtcNow.ToString());
            }
            return null;
        }
    }
}