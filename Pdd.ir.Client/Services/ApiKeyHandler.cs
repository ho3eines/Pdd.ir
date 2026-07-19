using System.Net.Http.Headers;

namespace Pdd.ir.Client.Services
{
    public class ApiKeyHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var apiKey = SecureApiKey.GetKey();
            if (!string.IsNullOrEmpty(apiKey))
                request.Headers.Add("X-API-Key", apiKey);

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
