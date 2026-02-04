using b1.Models;

namespace b1.Repositories
{
    public interface IPortfolioRepository
    {
        public Task<Portfolio> GetPortfolioByUsernameAsync(string portfilioId);
        public Task<List<Portfolio>> GetPortfoliosAsync(string userId);
        public Task<bool> PersistPortfolio(Portfolio portfolio);
    }
}