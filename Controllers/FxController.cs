using b1.Main;
using b1.Models;
using b1.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using MongoDB.Driver;

namespace b1.Controllers
{   
    [ApiController]
    [Produces("application/json")]
    [Route("api/v1/fxs")]
    public class FxController : ControllerBase
    {
        private PriceContext _ctx = null!;

        private PriceContextHolder _ctxHolder { get; init; }
        private IMongoDatabase _db { get; init; }
        const string ASSET_TYPE_NAME = "stock";
        const string TICKER_COL_NAME = "tickers";
        public FxController(IMongoDatabase dbInstance, PriceContextHolder ctxHolder)
        {
            _db = dbInstance;
            _ctxHolder = ctxHolder;
        }

        [HttpGet]
        [Route("symbols")]
        [ProducesResponseType<IList<string>>(StatusCodes.Status200OK)]
        [ProducesResponseType<IList<string>>(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public ActionResult<IList<string>> GetTickerNames()
        {   //another option here is to use TaskCompletionSource<bool> and signal from the PriceService that we can run
            //but I'm not sure abt the mechanics of what the HTTP request component does when all the pent up requests will 
            //suddenly continue executing here, what if there are hundreds of thousands, rather return 503 for now.
            var initialized = _ctxHolder.GetContext(ASSET_TYPE_NAME);
            if (initialized == null)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, null);
            }
            var tickerNames = initialized.GetSymbolNames();
            return Ok(tickerNames);
        }

        [HttpGet]
        [Route("{symbol}")]
        public async Task<ActionResult<TickerData>> GetTicker(string symbol)
        {
            if (symbol == null || symbol == "")
            {
                return StatusCode(StatusCodes.Status400BadRequest, null);
            }
            var tickerCol = _db.GetCollection<TickerData>(TICKER_COL_NAME);
            var res = await tickerCol.Aggregate().Match(t => t.Symbol == symbol).
                    FirstOrDefaultAsync();
            if (res != null)
            {
                return Ok(res);
            }
            return NotFound();
        }
    }
}