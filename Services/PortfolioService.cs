using System.Security.Claims;
using b1.Models;
using b1.Repositories;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace b1.Services
{
    public class PortfolioService
    {
        private IMongoCollection<Portfolio> _portfolios;
        public PortfolioService(IMongoDatabase db)
        {
            _portfolios = db.GetCollection<Portfolio>("portfolios");
        }

        public async Task<List<string>> GetPortfolioIdsForUser(ClaimsPrincipal principal)
        {
            var username = principal.FindFirst("sub")?.Value;
            var idList = new List<string>();
            //here I know we don't have many portfolios so we can read the list in one go.
            var plist = await _portfolios.Find(p => p.OwnerUsername == username).ToListAsync();
            foreach (var p in plist)
            {
                idList.Add(p.Id);
            }
            return idList;
        }
        public async Task CreatePortfolio(PortfolioCreationRequest portfRequest, ClaimsPrincipal principal)
        {
            var username = principal.FindFirstValue("sub");
            if (username == null)
                throw new Exception("No 'sub' claim, take care");
            //pass portfRequest and username to 
        }
    }
}