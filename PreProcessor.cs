using b1.Models;
using MongoDB.Bson;
using MongoDB.Driver;


namespace b1.Main
{
    public class PreProcessor
    {
        const string TICKER_COL_NAME = "tickers";
        private readonly Dictionary<string, ProcessAssetBase> _assetProcessors =
            new Dictionary<string, ProcessAssetBase>();
        private IMongoDatabase Db { get; init; } 
        public PreProcessor(IMongoDatabase dbInstance)
        {
            Db = dbInstance;
            Db.CreateCollection(ProcessAssetBase.ASSET_EOD_COL);
            Db.CreateCollection(TICKER_COL_NAME);
        }
        public async Task Run()
        {
            var col = Db.GetCollection<AssetEOD>(ProcessAssetBase.ASSET_EOD_COL);
            var indexKeys = Builders<AssetEOD>.IndexKeys.
                Ascending(asset => asset.Symbol).
                Ascending(asset => asset.Date);
            var index = new CreateIndexModel<AssetEOD>(indexKeys);
            await col.Indexes.CreateOneAsync(index);
            Db.CreateCollection(ProcessAssetBase.CHART_HIS_COL);
            var indexKey = Builders<ChartData>.IndexKeys.Ascending(chrt => chrt.Symbol);
            var chartIndex = new CreateIndexModel<ChartData>(indexKey);
            var chartCol = Db.GetCollection<ChartData>(ProcessAssetBase.CHART_HIS_COL);
            await chartCol.Indexes.CreateOneAsync(chartIndex);
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
            var t1 = ProcessAssets(stockNames);
            var t2 = ProcessAssets(etfNames);
            var t3 = ProcessAssets(fxNames);
            Task.WaitAll(t1, t2, t3);
            //ProcessType(stockNames); //<- obviously later change "AAPL" to stockNames List<string>
        }
        public void RegisterProcessor(string name, ProcessAssetBase processor)
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
                    ProcessAssetBase? processor = null;
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