
using System.Data.Common;
using System.Threading.Tasks;
using b1.Main;
using b1.Models;
using b1.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace b1.Controllers
{
    
    [Route("api/v1/tickers/stock")]
    public class StockController : AssetBaseController
    {
        protected override string AssetTypeName { get => "stock"; }
        public StockController(IMongoDatabase dbInstance, PriceContext ctx,AssetService assetService):
            base(dbInstance,ctx,assetService)
        {
           
        }
    }
}