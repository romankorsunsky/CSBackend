
using b1.Models;
using MongoDB.Driver;

namespace b1.Repositories
{
    public class MongoTickerRepo : ITickerRepository
    {
        private IMongoCollection<TickerData> _tickersRepo { get; set; }
        public MongoTickerRepo(IMongoDatabase db)
        {
            _tickersRepo = db.GetCollection<TickerData>("tickers");
        }
        public Task AddTicker(TickerData ticker)
        {
            throw new NotImplementedException();
        }

        public Task DeleteTickerBySymbol(string tickerSymbol)
        {
            throw new NotImplementedException();
        }
        public async Task<List<TickerData>> GetTickersByCSV(string symbolCSV)
        {
            List<string> names = symbolCSV.Split(",").ToList();
            List<TickerData> tickerData = [];
            if (names.Count == 0)
                return tickerData;
            var filter = Builders<TickerData>.Filter.In(td => td.Symbol, names);

            var tickersCursor = await _tickersRepo.FindAsync(filter);
            tickerData = await tickersCursor.ToListAsync();
            return tickerData;
        }
        public async Task<TickerData?> GetTickerBySymbol(string symbol)
        {
            var doc = await _tickersRepo.Find(s => s.Symbol == symbol).FirstOrDefaultAsync();
            return null;
        }
        public async Task<List<string>> GetTickerSymbolsByType(string tickerType)
        {
            var lst = new List<string>();
            var cursor = await _tickersRepo.FindAsync(t => t.TickerType == tickerType);
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