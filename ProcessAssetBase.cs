using b1.Models;
using MongoDB.Driver;
using SharpCompress.Compressors.Xz;

namespace b1.Main
{
    public abstract class ProcessAssetBase
    {
        //two weeks difference measured in days
        public const int TWO_W_D = 14;
        //two months measured in months, I don't know, maybe some asset processor class
        //will use a different date class instead of DateTime, so the steps of taking the EOD data
        //and their units matter because dates are being compared in the context of the implementind class.
        public const int TWO_M_M = 2;
        //same as with months, but with years
        public const int ONE_Y_Y = 1;
        //same as above, again
        public const int FIVE_Y_Y = 5;

        public const string ASSET_EOD_COL = "asseteod";
        public const string CHART_HIS_COL = "charthistory";
        //method to get the name of the asset type, serves like a class identifier, this is a stupid method tbh.
        protected readonly IMongoDatabase _db;
        public const string TICKER_COL_NAME = "tickers";
        public abstract string AssetTypeName { get; }
        protected List<TimedPrice> FyList { get; init; }
        protected List<TimedPrice> OyList { get; init; }
        protected List<TimedPrice> TmList { get; init; }
        protected List<TimedPrice> TwList { get; init; }
        protected DateTime OneYear { get; init; }
        protected DateTime TwoMonths { get; init; }
        protected DateTime TwoWeeks { get; init; }
        public ProcessAssetBase(IMongoDatabase dbInstance)
        {
            _db = dbInstance;
            FyList = new();
            OyList = new();
            TmList = new();
            TwList = new();
            var dateNow = DateTime.UtcNow;
            OneYear = dateNow.AddYears(-1);
            TwoMonths = dateNow.AddDays(-60);
            TwoWeeks = dateNow.AddDays(-15);
        }


        //this method takes a type, because we assume directories are "stocks/.. OR someOtherKind/.."
        public abstract Task Process(string stockName);   
        protected void PopulateChartHist(AssetEOD eod,ref int fy,ref int oy)
        {
            var date = eod.Date;
            var close = eod.Close;
            if (fy == 0)
            {
                FyList.Add(new TimedPrice()
                {
                    Date = date,
                    Price = close
                });
            }
            fy = (++fy) % 25;
            if (date >= OneYear)
            {
                if (oy == 0)
                {
                    OyList.Add(new TimedPrice()
                    {
                        Date = date,
                        Price = close
                    });
                }
                oy = (++oy) % 5;
            }
            if (date >= TwoMonths) //we are now in the time period where we have to populate
                                    //the list with EOD data of the last two months
            {
                TmList.Add(new TimedPrice()
                {
                    Date = date,
                    Price = close
                });
            }
            if (date >= TwoWeeks.AddDays(-1)) //we are now in the time period where we have to populate
                                                //the list with EOD data of the last two weeks
            {
                TwList.Add(new TimedPrice()
                {
                    Date = DateTime.SpecifyKind(date.AddHours(6), DateTimeKind.Utc),
                    Price = eod.Open
                });
                TwList.Add(new TimedPrice()
                {
                    Date = DateTime.SpecifyKind(date.AddHours(6), DateTimeKind.Utc),
                    Price = (eod.Low + eod.High) / 2
                });
                TwList.Add(new TimedPrice()
                {
                    Date = DateTime.SpecifyKind(date.AddHours(12), DateTimeKind.Utc),
                    Price = (eod.Open + eod.Low) / 2
                });
                TwList.Add(new TimedPrice()
                {
                    Date = DateTime.SpecifyKind(date.AddHours(18), DateTimeKind.Utc),
                    Price = eod.Close
                });
            }
        }
    }
}