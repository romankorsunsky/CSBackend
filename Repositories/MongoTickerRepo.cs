using b1.Models;
using MongoDB.Driver;
using ScottPlot;

namespace b1.Repositories
{
    public class MongoTickerRepo: ITickerRepository
    {
        private IMongoCollection<TickerData> _tickers;
        private IMongoDatabase _db;
        public MongoTickerRepo(IMongoDatabase db)
        {
            _db = db;
            _tickers = db.GetCollection<TickerData>("tickers");
        }
        public async Task AddTicker(TickerData ticker)
        {
            await _tickers.InsertOneAsync(ticker);
        }

        public async Task<TickerData?> GetTickerBySymbol(string symbol)
        {
            var doc = await _tickers.Find(s => s.Symbol == symbol).FirstOrDefaultAsync();
            return null;
        }
        public async Task<List<TickerData>> GetTickersFromCSV(string symbolCSV)
        {
            List<string> names = symbolCSV.Split(",").ToList();
            List<TickerData> tickerData = [];
            if (names.Count == 0)
                return tickerData;
            var filter = Builders<TickerData>.Filter.In(td => td.Symbol, names);

            var tickersCursor = await _tickers.FindAsync(filter);
            tickerData = await tickersCursor.ToListAsync();
            return tickerData;
        }
        public async Task<List<string>> GetTickerSymbolsByTypeName(string tickerType)
        {
            var lst = new List<string>();
            var cursor = await _tickers.FindAsync(t => t.TickerType == tickerType);
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
    }
}