
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

        private PriceContextHolder CtxHolder { get; init; }
        public PriceHistoryManager(PriceContextHolder contextHolder, IMongoDatabase dbInstnace)
        {
            Db = dbInstnace;
            CtxHolder = contextHolder;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var tickersCol = Db.GetCollection<TickerData>("tickers");
            var chartsCol = Db.GetCollection<ChartData>(ProcessAssetBase.CHART_HIS_COL);
            long count = 0;
            const int TWO_WEEK_TICKS = 30, TWO_MONTH_TICKS = 120, ONE_YEAR_TICKS = 600, FIVE_YEAR_TICKS = 3000;
            const int ONE_DAY_L = 120, TWO_WEEK_L = 56, TWO_MONTH_L = 60, ONE_YEAR_L = 73, FIVE_YEAR_L = 73;
            for (; ; )
            {
                Stopwatch sw = Stopwatch.StartNew();
                List<PriceContext> contexts = CtxHolder.GetAllContexts().ToList();
                foreach (var context in contexts)
                {
                    List<string> symbolNames = context.GetSymbolNames();
                    foreach (var symbol in symbolNames)
                    {
                        var latestPrice = context.GetTimedPrice(symbol); //get latest TimedPrice
                        await chartsCol.UpdateOneAsync(
                            t => t.Symbol == symbol,
                            Builders<ChartData>.Update.PushEach(
                                td => td.LastDayPrices,
                                new[] { latestPrice },
                                ONE_DAY_L
                            )
                        );
                        if (count % TWO_WEEK_TICKS == 0)
                        {
                           await chartsCol.UpdateOneAsync(
                                t => t.Symbol == symbol,
                                Builders<ChartData>.Update.PushEach(
                                    td => td.LastTwoWeekPrices,
                                    new[] { latestPrice },
                                    TWO_WEEK_L
                                )
                            );
                        }
                        if (count % TWO_MONTH_TICKS == 0)
                        {
                            await chartsCol.UpdateOneAsync(
                            t => t.Symbol == symbol,
                                Builders<ChartData>.Update.PushEach(
                                    td => td.LastTwoMonthPrices,
                                    new[] { latestPrice },
                                    TWO_MONTH_L
                                )
                            );
                        }
                        if (count % ONE_YEAR_TICKS == 0)
                        {
                            await chartsCol.UpdateOneAsync(
                            t => t.Symbol == symbol,
                                Builders<ChartData>.Update.PushEach(
                                    td => td.LastYearPrices,
                                    new[] { latestPrice },
                                    ONE_YEAR_L
                                )
                            );
                        }
                        if (count % FIVE_YEAR_TICKS == 0)
                        {
                            await chartsCol.UpdateOneAsync(
                            t => t.Symbol == symbol,
                            Builders<ChartData>.Update.PushEach(
                                td => td.LastFiveYearPrices,
                                new[] { latestPrice },
                                FIVE_YEAR_L
                            )
                        );
                        }
                    }
                }
                sw.Stop();
                Console.WriteLine("Time to insert all TimedPrice (microsecs):" + sw.Elapsed.Microseconds);
                await Task.Delay(TimeSpan.FromSeconds(12));
                count++;
            }
        }
    }
}