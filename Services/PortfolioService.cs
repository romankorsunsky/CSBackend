using b1.Repositories;

namespace b1.Services
{
    public class PortfolioService
    {
        private IPortfolioRepository PortfolioRepo { get; init; } = null!;
        public PortfolioService(IPortfolioRepository portfolioRepo)
        {
            PortfolioRepo = portfolioRepo;
        }
    }
}