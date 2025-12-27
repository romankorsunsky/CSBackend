

using b1.DataTransferObjects;
using Microsoft.AspNetCore.Mvc;

namespace b1.Controllers
{
    [ApiController]
    [Route("api/v1/stocks")]
    [Produces("application/json")]
    public class StockController : ControllerBase
    {

        
        private readonly IList<string> tickerNames = new List<string>() { "AAPL", "NVDA", "YOMAMA" };
        [HttpGet]
        [Route("tickers")]
        public ActionResult<IList<string>> GetTickerNames()
        {
            return Ok(tickerNames);
        }

        public ActionResult<IList<StockDTO>> GetTickers()
        {
            return new List<StockDTO>(){new()};
        }
    }
}