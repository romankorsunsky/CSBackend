
using System.Data.Common;
using b1.Main;
using b1.Models;
using b1.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace b1.Controllers
{
    [ApiController]
    [Route("api/v1/tickers/stock")]
    [Produces("application/json")]
    public class StockController : ControllerBase
    {
        private PriceContext? Ctx;

        private PriceContextHolder CtxHolder { get; init; }
        private IMongoDatabase Db { get; init; }
        const string AssetTypeName = "stock";
        const string TickerColName = "tickers";
        public StockController(IMongoDatabase dbInstance, PriceContextHolder ctxHolder)
        {
            Db = dbInstance;
            CtxHolder = ctxHolder;
            Ctx = CtxHolder.GetContext(AssetTypeName);
        }

        [HttpGet]
        [Route("symbolnames")]
        [ProducesResponseType<IList<string>>(StatusCodes.Status200OK)]
        [ProducesResponseType<IList<string>>(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public ActionResult<IList<string>> GetTickerNames()
        {
            //another option here is to use TaskCompletionSource<bool> and signal from the PriceService that we can run
            //but I'm not sure abt the mechanics of what the HTTP request component does when all the pent up requests will 
            //suddenly continue executing here, what if there are hundreds of thousands, rather return 503 for now.
            if (Ctx == null)
            {
                Ctx = CtxHolder.GetContext(AssetTypeName);
                return StatusCode(StatusCodes.Status503ServiceUnavailable, null);
            }
            var tickerNames = Ctx.GetSymbolNames();
            tickerNames.Sort();
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
            var tickerCol = Db.GetCollection<TickerData>(TickerColName);
            var res = await tickerCol.Aggregate().Match(t => t.Symbol == symbol).
                    FirstOrDefaultAsync();
            if (res != null)
            {
                return Ok(res);
            }
            return NotFound();
        }

        [HttpGet]
        [Route("{symbol}/history")]
        public async Task<ActionResult<ChartData>> GetChartData([FromRoute] string symbol)
        {
            ChartData chData = null!;
            var col = Db.GetCollection<ChartData>(ProcessAssetBase.CHART_HIS_COL);
            var results = await col.FindAsync(ch => ch.Symbol == symbol);
            chData = results.FirstOrDefault();
            if (chData == null)
            {
                return StatusCode(StatusCodes.Status204NoContent, null);
            }
            return Ok(chData);
        }

        [HttpGet]
        [Route("{symbol}/price")]
        public ActionResult<TimedPrice> GetSymbolPrice([FromRoute] string symbol)
        {
            if (Ctx == null)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, null);
            }
            var res = Ctx.GetTimedPrice(symbol);
            if (res == null)
            {
                return NotFound();
            }
            return Ok(res);
        }

        //rewrite this, move the CSV from Route, add it as a query, or add 
        [HttpGet]
        [Route("symbols/{symbolCSV}")]
        public async Task<ActionResult<List<TickerData>>> GetTickers([FromRoute] string symbolCSV)
        {
            List<string> names = symbolCSV.Split(",").ToList();
            List<TickerData> tickerData = [];
            if (names.Count == 0)
                return Ok(tickerData);
            var col = Db.GetCollection<TickerData>(TickerColName);
            var filter = Builders<TickerData>.Filter.In(td => td.Symbol, names);

            var tickers = await col.FindAsync(filter);
            tickerData = await tickers.ToListAsync();
            return Ok(tickerData);
        }
    }
}