using b1.Models;
using MongoDB.Driver;

namespace b1.Repositories
{
    public class MongoPortfolioRepo : IPortfolioRepository
    {
        private IMongoCollection<Portfolio> _portfolioCollection { get; init; } = null!;
        public MongoPortfolioRepo(IMongoDatabase db)
        {
            _portfolioCollection = db.GetCollection<Portfolio>("portfolios");
        }
        public async Task<Portfolio?> GetPortfolioByUsernameAsync(string portfolioId)
        {
            var results = await _portfolioCollection.FindAsync(p => p.Id == portfolioId);
            var portf = await results.FirstOrDefaultAsync();
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
                await _portfolioCollection.InsertOneAsync(portfolio);
                return true;
            }
            catch (MongoWriteException e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }
        public async Task<List<Portfolio>> GetPortfoliosAsync(string userId)
        {
            return await _portfolioCollection.Find(p => p.OwnerId == userId).ToListAsync();
        }
    }
}