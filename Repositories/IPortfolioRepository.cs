using b1.Models;

namespace b1.Repositories
{
    public interface IPortfolioRepository
    {
        public Task<Portfolio?> GetPortfolioByIdAsync(string portfilioId);
        public Task<List<Portfolio>> GetPortfoliosForUserAsync(string userId);
        public Task<bool> AddPortfolioAsync(Portfolio portfolio);
    }
}