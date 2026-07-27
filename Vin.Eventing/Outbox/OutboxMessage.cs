namespace Vin.Eventing.Outbox;

public sealed class OutboxMessage
{
    public Guid EventId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public int Version { get; set; }
    public string CorrelationId { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public DateTimeOffset OccurredOnUtc { get; set; }
    public DateTimeOffset? ProcessedOnUtc { get; set; }
    public DateTimeOffset? DeadLetteredOnUtc { get; set; }
    public int Attempts { get; set; }
    public string? LastError { get; set; }
}
