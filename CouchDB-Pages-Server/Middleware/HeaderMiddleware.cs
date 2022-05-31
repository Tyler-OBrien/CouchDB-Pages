using CouchDBPages.Server.Models.Config;
using Microsoft.Extensions.Options;

namespace CouchDBPages.Server.Middleware;

public class HeaderMiddleware : IMiddleware
{
    private readonly ApplicationConfig _applicationConfig;

    public HeaderMiddleware(IOptions<ApplicationConfig> applicationConfig)
    {
        _applicationConfig = applicationConfig.Value;
    }


    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        context.Response.Headers.TryAdd("location", _applicationConfig.LocationName);
        await next(context);
    }
}