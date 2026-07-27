namespace Vin.Eventing.Inbox;

public sealed class InboxMessage
{
    public Guid EventId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string CorrelationId { get; set; } = string.Empty;
    public DateTimeOffset ReceivedOnUtc { get; set; }
}
