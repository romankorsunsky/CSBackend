using b1.Models;
using MongoDB.Driver;

namespace b1.Repositories
{
    public class MongoChartsDataRepo
    {
        private IMongoCollection<ChartData> Charts { get; set; } = null!;
        public MongoChartsDataRepo(IMongoDatabase db)
        {
            Charts = db.GetCollection<ChartData>("charthistory");
        }
        public async Task<ChartData?> GetChartDataBySymbolName(string symbolName)
        {
            var results = await Charts.FindAsync(chart => chart.Symbol == symbolName);
            var data = await results.FirstOrDefaultAsync();
            return data;
        }
    }
}