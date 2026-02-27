using b1.Models;
using MongoDB.Driver;

namespace b1.Respositories{
    
    public interface IPositionRepository
    {
        /// <summary>
        /// <exception cref="WriteException">Thrown on any error</exception>
        /// </summary>
        public Task AddPostion(Position position);
        /// <summary>
        /// <exception cref="WriteException">Thrown on any error</exception>
        /// </summary>
        public Task<Position?> GetPosition(string positionId);

        /// <summary>
        /// Get all positions for portfolio
        /// </summary>
        /// <exception cref="WriteException">Thrown on any error</exception>
        /// <paramref name="portfolioId"/>
        public Task<List<Position>> GetPositionsByPortfolio(string portfolioId);
        /// <summary>
        /// Get
        /// </summary>
        /// <param name="portfolioId"></param>
        /// <param name="dt"></param>
        /// <returns>List of Positions for the portfolioId</returns>
        public Task<List<Position>> GetOpenPositionsByPtfFromDate(string portfolioId, DateTime dt);
        /// <summary>
        /// Deletes a position by Id
        /// </summary>
        /// <param name="positionId">Id of the Position</param>
        /// <returns>Task</returns>
        public Task<DeleteResult> DeletePosition(string positionId);
        /// <summary>
        /// Sets the corresponding position's status to CLOSED
        /// </summary>
        /// <param name="positionId">Id of the Position</param>
        public Task UpdatePositionStatus(string positionId);
        public Task DeletePositionsForPortfolio(string portfolioId);
    }
}