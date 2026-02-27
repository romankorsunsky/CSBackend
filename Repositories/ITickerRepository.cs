using b1.Models;

namespace b1.Repositories
{
    public interface ITickerRepository
    {
        public Task AddTicker(TickerData ticker);
        public Task DeleteTickerBySymbol(string tickerSymbol);
        public Task<List<string>> GetTickerSymbolsByType(string tickerType);
        public Task<TickerData?> GetTickerBySymbol(string symbol);
        public Task<List<TickerData>> GetTickersByCSV(string symbolCSV);
    }
}