
using b1.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace b1.Controllers
{
    [ApiController]
    [Produces("application/json")]
    [Route("api/v1/reports")]
    [Authorize]
    public class ReportsController : ControllerBase
    {
        private ReportService _reportService;
        public ReportsController(ReportService reportService)
        {
            _reportService = reportService;
        }

        [LoggingEnabled]
        [HttpGet]
        [Route("annual")]
        public async Task<ActionResult<UserReport>> GetReportForUser()
        {
            UserReport? rep = null;
            try
            {
                rep = await _reportService.GetUserReport(User);
                return Ok(rep);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }    
}
