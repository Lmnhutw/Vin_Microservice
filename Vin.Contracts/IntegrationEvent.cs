namespace Vin.Contracts;

public interface IIntegrationEvent
{
    Guid EventId { get; }
    string CorrelationId { get; }
    DateTimeOffset OccurredOnUtc { get; }
    string EventType { get; }
    int Version { get; }
}

public abstract record IntegrationEvent(
    Guid EventId,
    string CorrelationId,
    DateTimeOffset OccurredOnUtc) : IIntegrationEvent
{
    public abstract string EventType { get; }
    public virtual int Version => 1;
}
