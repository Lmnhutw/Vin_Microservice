using Vin.Contracts;

namespace Vin.Eventing.Abstractions;

public interface IIntegrationEventHandler
{
    bool CanHandle(string eventType, int version);
    Task HandleAsync(IntegrationEventEnvelope envelope, CancellationToken cancellationToken);
}
