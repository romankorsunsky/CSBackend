

using System.Collections.Concurrent;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using b1.Main;
using b1.Models;
using b1.Services;
using MathNet.Numerics.Distributions;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;

namespace b1.Services
{
    public abstract class PriceServiceBase : BackgroundService
    {
        const int MAX_INTERVAL = 8640;
        private PriceContextHolder _ctxHolder { get; init; }
        private PriceContext? _pc { get; set; } = null!;
        protected internal IMongoDatabase Db { get; init; }
        protected internal ValueGeneratorFactory ValueGeneratorFactory { get; init; }
        protected internal Dictionary<string, IValueGenerator> AssetToEODGen { get; init; }
        protected internal Dictionary<string, AssetEOD> AssetToEOD { get; init; }
        protected internal Dictionary<string, AssetEOD> AssetToTmrwEOD { get; init; }

        public abstract string AssetType { get; init; }
        protected PriceServiceBase(PriceContextHolder holder, IMongoDatabase dbInstance):base()
        {   
            Db = dbInstance;
            _ctxHolder = holder;
            ValueGeneratorFactory = new ValueGeneratorFactory();
            AssetToEODGen = new Dictionary<string, IValueGenerator>();
            AssetToEOD = new Dictionary<string, AssetEOD>();
            AssetToTmrwEOD = new Dictionary<string, AssetEOD>();
        }

        internal protected abstract Task<AssetEOD> MakeEod(AssetEOD eod, string assetName);
        //this method should add a Delegate to the ValueGeneratorFactory. Basically register the corresponding
        //IValueGenerator in the factoru, so we can get objects to generate prices.
        internal protected abstract Func<IValueGenerator> GetGeneratorCreator();

        //one important aspect of Initialize is setting the initial value of asset prices in the PriceContext
        //it is also making sure AssetToEOD containts initial EOD values.
        protected internal void Initialize(List<string> assetNames)
        {
            Func<IValueGenerator> maker = GetGeneratorCreator(); //lambda that creates a price generator object
            ValueGeneratorFactory.RegisterGenerator(AssetType, maker); //register that lambda with the asset type
            _ctxHolder.AddContext(AssetType, new PriceContext(AssetType));
            DateTime dateNow = DateTime.UtcNow;
            var pc = _ctxHolder.GetContext(AssetType);
            foreach (var s in assetNames)
            {

                AssetEOD? eodLast = null!;
                eodLast = GetLastEOD(s);
                var added = false;
                if (eodLast != null && pc != null)
                {
                    added = AssetToEOD.TryAdd(s, eodLast);
                    if (added)
                    {
                        pc.PutPrice(s, new TimedPrice(eodLast.Close, dateNow));
                        ConfigureGenerators(s, eodLast);
                    }
                }
                else
                {
                    throw new Exception("Failed to add EOD" + s);
                }
            }
            Console.WriteLine("FINISHED INITIALIZE FOR " + AssetType);
        }

        abstract internal protected void ConfigureGenerators(string s, AssetEOD eod);


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


        protected internal PriceContext? GetPriceContext()
        {
            return _ctxHolder.GetContext(AssetType);
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {   
            var col = Db.GetCollection<TickerData>("tickers");
            var filter = Builders<TickerData>.Filter.Eq(ticker => ticker.TickerType, AssetType);
            var assetList = col.Find(filter).Project(ticker => ticker.Symbol).ToList();
            Initialize(assetList);
            var _rft = new Dictionary<string, bool>();
            var _priceContext = _ctxHolder.GetContext(AssetType);
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
                Stopwatch sw = Stopwatch.StartNew();
                foreach (var name in assetList)
                {
                    try
                    {
                        DateTime dateNow = DateTime.UtcNow;
                        DateTime sameDayStart = DateTime.SpecifyKind(dateNow.Date, DateTimeKind.Utc);
                        bool in1h = dateNow >= sameDayStart.AddHours(23).AddMinutes(30) &&
                                dateNow < sameDayStart.AddHours(24);
                        bool out1min = dateNow >= sameDayStart.AddDays(1) &&
                                dateNow < sameDayStart.AddDays(1).AddMinutes(1);
                        if (AssetToEOD.TryGetValue(name, out var currentEod))
                        {
                            if (x % 72 == 0) //every 72 ticks is 12 minutes
                            {

                            }
                            double open, close;

                            if (in1h) //30 mins before midnight create EOD for next day
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
                            _priceContext?.PutPrice(name, new TimedPrice(price, dateNow));
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.StackTrace);
                    }

                }
                x++;
                sw.Stop();
                TimeSpan ts = sw.Elapsed;
                await Task.Delay(TimeSpan.FromSeconds(10)); // <- when making IHostedService add System.Timers.Timer insteadof Task.Delay
            }
        }
        
            
    }
}