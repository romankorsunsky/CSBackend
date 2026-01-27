
using b1.Main;
using b1.Models;
using b1.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;

namespace b1.Controllers
{
    
    [Route("api/v1/tickers/fx")]
    public class FxController : AssetBaseController
    {
        protected override string AssetTypeName { get => "etf"; }
        public FxController(IMongoDatabase dbInstance, PriceContext ctx,AssetService assetService):
            base(dbInstance,ctx,assetService)
        {
           
        }
    }   
}