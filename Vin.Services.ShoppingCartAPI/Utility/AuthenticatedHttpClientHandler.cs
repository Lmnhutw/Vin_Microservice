using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication;

namespace Vin.Services.ShoppingCartAPI.Utility
{
    namespace Vin.Services.ShoppingCartAPI.Utility
    {
        // A custom HTTP client handler that automatically attaches a Bearer token to each outgoing request.
        public class AuthenticatedHttpClientHandler : DelegatingHandler
        {
            private readonly IHttpContextAccessor _accessor;

            public AuthenticatedHttpClientHandler(IHttpContextAccessor accessor)
            {
                _accessor = accessor;
            }

            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                //Retrieve the access token for the current user from the HTTP context
                var token = await _accessor.HttpContext.GetTokenAsync("access_token");
                // Add the token to the request’s Authorization header in Bearer format
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                // Send the HTTP request to the next handler in the chain (or the server, if this is the last handler)
                return await base.SendAsync(request, cancellationToken);
            }
        }
    }
}