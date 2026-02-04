using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace b1.Main
{
    public class CaptureErrorsAttribute : ActionFilterAttribute
    {
        public override async Task OnActionExecutionAsync(ActionExecutingContext ctx, ActionExecutionDelegate next)
        {
            var actionRes = await next();
            if (actionRes.Result is ObjectResult objectResult)
            {
                var scode = objectResult.StatusCode;
                if (scode == StatusCodes.Status500InternalServerError)
                {
                    Console.WriteLine("[LOG] An internal server error for fixing later");
                    Console.WriteLine("[LOG] Error in :" + ctx.HttpContext.Request.Path);
                }
            }
        }
    }
}