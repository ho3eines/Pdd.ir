using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Pdd.ir.Server.Services
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class SessionAuthAttribute : Attribute, IAsyncActionFilter
    {
        private const string AuthHeaderName = "X-Auth";

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Skip auth for handshake endpoint
            var path = context.HttpContext.Request.Path.Value ?? "";
            if (path.Contains("/auth/handshake"))
            {
                await next();
                return;
            }

            var sessionService = context.HttpContext.RequestServices.GetRequiredService<ClientSessionService>();

            if (!context.HttpContext.Request.Headers.TryGetValue(AuthHeaderName, out var encryptedAuth))
            {
                context.Result = new UnauthorizedObjectResult(new { message = "Authentication required" });
                return;
            }

            var isValid = await sessionService.ValidateAuthHeaderAsync(encryptedAuth);
            if (!isValid)
            {
                context.Result = new UnauthorizedObjectResult(new { message = "Invalid or expired session" });
                return;
            }

            await next();
        }
    }
}
