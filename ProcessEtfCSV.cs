
using System.IO;
using System.Text.Json;
using b1.Models;
using MongoDB.Driver;


namespace b1.Main
{

    //in retrospect, I should use Template design pattern, but it's just classes for pre processing
    //and general setup of data, in a real world project the pre processing would be more thorough anyway.
    public class ProcessEtfCSV : IProcessAsset
    {
        private readonly IMongoDatabase _db; //in general should be a property for DI later.
        private readonly string TICKER_COL_NAME = "tickers";
        public ProcessEtfCSV(IMongoDatabase dbInstance)
        {
            _db = dbInstance;
        }
        public string AssetTypeName { get; } = "etf";
        public async Task Process(string assetName)
        {

            List<TimedPrice> fyList = new(), oyList = new(), tmList = new(), twList = new();
            var dateNow = DateTime.UtcNow;
            DateTime oneYear = dateNow.AddYears(-1),twoMonths = dateNow.AddDays(-60),twoWeeks = dateNow.AddDays(-15);
            var prefixLetter = AssetTypeName[0];
            var csvFilePath = Path.Join(AssetTypeName + "s",assetName + ".csv");
            var jsonFilePath = Path.Join(AssetTypeName + "s",assetName + ".json");
           
            var col = _db.GetCollection<AssetEOD>(IProcessAsset.ASSET_EOD_COL);
            var tickerInfoCol = _db.GetCollection<TickerData>(TICKER_COL_NAME);

            var jsonStr = File.ReadAllText(jsonFilePath);
            
            var options = new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true
            };
            TickerBaseData baseData = JsonSerializer.Deserialize<TickerBaseData>(jsonStr, options)!;
            var stockTicker = new TickerData(baseData, AssetTypeName);
            using (var reader = new StreamReader(csvFilePath))
            {
                EtfEOD eod = null!;
                var date = dateNow; // <-- this will be changeed in the loop later 100%, 
                int fy = 0, oy = 0;
                string? line = reader.ReadLine(); //I constructed the csv's myself so I can confidentyly say we can execute this line
                while ((line = reader.ReadLine()) != null)// && count <= BOUND)
                {

                    var splitline = line.Split(",");
                    double open, high, low, close, dividends, splits;
                    int volume;
                    date = DateTime.Parse(splitline[0].Split(" ")[0]); //extract date
                    date = DateTime.SpecifyKind(date, DateTimeKind.Utc);
                    //this line is just to so I insert the latest < 10 EOD prices, not all 5 years data
                    

                    if (!double.TryParse(splitline[1], out open))
                        continue;
                    if (!double.TryParse(splitline[2], out high))
                        continue;
                    if (!double.TryParse(splitline[3], out low))
                        continue;
                    if (!double.TryParse(splitline[4], out close))
                        continue;
                    if (!int.TryParse(splitline[5], out volume))
                        continue;
                    if (!double.TryParse(splitline[6], out dividends))
                        continue;
                    if (!double.TryParse(splitline[7], out splits))
                        continue;
                   
                    eod = new EtfEOD(assetName,date, open, high, close, low, volume, dividends, splits);

                    if (fy == 0)
                    {
                        fyList.Add(new TimedPrice()
                        {
                            Date = date,
                            Price = close
                        });
                    }
                    fy = (++fy) % 25;
                    if (date >= oneYear)
                    {
                        if (oy == 0)
                        {
                            oyList.Add(new TimedPrice()
                            {
                                Date = date,
                                Price = close
                            });
                        }
                        oy = (++oy) % 5;
                    }
                    if (date >= twoMonths) //we are now in the time period where we have to populate
                    //the list with EOD data of the last two months
                    {
                        tmList.Add(new TimedPrice()
                        {
                            Date = date,
                            Price = close
                        });
                    }
                    if (date >= twoWeeks.AddDays(-1)) //we are now in the time period where we have to populate
                    //the list with EOD data of the last two weeks
                    {
                        twList.Add(new TimedPrice()
                        {
                            Date = DateTime.SpecifyKind(date.AddHours(6), DateTimeKind.Utc),
                            Price = eod.Open
                        });
                        twList.Add(new TimedPrice()
                        {
                            Date = DateTime.SpecifyKind(date.AddHours(6), DateTimeKind.Utc),
                            Price = (eod.Low + eod.High) / 2
                        });
                        twList.Add(new TimedPrice()
                        {
                            Date = DateTime.SpecifyKind(date.AddHours(12), DateTimeKind.Utc),
                            Price = (eod.Open + eod.Low) / 2
                        });
                        twList.Add(new TimedPrice()
                        {
                            Date = DateTime.SpecifyKind(date.AddHours(18), DateTimeKind.Utc),
                            Price = eod.Close
                        });
                    }
                    col.InsertOne(eod); //<- REMOVE IT ! change it to InsertManyAsync, 
                    
                }

                GBMValueGenerator pricegen = new();
                MCRValueGenerator volgen = new();
                volgen.WithMean(eod.Volume).WithSigma(0.15 * eod.Volume);

                //this is the part where on the chance that the python scraper doesn't load recent enough data
                //we will generate data to fill the time period until today.
                while (date.Year == dateNow.Year && date.Month == dateNow.Month && date.Day < dateNow.Day)
                {
                    date = DateTime.SpecifyKind(date.AddDays(1), DateTimeKind.Utc);
                    var volume = volgen.GetValue(eod.Volume);
                    var open = pricegen.GetValue(eod.Open);
                    var close = pricegen.GetValue(eod.Close);
                    var high = pricegen.GetValue(eod.High);
                    var low = pricegen.GetValue(eod.Low);
                    var prices = new List<double>() { open, close, low, high };
                    var max = prices.Max();
                    var min = prices.Min();
                    low = min;
                    high = max;

                    eod = new EtfEOD(assetName,date, open, high, close, low, volume, 0.0, 0.0);
                    if (fy == 0)
                    {
                        fyList.Add(new TimedPrice()
                        {
                            Date = date,
                            Price = close
                        });
                    }
                    fy = (++fy) % 25;
                    if (date >= oneYear) //we are now in the time period where we have to populate
                                         //the list with EOD data of the last year
                    {
                        if (oy == 0)
                        {
                            oyList.Add(new TimedPrice()
                            {
                                Date = date,
                                Price = close
                            });
                        }
                        oy = (++oy) % 5;
                    }
                    if (date >= twoMonths) //we are now in the time period where we have to populate
                                           //the list with EOD data of the last two months
                    {
                        tmList.Add(new TimedPrice()
                        {
                            Date = date,
                            Price = close
                        });
                    }
                    if (date >= twoWeeks.AddDays(-1)) //we are now in the time period where we have to populate
                                                      //the list with EOD data of the last two weeks
                    {
                        twList.Add(new TimedPrice()
                        {
                            Date = DateTime.SpecifyKind(date.AddHours(6), DateTimeKind.Utc),
                            Price = eod.Open
                        });
                        twList.Add(new TimedPrice()
                        {
                            Date = DateTime.SpecifyKind(date.AddHours(6), DateTimeKind.Utc),
                            Price = (eod.Low + eod.High) / 2
                        });
                        twList.Add(new TimedPrice()
                        {
                            Date = DateTime.SpecifyKind(date.AddHours(12), DateTimeKind.Utc),
                            Price = (eod.Open + eod.Low) / 2
                        });
                        twList.Add(new TimedPrice()
                        {
                            Date = DateTime.SpecifyKind(date.AddHours(18), DateTimeKind.Utc),
                            Price = eod.Close
                        });
                    }
                    col.InsertOne(eod);
                }
                stockTicker.LastTwoWeekPrices = twList;
                stockTicker.LastTwoMonthPrices = tmList;
                stockTicker.LastYearPrices = oyList;
                stockTicker.LastFiveYearPrices = fyList;
                try
                {
                    await tickerInfoCol.InsertOneAsync(stockTicker);
                }
                catch (MongoWriteException)
                {
                    throw;
                }
            }
        }
    }
}