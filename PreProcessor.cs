using b1.Models;
using MongoDB.Bson;
using MongoDB.Driver;


namespace b1.Main
{
    public class PreProcessor
    {
        const string TICKER_COL_NAME = "tickers";
        private readonly Dictionary<string, IProcessAsset> _assetProcessors =
            new Dictionary<string, IProcessAsset>();
        private IMongoDatabase Db { get; init; } 
        public PreProcessor(IMongoDatabase dbInstance)
        {
            Db = dbInstance;
            Db.CreateCollection(IProcessAsset.ASSET_EOD_COL);
            //this is bad cuz it's hardcoded
            Db.CreateCollection(TICKER_COL_NAME); // <- should be injected usin the DI container.
        }
        public async Task Run()
        {
            var col = Db.GetCollection<AssetEOD>(IProcessAsset.ASSET_EOD_COL);
            var indexKeys = Builders<AssetEOD>.IndexKeys.
                Ascending(asset => asset.Symbol).
                Ascending(asset => asset.Date);
            var index = new CreateIndexModel<AssetEOD>(indexKeys);
            await col.Indexes.CreateOneAsync(index);

            List<string> etfNames = new List<string>();
            List<string> fxNames = new List<string>();
            List<string> stockNames = new List<string>();

            var stockPaths = Directory.GetFiles("stocks");
            var fxPaths = Directory.GetFiles("fxs");
            var etfPaths = Directory.GetFiles("etfs");

            foreach (string s in stockPaths)
            {
                var splitS = s.Split(".");
                if (splitS[1] == "csv")
                {
                    var fileName = Path.GetFileName(s);
                    stockNames.Add(fileName.Split(".")[0]);
                    RegisterProcessor(fileName.Split(".")[0], new ProcessStockCSV(Db));
                }
            }
            foreach (string s in etfPaths)
            {   
                var splitS = s.Split(".");
                if (splitS[1] == "csv")
                {
                    var fileName = Path.GetFileName(s);
                    etfNames.Add(fileName.Split(".")[0]);
                    RegisterProcessor(fileName.Split(".")[0], new ProcessEtfCSV(Db));
                }
            }
            foreach (string s in fxPaths)
            {
                var splitS = s.Split(".");
                if (splitS[1] == "csv")
                {
                    var fileName = Path.GetFileName(s);
                    fxNames.Add(fileName.Split(".")[0]);
                    RegisterProcessor(fileName.Split(".")[0], new ProcessFxCSV(Db));
                }
            }

            await ProcessAssets(stockNames);
            //ProcessType(stockNames); //<- obviously later change "AAPL" to stockNames List<string>
        }
        public void RegisterProcessor(string name, IProcessAsset processor)
        {   
            if (name != null && name != "" && processor != null)
            {
                _assetProcessors.Add(name, processor);
            } 
        }
        
        public async Task ProcessAssets(List<string> assetNames)
        { 
            foreach (var asset in assetNames)
            {
                try
                {
                    IProcessAsset? processor = null;
                    if (_assetProcessors.TryGetValue(asset, out processor))
                    {
                        await processor.Process(asset);
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
    }
}