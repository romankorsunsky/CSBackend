using b1.Infrastructure;
using b1.Main;
using b1.Messages;
using b1.Models;
using b1.Services;
using MathNet.Numerics.Distributions;
using MongoDB.Driver;

namespace b1.Services
{
    public abstract class PriceBackgroundServiceBase : BackgroundService
    {
        const int MAX_INTERVAL = 8640;
        protected PriceContext Ctx { get; init; }
        protected internal IMongoDatabase Db { get; init; }
        private IMessageChannel MsgBroker { get; init; }
        protected internal Dictionary<string, IValueGenerator> AssetToEODGen { get; init; }
        protected internal Dictionary<string, AssetEOD> AssetToEOD { get; init; } //latest End Of Day entry
        protected internal Dictionary<string, AssetEOD> AssetToTmrwEOD { get; init; }
        public abstract string AssetType { get; init; }
        protected PriceBackgroundServiceBase(
            PriceContext ctx,
            IMongoDatabase dbInstance,
            IMessageChannel msgBroker) : base()
        {
            MsgBroker = msgBroker;
            Db = dbInstance;
            Ctx = ctx;
            AssetToEODGen = new Dictionary<string, IValueGenerator>();
            AssetToEOD = new Dictionary<string, AssetEOD>();
            AssetToTmrwEOD = new Dictionary<string, AssetEOD>();
        }

        internal protected abstract Task<AssetEOD> MakeEod(AssetEOD eod, string assetName);
        //one important aspect of Initialize is setting the initial value of asset prices in the PriceContext
        //it is also making sure AssetToEOD containts initial EOD values.
        protected internal async Task Initialize(List<string> assetNames)
        {
            DateTime dateNow = DateTime.UtcNow;
            Dictionary<string, AssetEOD> assets = new();
            foreach (var s in assetNames)
            {
                AssetEOD? eodLast;
                eodLast = GetLastEOD(s);
                if (eodLast != null)
                {
                    if (AssetToEOD.TryAdd(s, eodLast))
                    {
                        //the following line makes sure the PriceContext has an initial price for 
                        //future calculations
                        await PublishPrice(s, new TimedPrice(eodLast.Close, dateNow));
                        assets.Add(s, eodLast);
                    }
                }
                else
                {
                    throw new Exception("Failed to add EOD" + s);
                }
            }
            ConfigureGenerators(assets);
        }

        abstract internal protected void ConfigureGenerators(Dictionary<string, AssetEOD> assetsMap);

        protected internal AssetEOD GetLastEOD(string name)
        {
            var eodCol = Db.GetCollection<AssetEOD>(ProcessAssetBase.ASSET_EOD_COL);
            var filter = Builders<AssetEOD>.Filter.Eq(s => s.Symbol, name);
            var latest = eodCol.Find(filter).
                    Sort(Builders<AssetEOD>.Sort.Descending(s => s.Date)).
                    Limit(1).
                    FirstOrDefault();
            if (latest != null)
                return latest;
            throw new ArgumentNullException("Couldn't find EOD data for asset: " + name);
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var col = Db.GetCollection<TickerData>("tickers");
            //get tickers of my type
            var filter = Builders<TickerData>.Filter.Eq(ticker => ticker.TickerType, AssetType);
            var assetList = col.Find(filter).Project(ticker => ticker.Symbol).ToList();
            await Initialize(assetList);
            var _rft = new Dictionary<string, bool>(); // ready_for_tomorrow <- indicates if we can put tomorrows EOD
            foreach (var name in assetList)
            {
                _rft.Add(name, false);
            }
            int x = 0;
            Normal normal = new Normal(0, 1 / 3.0);
            for (; ; )
            {
                if (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                foreach (var name in assetList)
                {
                    try
                    {
                        DateTime dateNow = DateTime.UtcNow;
                        DateTime sameDayStart = DateTime.SpecifyKind(dateNow.Date, DateTimeKind.Utc);
                        bool in1h = dateNow >= sameDayStart.AddHours(23).AddMinutes(10) &&
                                dateNow < sameDayStart.AddHours(24);
                        bool out1min = dateNow >= sameDayStart.AddDays(1) &&
                                dateNow < sameDayStart.AddDays(1).AddMinutes(1);
                        if (AssetToEOD.TryGetValue(name, out var currentEod))
                        {
                            if (x % 72 == 0) //every 72 ticks is 12 minutes
                            {

                            }
                            double open, close;

                            if (in1h) //50 mins before midnight create EOD for next day
                            {
                                if (!_rft[name])
                                {
                                    var newEod = await MakeEod(currentEod, name); //<- create tomorrow's EOD
                                    AssetToTmrwEOD[name] = newEod; //<- save tomorrowEOD
                                    _rft[name] = true; //><- only mark for saving when day shifts
                                }
                            }
                            if (out1min) //in first tick after midnight change 
                            {
                                if (_rft[name])
                                {
                                    var curr = AssetToEOD[name];
                                    var tmrw = AssetToTmrwEOD[name];

                                    AssetToEOD[name] = AssetToTmrwEOD[name];
                                    _rft[name] = false;
                                    x = 0;
                                }
                            }
                            open = currentEod.Open;
                            close = currentEod.Close;
                            var price = open + (close - open) * (((x) / (double)MAX_INTERVAL) + normal.Sample() * x * ((double)MAX_INTERVAL - x) / Math.Pow(MAX_INTERVAL / 2.0, 2));

                            try
                            {
                                await PublishPrice(name, new TimedPrice(price, dateNow));
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("failed to produce prices for " + name);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.StackTrace);
                    }
                }
                x++;
                await Task.Delay(TimeSpan.FromSeconds(10));
            }
        }
        private async Task PublishPrice(string symbol, TimedPrice tp)
        {  
            var arg = new PriceChangedMsg(symbol, tp);
            await MsgBroker.PublishEvent<PriceChangedMsg>(arg); 
        }
    }
}