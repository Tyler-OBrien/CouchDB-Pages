using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Net.Http.Headers;

namespace CouchDBPages.Server.Middleware;

[AttributeUsage(AttributeTargets.Method)]
public class ETagMiddleware : Attribute, IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var executedContext = await next();
        var fileStreamResult = executedContext.Result as FileStreamResult;
        if (fileStreamResult?.EntityTag != null)
            if (context.HttpContext.Request.Headers.TryGetValue(HeaderNames.IfNoneMatch, out var ifNoneMatch))
                if (fileStreamResult.EntityTag.Tag.ToString().Equals(ifNoneMatch, StringComparison.OrdinalIgnoreCase))
                    context.Result = new StatusCodeResult(304);
    }
}