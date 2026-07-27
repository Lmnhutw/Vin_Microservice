using System.Text.Json;
using System.Text.Json.Serialization;

namespace Vin.Contracts;

public sealed record IntegrationEventEnvelope(
    Guid EventId,
    string EventType,
    int Version,
    string CorrelationId,
    DateTimeOffset OccurredOnUtc,
    JsonElement Payload)
{
    public static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public static IntegrationEventEnvelope From<TEvent>(TEvent integrationEvent) where TEvent : IIntegrationEvent
        => new(
            integrationEvent.EventId,
            integrationEvent.EventType,
            integrationEvent.Version,
            integrationEvent.CorrelationId,
            integrationEvent.OccurredOnUtc,
            JsonSerializer.SerializeToElement(integrationEvent, JsonOptions));

    public TEvent Deserialize<TEvent>() where TEvent : IIntegrationEvent
        => Payload.Deserialize<TEvent>(JsonOptions)
            ?? throw new JsonException($"Unable to deserialize event {EventType} v{Version}.");
}
