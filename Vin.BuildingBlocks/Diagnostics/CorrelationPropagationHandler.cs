using System.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace Vin.BuildingBlocks.Diagnostics;

public sealed class CorrelationPropagationHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CorrelationPropagationHandler(IHttpContextAccessor httpContextAccessor)
        => _httpContextAccessor = httpContextAccessor;

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (!request.Headers.Contains(CorrelationIdMiddleware.HeaderName))
        {
            var correlationId = _httpContextAccessor.HttpContext?.TraceIdentifier
                ?? Activity.Current?.TraceId.ToString()
                ?? Guid.NewGuid().ToString("N");
            request.Headers.TryAddWithoutValidation(CorrelationIdMiddleware.HeaderName, correlationId);
        }

        return base.SendAsync(request, cancellationToken);
    }
}
