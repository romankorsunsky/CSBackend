using b1.Models;
using MongoDB.Driver;

namespace b1.Repositories
{
    public class MongoPortfolioRepo : IPortfolioRepository
    {
        private IMongoCollection<Portfolio> PortfolioCollection { get; init; } = null!;
        public MongoPortfolioRepo(IMongoDatabase db)
        {
            db.CreateCollection("portfolios");
            PortfolioCollection = db.GetCollection<Portfolio>("portfolios");
        }
        public async Task<Portfolio?> GetPortfolioByUsername(string username, string portfolioId)
        {
            var results = await PortfolioCollection.FindAsync(p => p.OwnerUsername == username);
            var portf = await results.FirstOrDefaultAsync();
            if (portf is null)
            {
                return null;
            }
            return portf;
        }

        /// <summary>
        /// Assumes portfolio is validated prior
        /// </summary>
        /// <param name="portfolio"></param>
        /// <returns>returns true on succes, false on failure to persist</returns>
        public async Task<bool> PersistPortfolio(Portfolio portfolio)
        {
            try
            {
                await PortfolioCollection.InsertOneAsync(portfolio);
                return true;
            }
            catch (MongoWriteException e)
            {
                return false;
            }
        }
    }
}