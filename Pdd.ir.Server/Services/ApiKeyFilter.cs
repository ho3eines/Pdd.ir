using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Pdd.ir.Server.Services
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class ApiKeyAttribute : Attribute, IAsyncActionFilter
    {
        private const string ApiKeyHeaderName = "X-API-Key";

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var config = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
            var configuredApiKey = config["ApiKey"];

            if (string.IsNullOrEmpty(configuredApiKey))
            {
                await next();
                return;
            }

            if (!context.HttpContext.Request.Headers.TryGetValue(ApiKeyHeaderName, out var extractedApiKey))
            {
                context.Result = new UnauthorizedObjectResult(new { message = "API Key is missing" });
                return;
            }

            if (!string.Equals(configuredApiKey, extractedApiKey, StringComparison.Ordinal))
            {
                context.Result = new UnauthorizedObjectResult(new { message = "Invalid API Key" });
                return;
            }

            await next();
        }
    }
}
