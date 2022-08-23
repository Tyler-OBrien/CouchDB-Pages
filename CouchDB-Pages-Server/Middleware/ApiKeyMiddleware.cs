using CouchDBPages.Server.Models.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CouchDBPages.Server.Middleware;

[AttributeUsage(AttributeTargets.Class)]
public class ApiKeyMiddleware : Attribute, IAsyncActionFilter
{
    private const string APIKEYNAME = "ApiKey";


    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (!context.HttpContext.Request.Headers.TryGetValue(APIKEYNAME, out var extractedApiKey))
        {
            context.Result = new ContentResult
            {
                StatusCode = 401,
                Content = "Api Key was not provided"
            };
            return;
        }

        var secretsService = context.HttpContext.RequestServices.GetRequiredService<ISecretsService>();

        var apiKey = await secretsService.GetSecret(APIKEYNAME, context.HttpContext.RequestAborted);

        if (apiKey == null)
        {
            context.Result = new ContentResult
            {
                StatusCode = 401,
                Content = "No API Key Setup."
            };
            return;
        }

        if (!apiKey.Secret.Equals(extractedApiKey, StringComparison.OrdinalIgnoreCase))
        {
            context.Result = new ContentResult
            {
                StatusCode = 401,
                Content = "Api Key is not valid"
            };
            return;
        }

        await next();
    }
}