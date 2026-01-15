using b1.Models;

namespace b1.Repositories
{
    public interface IPortfolioRepository
    {
        public Task<Portfolio> GetPortfolioByUsername(string username,string portfilioId);
        public Task<bool> PersistPortfolio(Portfolio portfolio);
    }
}