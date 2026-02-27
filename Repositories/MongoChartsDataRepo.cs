using b1.Models;
using MongoDB.Driver;

namespace b1.Repositories
{
    public class MongoChartsDataRepo: IChartDataRepository
    {
        private IMongoCollection<ChartData> _charts { get; set; } = null!;
        public MongoChartsDataRepo(IMongoDatabase db)
        {
            _charts = db.GetCollection<ChartData>("charthistory");
        }
        public async Task<ChartData?> GetChartDataBySymbolName(string symbolName)
        {
            var results = await _charts.FindAsync(chart => chart.Symbol == symbolName);
            var data = await results.FirstOrDefaultAsync();
            return data;
        }
        public async Task DeleteChartData(string Id)
        {
            await _charts.DeleteOneAsync(ch => ch.Id == Id);
        }
    }
}