using b1.Main;
using b1.Models;
using MongoDB.Bson;
using MongoDB.Driver;


namespace b1.Services
{
    public class EtfPriceService : PriceServiceBase
    {
        private Dictionary<string, IValueGenerator> _volumeGenerators;
        public override string AssetType { get; init; } = "etf";
        public EtfPriceService(PriceContext ctx,IMongoDatabase dbInstance,IMessageChannel broker)
         : base(ctx,dbInstance,broker)
        {
            _volumeGenerators = new Dictionary<string, IValueGenerator>();
        }
        protected internal override async Task<AssetEOD> MakeEod(AssetEOD eod,string name)
        {
            if (eod is EtfEOD)
            {
                var typedEod = (EtfEOD)eod;
                var generator = AssetToEODGen[name];
                var newDate = typedEod.Date.Date.AddDays(1); //AssetEOD has Date field, and so does a DateTime
                var open = typedEod.Close;
                var n1 = generator.GetValue(open);
                var n2 = generator.GetValue(typedEod.Open);
                var close = generator.GetValue(typedEod.Close);
                var high = Math.Max(Math.Max(n1, n2), Math.Max(open, close));
                var low = Math.Min(Math.Min(n1, n2), Math.Min(open, close));
                var volume = _volumeGenerators[name].GetValue(typedEod.Volume);
                EtfEOD res = new EtfEOD(name, newDate, open, high, close, low, volume, 0.0, 0.0);
                var col = Db.GetCollection<AssetEOD>(ProcessAssetBase.ASSET_EOD_COL);
                await col.InsertOneAsync(res);
                return res;
            }
            else
            {
                throw new Exception("Some shenanigans with AssetEOD deserialization, expected EtfEOD, got something else.");
            }
        }

        protected internal override void ConfigureGenerators(Dictionary<string,AssetEOD> assetsMap)
        {
            var names = assetsMap.Keys.ToList();
            var priceGenerator = new GBMValueGenerator();
            var volGenerator = new MCRValueGenerator();
            foreach (var name in names)
            {
                if (assetsMap.TryGetValue(name, out var eod))
                {
                    AssetToEODGen.Add(name, priceGenerator);
                    var typedEod = (EtfEOD)eod;
                    volGenerator.WithMean(typedEod.Volume).WithSigma(volGenerator.Sigma * volGenerator.Mean);
                    if (!_volumeGenerators.TryAdd(name, volGenerator))
                        throw new Exception("Couldn't add VolumeGenerator for:" + name);
                }

            }
        }
    }
}