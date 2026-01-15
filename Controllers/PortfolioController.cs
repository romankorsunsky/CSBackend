using System.Security.Principal;
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
        private PortfolioService PortfolioServ { get; init; } = null!;
        public PortfolioController(PortfolioService portServ)
        {
            PortfolioServ = portServ;
        }

        [HttpGet]
        [Route("{username}")]
        public ActionResult<IEnumerable<string>> GetPortfoliosForUser([FromRoute] string username)
        {
            var principal = User.Identity?.Name ?? User.FindFirst("sub")?.Value;
            if (principal != username)
                return StatusCode(StatusCodes.Status401Unauthorized);
            List<string> portfolios = new List<string>();
            portfolios.AddRange(new[] { "Portfolio1", "Portfolio2" });
            return Ok(portfolios);
        }
    }
}