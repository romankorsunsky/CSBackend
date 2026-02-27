using System.Globalization;
using System.Security.Claims;
using b1.Main;
using b1.Models;
using b1.Repositories;
using b1.Respositories;
using MongoDB.Bson;

namespace b1.Services
{
    public class ReportService
    {
        private IUserRepository _userRepo;
        private IPositionRepository _posRepo;
        private IPortfolioRepository _ptfRepo;
        private PriceContext _priceCtx;

        public ReportService(IUserRepository userRepo, IPositionRepository posRepo,
            IPortfolioRepository ptfRepo, PriceContext priceCtx)
        {
            _userRepo = userRepo;
            _posRepo = posRepo;
            _ptfRepo = ptfRepo;
            _priceCtx = priceCtx;
        }

        public async Task<UserReport?> GetUserReport(ClaimsPrincipal cp)
        {
            string? userId = cp.FindFirst("sub")?.Value;
            if (userId == null)
            {
                return null;
            }
            User? usr = await _userRepo.GetUserById(userId);
            if (usr == null)
            {
                return null;
            }
            var dateNow = DateTime.UtcNow;
            string year = dateNow.Year.ToString();
            DateTime dt;
            if (!DateTime.TryParseExact(year, "yyyy", CultureInfo.InvariantCulture,
                DateTimeStyles.AdjustToUniversal, out dt))
            {
                return null;
            }
            DateTime utcStart = DateTime.SpecifyKind(dt, DateTimeKind.Utc);
            double balance = usr.Balance;
            List<Portfolio> ptfs = await _ptfRepo.GetPortfoliosForUserAsync(userId);
            List<Position> positions = new List<Position>();
            foreach (var ptf in ptfs)
            {
                var positionsForPtf = await _posRepo.GetOpenPositionsByPtfFromDate(ptf.Id, utcStart);
                positions.AddRange(positionsForPtf);
            }
            var lst = new List<AssetGain>();
            var assetToCurrentWorth = new Dictionary<string, double>();
            var totalPtfWorth = 0.0;
            int shortCount = 0, longCount = 0;
            foreach (var pos in positions)
            {

                TimedPrice? tp = _priceCtx.GetTimedPrice(pos.AssetSymbol);
                if (tp == null)
                {
                    continue; //maybe better to throw error and log, cuz the report will be incomplete
                }
                double currPrice = tp.Price;
                var currSum = assetToCurrentWorth.GetValueOrDefault(pos.AssetSymbol);
                if (pos.PositionType == PositionDirection.SHORT)
                {
                    shortCount++;
                    currSum -= currPrice;
                }
                else
                {
                    longCount++;
                    currSum += currPrice;
                }

                assetToCurrentWorth[pos.AssetSymbol] = currSum;
            }
            foreach (var sym in assetToCurrentWorth.Keys)
            {
                totalPtfWorth += assetToCurrentWorth[sym];
                lst.Add(new AssetGain(sym, assetToCurrentWorth[sym]));
            }
            var report = new UserReport(balance, totalPtfWorth, shortCount, longCount, lst);
            return report;
        }
    }
    public class UserReport
    {
        public List<AssetGain> PerAssetGain { get; set; } = null!;
        public double CurrentBalance { get; set; }
        public double TotalPortfoliosWorth { get; set; }
        public int OpenShortPositionsCount { get; set; }
        public int OpenLongPositionsCount { get; set; }

        public UserReport(double currentBalance, double totalPtfWorth, int openShortCount,
            int openLongCount, List<AssetGain> assetGains)
        {
            CurrentBalance = currentBalance;
            TotalPortfoliosWorth = totalPtfWorth;
            OpenShortPositionsCount = openShortCount;
            OpenLongPositionsCount = openLongCount;
            PerAssetGain = assetGains;
        }
    }
    public struct AssetGain
    {
        public string AssetName { get; set; }
        public double Gain { get; set; }
        public AssetGain(string name, double gain)
        {
            AssetName = name;
            Gain = gain;
        }
    }
}