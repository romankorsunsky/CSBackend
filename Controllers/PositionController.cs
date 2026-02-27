using System.Security.Claims;
using b1.DTOs;
using b1.Models;
using b1.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace b1.Controllers
{
    [ApiController]
    [Route("api/v1/portfolios")]
    [Produces("application/json")]
    [Authorize]
    public class PositionController : ControllerBase
    {
        private PositionService _posServ;
        public PositionController(PositionService posServ)
        {
            _posServ = posServ;
        }

        [Route("{id}/position-request")]
        [HttpPost]
        public async Task<ActionResult<PositionVerificationDTO>> AddPositionRequest(
            [FromBody] PositionCreationRequest request,
            [FromRoute] string id)
        {
            OpenPositionResult result = await _posServ.RequestToOpenPosition(request, User, id);
            if (result.Problem != null)
            {
                return StatusCode(StatusCodes.Status403Forbidden,result.Problem);
            }
            var posVer = result.PosRef;
            if (posVer == null)
                return StatusCode(StatusCodes.Status500InternalServerError);
            return Ok(new PositionVerificationDTO(posVer));
        }
        [LoggingEnabled]
        [HttpPost]
        [Route("{id}/position-confirmation")]
        public async Task<ActionResult> AddPosition([FromBody] PositionConfirmation posConf,
            [FromRoute] string id)
        {
            if (posConf.Confirmed == false)
                return StatusCode(StatusCodes.Status500InternalServerError);
            PositionDTO? posDTO = await _posServ.OpenPositionFinalize(posConf.VerificationId,User);
            return Ok(posDTO);
        }

        [LoggingEnabled]
        [HttpPost]
        [Route("{ptfId}/position-close/{posId}")]
        public async Task<ActionResult> ClosePosition([FromRoute] string ptfId,
            [FromRoute] string posId)
        {
            try
            {
                var res = await _posServ.ClosePositionAsync(ptfId,posId, User);
                if (res == null)
                    return StatusCode(StatusCodes.Status412PreconditionFailed);
                return Ok();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
