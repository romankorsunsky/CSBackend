
using b1.Main;
using b1.Models;
using b1.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;

namespace b1.Controllers
{
    [ApiController]
    [Produces("application/json")]
    public abstract class AssetBaseController : ControllerBase
    {
        private PriceContext _priceContext;
        private AssetService _assetService;
        private IMongoDatabase _db { get; init; }
        protected abstract string AssetTypeName { get;}
        public AssetBaseController(IMongoDatabase dbInstance, PriceContext ctx,AssetService assetService)
        {
            _assetService = assetService;
            _db = dbInstance;
            _priceContext = ctx;
        }
        [HttpGet]
        [Route("symbolnames")]
        public async Task<ActionResult<IList<string>>> GetTickerNames()
        {
            var tickerNames = await _assetService.GetTickerSymbolsByType(AssetTypeName);
            if (tickerNames.Count == 0)
                return NotFound();
            tickerNames.Sort(); //sorted for consistent result
            return Ok(tickerNames);
        }

        [HttpGet]
        [Route("{symbol}")]
        public async Task<ActionResult<TickerData>> GetTicker(string symbol)
        {
            var res = await _assetService.GetTickerBySymbol(symbol);
            if (res != null)
            {
                return Ok(res);
            }
            return NotFound();
        }

        [LoggingEnabled]
        [HttpGet]
        [Route("{symbol}/history")]
        public async Task<ActionResult<ChartData>> GetChartData([FromRoute] string symbol)
        {
            ChartData? chData = null;
            chData = await _assetService.GetChartData(symbol);
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
            var res = _priceContext.GetTimedPrice(symbol);
            if (res == null)
            {
                return NotFound();
            }
            return Ok(res);
        }
        [HttpGet]
        [Route("prices/{symbolCSV}")]
        public ActionResult<List<TimedPriceWithSymbol>> GetTickersListPrices([FromRoute] string symbolCSV)
        {
            if (symbolCSV == null)
                return StatusCode(StatusCodes.Status400BadRequest);
            List<TimedPriceWithSymbol> priceList = new List<TimedPriceWithSymbol>();
            var symbolList = symbolCSV.Split(",");
            foreach (var s in symbolList)
            {
                TimedPrice? tp = _priceContext.GetTimedPrice(s);
                if (tp == null)
                    continue;
                priceList.Add(new TimedPriceWithSymbol(tp.Price, tp.Date, s));
            }
            return priceList;
        }       

        [HttpGet]
        [Route("symbols/{symbolCSV}")]
        public async Task<ActionResult<List<TickerData>>> GetTickers([FromRoute] string symbolCSV)
        {
            var tickerData = await _assetService.GetTickersByCSV(symbolCSV);
            if (tickerData.Count == 0)
                return NotFound();
            return Ok(tickerData);
        }       
    }   
}