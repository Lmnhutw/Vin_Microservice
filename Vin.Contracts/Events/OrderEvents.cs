namespace Vin.Contracts.Events;

public static class EventTypes
{
    public const string OrderSubmittedV1 = "vin.orders.submitted.v1";
    public const string PaymentAuthorizedV1 = "vin.payments.authorized.v1";
    public const string PaymentRejectedV1 = "vin.payments.rejected.v1";
    public const string InventoryReservedV1 = "vin.inventory.reserved.v1";
    public const string InventoryRejectedV1 = "vin.inventory.rejected.v1";
    public const string PaymentRefundRequestedV1 = "vin.payments.refund-requested.v1";
    public const string PaymentRefundedV1 = "vin.payments.refunded.v1";
    public const string OrderConfirmedV1 = "vin.orders.confirmed.v1";
    public const string OrderRejectedV1 = "vin.orders.rejected.v1";
}

public sealed record OrderLineV1(int ProductId, string ProductName, decimal UnitPrice, int Quantity);

public sealed record OrderSubmittedV1(
    Guid EventId,
    string CorrelationId,
    DateTimeOffset OccurredOnUtc,
    Guid OrderId,
    string UserId,
    decimal TotalAmount,
    IReadOnlyList<OrderLineV1> Lines) : IntegrationEvent(EventId, CorrelationId, OccurredOnUtc)
{
    public override string EventType => EventTypes.OrderSubmittedV1;
}

public sealed record PaymentAuthorizedV1(
    Guid EventId,
    string CorrelationId,
    DateTimeOffset OccurredOnUtc,
    Guid OrderId,
    string PaymentReference,
    IReadOnlyList<OrderLineV1> Lines) : IntegrationEvent(EventId, CorrelationId, OccurredOnUtc)
{
    public override string EventType => EventTypes.PaymentAuthorizedV1;
}

public sealed record PaymentRejectedV1(
    Guid EventId,
    string CorrelationId,
    DateTimeOffset OccurredOnUtc,
    Guid OrderId,
    string Reason) : IntegrationEvent(EventId, CorrelationId, OccurredOnUtc)
{
    public override string EventType => EventTypes.PaymentRejectedV1;
}

public sealed record InventoryReservedV1(
    Guid EventId,
    string CorrelationId,
    DateTimeOffset OccurredOnUtc,
    Guid OrderId,
    IReadOnlyList<OrderLineV1> Lines) : IntegrationEvent(EventId, CorrelationId, OccurredOnUtc)
{
    public override string EventType => EventTypes.InventoryReservedV1;
}

public sealed record InventoryRejectedV1(
    Guid EventId,
    string CorrelationId,
    DateTimeOffset OccurredOnUtc,
    Guid OrderId,
    string Reason) : IntegrationEvent(EventId, CorrelationId, OccurredOnUtc)
{
    public override string EventType => EventTypes.InventoryRejectedV1;
}

public sealed record PaymentRefundRequestedV1(
    Guid EventId,
    string CorrelationId,
    DateTimeOffset OccurredOnUtc,
    Guid OrderId,
    string Reason) : IntegrationEvent(EventId, CorrelationId, OccurredOnUtc)
{
    public override string EventType => EventTypes.PaymentRefundRequestedV1;
}

public sealed record PaymentRefundedV1(
    Guid EventId,
    string CorrelationId,
    DateTimeOffset OccurredOnUtc,
    Guid OrderId,
    string PaymentReference) : IntegrationEvent(EventId, CorrelationId, OccurredOnUtc)
{
    public override string EventType => EventTypes.PaymentRefundedV1;
}

public sealed record OrderConfirmedV1(
    Guid EventId,
    string CorrelationId,
    DateTimeOffset OccurredOnUtc,
    Guid OrderId) : IntegrationEvent(EventId, CorrelationId, OccurredOnUtc)
{
    public override string EventType => EventTypes.OrderConfirmedV1;
}

public sealed record OrderRejectedV1(
    Guid EventId,
    string CorrelationId,
    DateTimeOffset OccurredOnUtc,
    Guid OrderId,
    string Reason) : IntegrationEvent(EventId, CorrelationId, OccurredOnUtc)
{
    public override string EventType => EventTypes.OrderRejectedV1;
}
