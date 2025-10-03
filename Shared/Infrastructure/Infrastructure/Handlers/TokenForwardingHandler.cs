using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace TransitAFC.Shared.Infrastructure.Handlers
{
    public class TokenForwardingHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<TokenForwardingHandler> _logger;

        public TokenForwardingHandler(IHttpContextAccessor httpContextAccessor, ILogger<TokenForwardingHandler> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var token = GetTokenFromCurrentRequest();

            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                _logger.LogDebug("JWT token forwarded to downstream service: {Url}", request.RequestUri);
            }
            else
            {
                _logger.LogWarning("No JWT token found to forward to downstream service: {Url}", request.RequestUri);
            }

            return await base.SendAsync(request, cancellationToken);
        }

        private string? GetTokenFromCurrentRequest()
        {
            try
            {
                var authHeader = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].FirstOrDefault();

                if (authHeader != null && authHeader.StartsWith("Bearer "))
                {
                    return authHeader.Substring("Bearer ".Length).Trim();
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting token from current request");
                return null;
            }
        }
    }
}