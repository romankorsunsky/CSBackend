using b1.Controllers;
using MongoDB.Driver;

namespace b1.Repositories
{
    public interface IReadOnlyNewsRepository
    {
        public Task<List<NewsItem>> GetNewsItems();
    }
    public class MongoReadonlyNewsRepo : IReadOnlyNewsRepository
    {
        private IMongoCollection<NewsItem> _newsCol;

        public MongoReadonlyNewsRepo(IMongoDatabase db) {
            _newsCol = db.GetCollection<NewsItem>("news");
            var indexModel = new CreateIndexModel<NewsItem>(
                Builders<NewsItem>.IndexKeys.Ascending(item => item.Id)
            );
            _newsCol.Indexes.CreateOne(indexModel);
        }
        public async Task<List<NewsItem>> GetNewsItems()
        {
            var filter = Builders<NewsItem>.Filter.Gt(ni => ni.Date, DateTime.UtcNow.AddDays(-1));
            var res = await _newsCol.Find(filter).Limit(20).ToListAsync();
            return res;
        }
    }
}