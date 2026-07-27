namespace Vin.MessageBus;

public sealed record MessageBusMetadata(
    string CorrelationId,
    string? MessageId = null,
    string? EventType = null,
    int? EventVersion = null);
