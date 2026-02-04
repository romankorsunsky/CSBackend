using b1.Main;
using b1.Messages;
using b1.Models;
using b1.Repositories;
using MongoDB.Driver;

namespace b1.Services
{
    public class AssetService
    {
        private IMongoCollection<TickerData> _tickersCollection;
        private MongoChartsDataRepo _chartData;
        private PriceContext _pc;
        public AssetService(IMongoDatabase db, MongoChartsDataRepo chartRepo, PriceContext pc)
        {
            _tickersCollection = db.GetCollection<TickerData>("tickers");
            _chartData = chartRepo;
            _pc = pc;
        }
        public async Task<TickerData?> GetTickerBySymbol(string symbol)
        {
            var doc = await _tickersCollection.Find(s => s.Symbol == symbol).FirstOrDefaultAsync();
            return null;
        }
        public async Task<List<string>> GetTickerSymbolsByType(string tickerType)
        {
            var lst = new List<string>();
            var cursor = await _tickersCollection.FindAsync(t => t.TickerType == tickerType);
            while (await cursor.MoveNextAsync())
            {
                var curr = cursor.Current;
                foreach (var ticker in curr)
                {
                    lst.Add(ticker.Symbol);
                }
            }
            return lst;
        }

        public async Task<List<TickerData>> GetTickersByCSV(string symbolCSV)
        {
            List<string> names = symbolCSV.Split(",").ToList();
            List<TickerData> tickerData = [];
            if (names.Count == 0)
                return tickerData;
            var filter = Builders<TickerData>.Filter.In(td => td.Symbol, names);

            var tickersCursor = await _tickersCollection.FindAsync(filter);
            tickerData = await tickersCursor.ToListAsync();
            return tickerData;
        }
        public async Task<ChartData?> GetChartData(string symbol)
        {
            return await _chartData.GetChartDataBySymbolName(symbol);
        }
        
    }
}