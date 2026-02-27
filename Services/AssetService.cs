using b1.Main;
using b1.Messages;
using b1.Models;
using b1.Repositories;
using MongoDB.Driver;

namespace b1.Services
{
    public class AssetService
    {
        private ITickerRepository _tickersRepo;
        private IChartDataRepository _chartData;
        private PriceContext _pc;
        public AssetService(ITickerRepository tickers, IChartDataRepository chartRepo, PriceContext pc)
        {
            _tickersRepo = tickers;
            _chartData = chartRepo;
            _pc = pc;
        }
        public async Task<TickerData?> GetTickerBySymbol(string symbol)
        {
            var doc = await _tickersRepo.GetTickerBySymbol(symbol);
            return null;
        }
        public async Task<List<string>> GetSymbolsByType(string tickerType)
        {
            List<string> lst = await _tickersRepo.GetTickerSymbolsByType(tickerType);
            return lst;
        }

        public async Task<List<TickerData>> GetTickersByCSV(string symbolCSV)
        {

            List<TickerData> tickersData;
            tickersData = await _tickersRepo.GetTickersByCSV(symbolCSV);
            return tickersData;
        }
        public async Task<ChartData?> GetChartData(string symbol)
        {
            return await _chartData.GetChartDataBySymbolName(symbol);
        }
        
    }
}