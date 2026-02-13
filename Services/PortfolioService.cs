using System.Collections;
using System.Security.Claims;
using b1.Main;
using b1.Models;
using b1.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages.Infrastructure;
using Microsoft.OpenApi.Extensions;
using MongoDB.Bson;
using MongoDB.Driver;

namespace b1.Services
{
    public class PortfolioService
    {
        private IMongoCollection<Portfolio> _portfolios; //I should use repositories, but I barely have code here.
        private IMongoCollection<Position> _positions;
        public PortfolioService(IMongoDatabase db)
        {
            _portfolios = db.GetCollection<Portfolio>("portfolios");
            _positions = db.GetCollection<Position>("positions");
        }
        /// <summary>
        /// Returns a list of Portfolios for the user.
        /// </summary>
        /// <param name="userId">Here for some reason I decided</param>
        /// <returns></returns>
        public async Task<List<PortfolioDTO>> GetPortfoliosForUserAsync(ClaimsPrincipal principal)
        {
            var userId = principal.FindFirst("sub")?.Value;
            var portfolios = await _portfolios.Find(p => p.OwnerId == userId).ToListAsync();

            var portfolioDTOs = new List<PortfolioDTO>(portfolios.Count);
            //select id's
            var portfolioIds = new List<string>();
            portfolios.ForEach(p => portfolioIds.Add(p.Id));
            //find all positions belonging to the portfolio's the user has
            var positionFilter = Builders<Position>.Filter.In(pos => pos.PortfolioId, portfolioIds);
            var cursor = await _positions.FindAsync(positionFilter); //cursor of ALL positions belonging to the user
            var dict = portfolioIds.ToDictionary(s => s, s => new List<PositionDTO>());
            //there may be a lot of positions so let's use cursor and group positions by their owning portfolio
            while (await cursor.MoveNextAsync())
            {
                foreach (var pos in cursor.Current)
                {
                    dict[pos.PortfolioId].Add(new PositionDTO(pos));
                }
            }
            //for each Portfolio and the corresponding group of positions, generate a portfolioDTO to send back.
            foreach (Portfolio p in portfolios)
            {
                if (dict.TryGetValue(p.Id, out var positionList))
                {
                    portfolioDTOs.Add(new PortfolioDTO(p, positionList));
                }
                else
                {
                    portfolioDTOs.Add(new PortfolioDTO(p, new List<PositionDTO>(0)));
                }
            }
            return portfolioDTOs;
        }
        /// <summary>
        /// Returns a list of Id's of the Portfolios belonging to the User
        /// </summary>
        /// <param name="principal">Principal object for user identification</param>
        /// <returns></returns>
        public async Task<List<string>> GetPortfolioIdsForUser(ClaimsPrincipal principal)
        {
            var ownerId = principal.FindFirst("sub")?.Value;
            var idList = new List<string>();
            //here I know we don't have many portfolios so we can read the list in one go nad not use cursor
            var plist = await _portfolios.Find(p => p.OwnerId == ownerId).ToListAsync();
            foreach (var p in plist)
            {
                idList.Add(p.Id);
            }
            return idList;
        }
    
        /// <summary>
        /// Attempts to create a portfolio, and insert it into the repository.
        /// On success returns the Id of the new Portfolio.
        /// </summary>
        /// <param name="portfRequest">A request for a portfolio</param>
        /// <param name="principal">The principal object which will let the service figure out the owner
        /// of the portfolio</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<string?> CreatePortfolio(PortfolioCreationRequest portfRequest, ClaimsPrincipal principal)
        {
            var ownerId = principal.FindFirstValue("sub");
            if (ownerId == null)
                throw new Exception("No 'sub' claim, take care");
            Portfolio? ptf = null;
            switch (portfRequest.PtfType)
            {
                case "REGULAR":
                    ptf = new RegularPortfolio(ownerId, portfRequest.DisplayName);
                    break;
                case "ADVANCED":
                    ptf = new AdvancedPortfolio(ownerId, portfRequest.DisplayName);
                    break;
            }
            if (ptf == null)
            {
                return null;
            }
            try
            {
                await _portfolios.InsertOneAsync(ptf);
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to insert new Portfolio: \n" + e.Message);
            }
            return ptf.Id;
        }

        public async Task<PortfolioDTO?> GetPortfolioById(string ptfId)
        {
            try
            {
                var ptf = await _portfolios.Find(p => p.Id == ptfId).FirstOrDefaultAsync();
                if (ptf == null)
                {
                    return null;
                }
                //to be honest, I should make sure I created a composite key index on (symbol,portfolioId)
                var positions = await _positions.Find(p => p.PortfolioId == ptf.Id).ToListAsync();
                List<PositionDTO> positionDtoList = new List<PositionDTO>();
                foreach (var position in positions)
                {
                    positionDtoList.Add(new PositionDTO(position));
                }
                return new PortfolioDTO(ptf, positionDtoList);
            }
            catch (Exception e)
            {
                //I should maybe log errors instead of consuming them and returning a non descriptive 'null'.
                //but I have to think about, validation, implememting refresh tokens, need to refactor to adhere to RESTful
                //and much more, so I will let it go this time.
                return null;
            }
        }
    }
}