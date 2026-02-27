using b1.Main;
using b1.Models;
using b1.Respositories;
using MongoDB.Driver;

namespace b1.Repositories
{
    public class MongoPositionRepo : IPositionRepository
    {
        private IMongoCollection<Position> _positionsCol;
        public MongoPositionRepo(IMongoDatabase db)
        {
            _positionsCol = db.GetCollection<Position>("positions");
        }
        public async Task AddPostion(Position position)
        {
            try
            {
                await _positionsCol.InsertOneAsync(position);
            }
            catch (Exception)
            {
                throw new WriteException("Couldn't add position");
            }
        }

        public async Task UpdatePositionStatus(string positionId)
        {
            var update = Builders<Position>.Update.Set(pos => pos.Closed, true);
            await _positionsCol.UpdateOneAsync(pos => pos.Id == positionId, update);
        }

        public async Task<DeleteResult> DeletePosition(string positionId)
        {
            try
            {
                return await _positionsCol.DeleteOneAsync(p => p.Id == positionId);
            }
            catch (Exception)
            {
                throw new WriteException("[Couldn't delelet position with Id]" + positionId);
            }            
        }

        public async Task DeletePositionsForPortfolio(string portfolioId)
        {          
            await _positionsCol.DeleteManyAsync(p => p.PortfolioId == portfolioId);             
        }

        public async Task<List<Position>> GetOpenPositionsByPtfFromDate(string portfolioId, DateTime dt)
        {
            var date = DateTime.SpecifyKind(dt, DateTimeKind.Utc);
            var filter1 = Builders<Position>.Filter.Gte(pos => pos.OpenedAt, date);
            var filter2 = Builders<Position>.Filter.Eq(pos => pos.PortfolioId, portfolioId);
            var filter3 = Builders<Position>.Filter.Eq(pos => pos.Closed, false);
            var combinedFilter = Builders<Position>.Filter.And(filter1, filter2, filter3);
            var res = await _positionsCol.Find(combinedFilter).ToListAsync();
            return res;
        }

        public async Task<Position?> GetPosition(string positionId)
        {
            try
            {
                return await _positionsCol.Find(p => p.Id == positionId).FirstOrDefaultAsync();
            }
            catch (Exception)
            {
                throw new WriteException("[Couldn't find position with Id]: " + positionId);
            }                  
        }

        public async Task<List<Position>> GetPositionsByPortfolio(string portfolioId)
        {
            List<Position> posList;
            posList = await _positionsCol.Find(p => p.PortfolioId == portfolioId).ToListAsync();
            return posList;
        }
    }
}