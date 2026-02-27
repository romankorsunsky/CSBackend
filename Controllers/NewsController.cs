
using System.Text.Json.Serialization;
using b1.Models;
using b1.Services;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace b1.Controllers
{
    [ApiController]
    [Route("api/v1/news")]
    [Produces("application/json")]
    public class NewsController : ControllerBase
    {
        private NewsService _newsService;
        public NewsController(NewsService ns)
        {
            _newsService = ns;
        }
        [HttpGet]
        [Route("")]
        public async Task<ActionResult<List<NewsItem>>> GetNews()
        {
            var lst = await _newsService.GetNews();
            return Ok(lst);
        }
    }

    public class NewsItem
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;

        [BsonElement("title")]
        public string Title { get; set; } = null!;

        [BsonElement("content")]
        public string Content { get; set; } = null!;

        [BsonElement("date")]
        public DateTime Date { get; set; }
    }
}
