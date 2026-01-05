
using b1.DTO;
using b1.Models;
using Microsoft.AspNetCore.Mvc;

namespace b1.Controllers
{
    [ApiController]
    [Route("api/v1/news")]
    [Produces("application/json")]
    public class NewsController : ControllerBase
    {
        private static IList<string> ArticleNames { get; } = new List<string>()
        {
            "Good News", "Bad News", "Average News"
        };
        //here we will add a news service I will decide if it will be read from regular files or 
        //I will hafve to make a collection or something we will see.
        
        

        public IList<ArticleDTO> GetArticleFeed()
        {
            return null;
        } 
    }

    
}
