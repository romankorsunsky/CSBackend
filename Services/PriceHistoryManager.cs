
using System.Diagnostics;
using b1.Main;
using b1.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace b1.Services
{
    //this background service will occasionaly check on the PriceContext, read TimedPrices, 
    //and update the corresponding Ticker array, for example, every 12 minutes, it will read all
    //prices, every 12 minutes it will $pop first object in Ticker.oneday, and $push a new object

    public class PriceHistoryManager : BackgroundService
    {
        
        private IMongoDatabase Db { get; init; }

        private PriceContext Ctx { get; init; }
        public PriceHistoryManager(PriceContext context, IMongoDatabase dbInstnace)
        {
            Db = dbInstnace;
            Ctx = context;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var tickersCol = Db.GetCollection<TickerData>("tickers");
            var chartsCol = Db.GetCollection<ChartData>(ProcessAssetBase.CHART_HIS_COL);
            long count = 0;
            const int TWO_WEEK_TICKS = 30, TWO_MONTH_TICKS = 120, ONE_YEAR_TICKS = 600, FIVE_YEAR_TICKS = 3000;
            const int ONE_DAY_L = 120, TWO_WEEK_L = 56, TWO_MONTH_L = 60, ONE_YEAR_L = 73, FIVE_YEAR_L = 73;
            List<string> symbolNames = null!;

            for (; ; )
            {
                TimedPrice? latestPrice = null!;
                symbolNames = Ctx.GetSymbolNames();
                foreach (var symbol in symbolNames)
                {
                    latestPrice = Ctx.GetTimedPrice(symbol); //get latest TimedPrice
                    List<UpdateDefinition<ChartData>> updates = new(4);
                    var upd = Builders<ChartData>.Update.PushEach(ch => ch.LastDayPrices, new[] { latestPrice }, ONE_DAY_L);
                    updates.Add(upd);
                    if (count % TWO_WEEK_TICKS == 0)
                    {
                        upd = Builders<ChartData>.Update.PushEach(ch => ch.LastTwoWeekPrices, new[] { latestPrice }, TWO_WEEK_L);
                    }
                    if (count % TWO_MONTH_TICKS == 0)
                    {
                        upd = Builders<ChartData>.Update.PushEach(ch => ch.LastTwoMonthPrices, new[] { latestPrice }, TWO_MONTH_L);
                    }
                    if (count % ONE_YEAR_TICKS == 0)
                    {
                        upd = Builders<ChartData>.Update.PushEach(ch => ch.LastYearPrices, new[] { latestPrice }, ONE_YEAR_L);
                    }
                    if (count % FIVE_YEAR_TICKS == 0)
                    {
                        upd = Builders<ChartData>.Update.PushEach(ch => ch.LastFiveYearPrices, new[] { latestPrice }, FIVE_YEAR_L);
                    }
                    var combinedUpdates = Builders<ChartData>.Update.Combine(updates);
                    await chartsCol.UpdateOneAsync(ch => ch.Symbol == symbol, combinedUpdates);
                }
                await Task.Delay(TimeSpan.FromSeconds(12));
                count++;
            }
        }
    }
}