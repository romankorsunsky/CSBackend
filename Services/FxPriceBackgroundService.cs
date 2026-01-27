using b1.Main;
using b1.Models;
using b1.Services;
using Microsoft.VisualBasic;
using MongoDB.Driver;

namespace sadna.Services
{
    public class FxPriceBackgroundService : PriceBackgroundServiceBase
    {
        public override string AssetType { get; init; } = "fx";
        
        public FxPriceBackgroundService(PriceContext ctx,IMongoDatabase dbInstance,IMessageChannel broker)
         : base(ctx,dbInstance,broker) {}

        protected internal override void ConfigureGenerators(Dictionary<string,AssetEOD> assetsMap)
        {
            var names = assetsMap.Keys.ToList();
            
            foreach (var name in names)
            {
                if (assetsMap.TryGetValue(name, out var eod))
                {
                    AssetToEODGen[name] = new MCRValueGenerator().WithMean(eod.Open); 
                }
            }
        }
        protected internal override async Task<AssetEOD> MakeEod(AssetEOD eod,string name)
        {
            if (eod is ForexEOD)
            {
                var typedEod = (ForexEOD)eod;
                var generator = AssetToEODGen[name];
                var newDate = typedEod.Date.Date.AddDays(1); //AssetEOD has Date field, and so does a DateTime
                var open = typedEod.Close;
                var n1 = generator.GetValue(open);
                var n2 = generator.GetValue(typedEod.Open);
                var close = generator.GetValue(typedEod.Close);
                var high = Math.Max(Math.Max(n1, n2), Math.Max(open, close));
                var low = Math.Min(Math.Min(n1, n2), Math.Min(open, close));
                ForexEOD res = new ForexEOD(name, newDate, open, high, close, low);
                var col = Db.GetCollection<AssetEOD>(ProcessAssetBase.ASSET_EOD_COL);
                await col.InsertOneAsync(res);
                return res;
            }
            else
            {
                throw new Exception("Some shenanigans with AssetEOD deserialization, expected ForexEOD, got something else.");
            }
        }
    }
}