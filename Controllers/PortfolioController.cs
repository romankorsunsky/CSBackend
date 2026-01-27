using System.Security.Principal;
using System.Threading.Tasks;
using b1.Models;
using b1.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace b1.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/v1/portfolio")]
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
        [HttpPost]
        [Route("regular")]
        public async Task<ActionResult> CreateRegularPortfolio([FromBody]RegularPortfolioCreationRequest req)
        {
            //portfolios are crated rarely, so performance of true/false result vs try/catch for business logic 
            //is ngeligible ? negligable ? 
            var principal = User;
            try
            {
                await _portfolioServ.CreatePortfolio(req, principal);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            return Ok();
        }
        [HttpPost]
        [Route("advanced")]
        public async Task<ActionResult> CreateAdvancedPortfolio([FromBody] AdvancedPortfolioCreationRequest req)
        {
            var principal = User;
            try
            {
                await _portfolioServ.CreatePortfolio(req, principal);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            return Ok();
        }
    }
}