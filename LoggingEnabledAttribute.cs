using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using MongoDB.Bson;

namespace b1
{
    
    public class LoggingEnabledAttribute : ActionFilterAttribute
    {
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var httpCtx = context.HttpContext;
            httpCtx.Request.EnableBuffering(); //to not consume the stream
            var request = httpCtx.Request;
            using (var reader = new StreamReader(httpCtx.Request.Body, Encoding.UTF8))
            {
                string body = await reader.ReadToEndAsync();
                Console.WriteLine($"[REQUEST]: METHOD: {request.Method}\nROUTE: {request.Path}");
            }
            var executionRes = await next();
            //<- If on this line, I set context.HttpContext.Response to some value, I think it will
            //short circuit and skip to Result filters, there we actually Serialize, generate Views etc.
            if (executionRes.Result is ObjectResult res)
            {
                var jsonResult = JsonSerializer.Serialize(res.Value);
                Console.WriteLine($"[RESPONSE BODY]: {jsonResult}");
            }
            Console.WriteLine("[LOG] Status = " + httpCtx.Response.StatusCode);
        }
    }
}