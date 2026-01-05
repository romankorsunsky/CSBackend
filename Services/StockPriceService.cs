
using System.Threading.Tasks;
using b1.Main;
using b1.Models;
using b1.Services;
using MongoDB.Bson;
using MongoDB.Driver;

namespace sadna.Services
{
    public class StockPriceService: PriceServiceBase
    {
        private bool _addedMCRV = false;
        private Dictionary<string, IValueGenerator> _volumeGenerators;
        public override string AssetType { get; init; } = "stock";

        public StockPriceService(PriceContextHolder ctx,IMongoDatabase dbInstance) : base(ctx,dbInstance) {
            _volumeGenerators = new Dictionary<string, IValueGenerator>();
        }

        internal protected override Func<IValueGenerator> GetGeneratorCreator()
        {
            return () => new GBMValueGenerator();
        }
        protected internal override void ConfigureGenerators(string name, AssetEOD eod)
        {
            if (!_addedMCRV)
            {
                ValueGeneratorFactory.RegisterGenerator("mcrv", () => { return new MCRValueGenerator(); });
                _addedMCRV = true;
            }
            var volGen = (MCRValueGenerator)ValueGeneratorFactory.GetValueGenerator("mcrv");
            if (eod is StockEOD)
            {
                var typedEod = (StockEOD)eod;
                volGen.WithMean(typedEod.Volume).WithSigma(volGen.Sigma * volGen.Mean);
                if (!_volumeGenerators.TryAdd(name, volGen))
                    throw new Exception("Couldn't add VolumeGenerator for:" + name);
            }
        }
        //assumes eod not null here
        protected internal override async Task<AssetEOD> MakeEod(AssetEOD eod, string name)
        {
            if (eod is StockEOD)
            {
                var typedEod = (StockEOD)eod;
                var generator = AssetToEODGen[name];
                var newDate = typedEod.Date.Date.AddDays(1); //AssetEOD has Date field, and so does a DateTime
                var open = typedEod.Close;
                var n1 = generator.GetValue(open);
                var n2 = generator.GetValue(typedEod.Open);
                var close = generator.GetValue(typedEod.Close);
                var high = Math.Max(Math.Max(n1, n2), Math.Max(open, close));
                var low = Math.Min(Math.Min(n1, n2), Math.Min(open, close));
                var volume = _volumeGenerators[name].GetValue(typedEod.Volume);
                StockEOD res = new StockEOD(name, newDate, open, high, close, low, volume, 0.0, 0.0);
                var col = Db.GetCollection<AssetEOD>(ProcessAssetBase.ASSET_EOD_COL);
                await col.InsertOneAsync(res);
                return res;
            }
            else
            {
                throw new Exception("Some shenanigans with AssetEOD deserialization, expected StockEOD, got something else.");
            }
        }
    }
}