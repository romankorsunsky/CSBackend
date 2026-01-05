
using System.IO;
using System.Text.Json;
using b1.Models;
using MongoDB.Driver;
using ScottPlot.Plottables;

namespace b1.Main
{
    public class ProcessStockCSV : ProcessAssetBase
    {
        public override string AssetTypeName => "stock";

        public ProcessStockCSV(IMongoDatabase dbInstance) : base(dbInstance){}
        public override async Task Process(string assetName)
        {
            var dateNow = DateTime.UtcNow;
            var csvFilePath = Path.Join(AssetTypeName + "s", assetName + ".csv");
            var jsonFilePath = Path.Join(AssetTypeName + "s", assetName + ".json");
            //get EOD collection ,ChartHistory collection and TickerData collection
            var assetCol = _db.GetCollection<AssetEOD>(ASSET_EOD_COL);
            var chartCol = _db.GetCollection<ChartData>(CHART_HIS_COL);
            var tickerInfoCol = _db.GetCollection<TickerData>(TICKER_COL_NAME);

            var jsonStr = File.ReadAllText(jsonFilePath);
            var options = new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true
            };
            TickerBaseData? baseData = null!;
            ChartData chartData = null!;
            try
            {
                baseData = JsonSerializer.Deserialize<TickerBaseData>(jsonStr, options);
                if (baseData == null)
                {
                    throw new Exception("Failed to process entry in JSON file: " + jsonStr);
                }
                if (baseData.Name != null)
                {
                    chartData = new ChartData(baseData.Name);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            if (baseData != null)
            {
                var assetTicker = new TickerData(baseData, AssetTypeName);

                using (var reader = new StreamReader(csvFilePath))
                {

                    StockEOD eod = null!;
                    var date = dateNow; // <-- this will be changeed in the loop later 100%, mb change to DateTime? date = null!;
                    int fy = 0, oy = 0;
                    string? line = reader.ReadLine(); //I constructed the csv's myself so I can confidentyly say we can execute this line
                    List<AssetEOD> eodList = new List<AssetEOD>();
                    while ((line = reader.ReadLine()) != null)// && count <= BOUND)
                    {
                        var splitline = line.Split(",");
                        double open, high, low, close, dividends, splits;
                        int volume;

                        date = DateTime.Parse(splitline[0].Split(" ")[0]); //extract date
                        date = DateTime.SpecifyKind(date, DateTimeKind.Utc);
                        //
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

                        eod = new StockEOD(assetName, date, open, high, close, low, volume, dividends, splits);
                        PopulateChartHist(eod, ref fy, ref oy);
                        eodList.Add(eod);
                    }
                    await assetCol.InsertManyAsync(eodList);
                    GBMValueGenerator pricegen = new();
                    MCRValueGenerator volgen = new();
                    volgen.WithMean(eod.Volume).WithSigma(0.15 * eod.Volume);
                    eodList = new List<AssetEOD>();
                    //this is the part where on the chance that the python scraper doesn't load recent enough data
                    //we will generate data to fill the time period until today.
                    while (date < dateNow)
                    {   //date.Year == dateNow.Year && date.Month == dateNow.Month && date.Day < dateNow.Day
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
                        eod = new StockEOD(assetName, date, open, high, close, low, volume, 0.0, 0.0);
                        PopulateChartHist(eod, ref fy, ref oy);
                        eodList.Add(eod);
                    }
                    Console.WriteLine($"IT HAS {eodList.Count}");
                    chartData.LastTwoWeekPrices = TwList;
                    chartData.LastTwoMonthPrices = TmList;
                    chartData.LastYearPrices = OyList;
                    chartData.LastFiveYearPrices = FyList;

                    try
                    {
                        await chartCol.InsertOneAsync(chartData);
                        await tickerInfoCol.InsertOneAsync(assetTicker);
                        if (eodList.Count != 0)
                            await assetCol.InsertManyAsync(eodList);
                    }
                    catch (MongoWriteException e)
                    {
                        Console.WriteLine(e.Message);
                    }
                    Console.WriteLine("PROCESSED: " + assetName);
                }

            }
        }
    }
}