using b1.Controllers;
using b1.Repositories;

namespace b1.Services{
    public class NewsService
    {
        private IReadOnlyNewsRepository _newsRepo;

        public NewsService(IReadOnlyNewsRepository newsRepo)
        {
            _newsRepo = newsRepo;
        }

        public async Task<List<NewsItem>> GetNews()
        {
            var res = await _newsRepo.GetNewsItems();
            return res;
        }
    }
}