using b1.Main;
using b1.Messages;
using b1.Models;
using b1.Repositories;
using MongoDB.Driver;

namespace b1.Services
{
    public class AssetService
    {
        private ITickerRepository _tickerRepo;
        private MongoChartsDataRepo _chartData;
        private PriceContext _pc;
        public AssetService(ITickerRepository tickerRepo, MongoChartsDataRepo chartRepo, PriceContext pc)
        {
            _tickerRepo = tickerRepo;
            _chartData = chartRepo;
            _pc = pc;
        }
        public async Task<TickerData?> GetTickerBySymbol(string symbol)
        {
            var ticker = await _tickerRepo.GetTickerBySymbol(symbol);
            return ticker;
        }
        public async Task<List<string>> GetTickerSymbolsByType(string tickerType)
        {
            return await _tickerRepo.GetTickerSymbolsByTypeName(tickerType);
        }

        public async Task<List<TickerData>> GetTickers(string symbolCSV)
        {
            return await _tickerRepo.GetTickersFromCSV(symbolCSV);
        }
        public async Task<ChartData?> GetChartData(string symbol)
        {
            return await _chartData.GetChartDataBySymbolName(symbol);
        }

        public async Task<List<TickerData>> GetTickersByCSV(string symbolCSV)
        {
            return await _tickerRepo.GetTickersFromCSV(symbolCSV);
        }
    }
}