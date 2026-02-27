using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using b1.DTOs;
using b1.Main;
using b1.Models;
using b1.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace b1.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/v1/portfolios")]
    [Produces("application/json")]
    public class PortfolioController : ControllerBase
    {
        private PortfolioService _portfolioServ { get; init; } = null!;
        public PortfolioController(PortfolioService portServ)
        {
            _portfolioServ = portServ;
        }

        [HttpGet]
        [Route("/ids")]
        public async Task<ActionResult<IEnumerable<string>>> GetPortfoliosIds()
        {
            var principal = User;
            var portfolioIds = await _portfolioServ.GetPortfolioIdsForUser(principal);
            if (portfolioIds.Count == 0)
            {
                return NotFound();
            }
            return Ok(portfolioIds);
        }
        
        [HttpGet]
        public async Task<ActionResult<List<PortfolioDTO>>> GetPortfolios()
        {
            var portfolios = await _portfolioServ.GetPortfoliosForUserAsync(User);
            return Ok(portfolios);
        }

        [HttpPost]
        public async Task<ActionResult<PortfolioDTO>> CreatePortfolio([FromBody] PortfolioCreationRequest req)
        {
            var principal = User;
            string? ptfId;
            PortfolioDTO? dto = null;
            try
            {
                ptfId = await _portfolioServ.CreatePortfolio(req, principal);
                if (ptfId == null)
                {
                    return NotFound();
                }
                dto = await _portfolioServ.GetPortfolioById(ptfId);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            return Ok(dto);
        }
    }
}