using System.Security.Claims;
using b1.Models;
using b1.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace b1.Controllers
{
    [ApiController]
    [Route("api/v1/portfolio")]
    [Produces("application/json")]
    //[Authorize]
    public class PositionController : ControllerBase
    {
        private PositionService _posServ;
        public PositionController(PositionService posServ)
        {
            _posServ = posServ;
        }

        
        [LoggingEnabled]
        [Route("{id}/position-request")]
        [HttpPost]
        public async Task<ActionResult<PositionVerification>> AddPositionRequest(PositionCreationRequest request)
        {
            PositionVerification? confirmation = await _posServ.VerifyPosition(request, User);
            if (confirmation == null)
            {
                return StatusCode(StatusCodes.Status403Forbidden);
            }
            return Ok(confirmation);
        }
        [HttpGet]
        [Route("getok")]
        public ActionResult GetOk()
        {
            return Ok();
        }

        
        [LoggingEnabled]
        [Route("get-confirmation")]
        [HttpGet]
        public async Task<ActionResult<PositionVerification>> TestHandler()
        {
            var verification = new PositionVerification("AAPL", 123, 123.1, DateTime.UtcNow.ToString());
            return Ok(verification);
        }

        [LoggingEnabled]
        
        [HttpPost]
        [Route("positionconfirmation")]
        public async Task<ActionResult> AddPosition(PositionVerification posConf)
        {
            return Ok();
        }

    }
    public class PositionVerification
    {
        public string Id;
        public string Symbol { get; set; } = null!;
        public long Quantity { get; set; }
        public double Price { get; set; }
        public string TimeIssued { get; set; } = null!;

        public PositionVerification(string symbol, long quantity, double price, string timeIssued)
        {
            Symbol = symbol;
            Quantity = quantity;
            Price = price;
            TimeIssued = timeIssued;
        }
    }
    public class PositionConfirmation
    {
        public PositionVerification Verification { get; set; } = null!;
        public bool Decision { get; set; }

        public PositionConfirmation(PositionVerification verification, bool decision) {
            Verification = verification;
            Decision = decision;
        }
    }
}
