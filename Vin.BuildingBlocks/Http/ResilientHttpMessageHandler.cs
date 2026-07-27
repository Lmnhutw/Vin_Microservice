using System.Collections.Concurrent;
using System.Net;
using Microsoft.Extensions.Logging;

namespace Vin.BuildingBlocks.Diagnostics;

public sealed class ResilientHttpMessageHandler : DelegatingHandler
{
    private const int MaxAttempts = 3;
    private static readonly ConcurrentDictionary<string, CircuitState> Circuits = new(StringComparer.OrdinalIgnoreCase);
    private readonly ILogger<ResilientHttpMessageHandler> _logger;

    public ResilientHttpMessageHandler(ILogger<ResilientHttpMessageHandler> logger) => _logger = logger;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var destination = request.RequestUri?.Authority ?? "unknown";
        var circuit = Circuits.GetOrAdd(destination, _ => new CircuitState());
        if (circuit.IsOpen)
        {
            _logger.LogWarning("Circuit open for {Destination}", destination);
            return new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
            {
                RequestMessage = request,
                ReasonPhrase = "Downstream circuit is open"
            };
        }

        var retryable = request.Method == HttpMethod.Get || request.Method == HttpMethod.Head || request.Method == HttpMethod.Options;
        for (var attempt = 1; ; attempt++)
        {
            using var outbound = attempt == 1 ? null : Clone(request);
            var message = outbound ?? request;
            try
            {
                var response = await base.SendAsync(message, cancellationToken);
                if (!retryable || !IsTransient(response.StatusCode) || attempt == MaxAttempts)
                {
                    TrackResponse(circuit, response.StatusCode, destination);
                    return response;
                }

                response.Dispose();
                await DelayAsync(attempt, cancellationToken);
            }
            catch (HttpRequestException exception) when (retryable && attempt < MaxAttempts)
            {
                _logger.LogWarning(exception, "Transient HTTP failure calling {Destination}; retry {Attempt}/{MaxAttempts}", destination, attempt, MaxAttempts);
                await DelayAsync(attempt, cancellationToken);
            }
            catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested && retryable && attempt < MaxAttempts)
            {
                _logger.LogWarning("Downstream timeout calling {Destination}; retry {Attempt}/{MaxAttempts}", destination, attempt, MaxAttempts);
                await DelayAsync(attempt, cancellationToken);
            }
            catch
            {
                circuit.RecordFailure();
                throw;
            }
        }
    }

    private static HttpRequestMessage Clone(HttpRequestMessage request)
    {
        var copy = new HttpRequestMessage(request.Method, request.RequestUri)
        {
            Version = request.Version,
            VersionPolicy = request.VersionPolicy
        };
        foreach (var header in request.Headers)
        {
            copy.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        return copy;
    }

    private static bool IsTransient(HttpStatusCode statusCode)
        => statusCode == HttpStatusCode.RequestTimeout || statusCode == HttpStatusCode.TooManyRequests || (int)statusCode >= 500;

    private void TrackResponse(CircuitState circuit, HttpStatusCode statusCode, string destination)
    {
        if (IsTransient(statusCode))
        {
            circuit.RecordFailure();
            _logger.LogWarning("Downstream {Destination} responded {StatusCode}", destination, (int)statusCode);
            return;
        }

        circuit.RecordSuccess();
    }

    private static Task DelayAsync(int attempt, CancellationToken cancellationToken)
        => Task.Delay(TimeSpan.FromMilliseconds(150 * Math.Pow(2, attempt - 1)), cancellationToken);

    private sealed class CircuitState
    {
        private const int FailureThreshold = 5;
        private DateTimeOffset _openedUntil = DateTimeOffset.MinValue;
        private int _failures;

        public bool IsOpen => DateTimeOffset.UtcNow < _openedUntil;

        public void RecordSuccess() => Interlocked.Exchange(ref _failures, 0);

        public void RecordFailure()
        {
            if (Interlocked.Increment(ref _failures) >= FailureThreshold)
            {
                _openedUntil = DateTimeOffset.UtcNow.AddSeconds(20);
                Interlocked.Exchange(ref _failures, 0);
            }
        }
    }
}
