using b1.Models;

namespace b1.Repositories{
    public interface IChartDataRepository
    {
        public Task<ChartData?> GetChartDataBySymbolName(string symbolName);
        public  Task DeleteChartData(string Id);
    }
}