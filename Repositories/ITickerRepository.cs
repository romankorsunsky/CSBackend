using b1.Models;

namespace b1.Repositories
{
    public interface ITickerRepository
    {
        public Task<TickerData?> GetTickerBySymbol(string symbol);
        public Task<List<string>> GetTickerSymbolsByTypeName(string tickerType);
        public Task AddTicker(TickerData ticker);
        public Task<List<TickerData>> GetTickersFromCSV(string symbolCSV);
    }
}