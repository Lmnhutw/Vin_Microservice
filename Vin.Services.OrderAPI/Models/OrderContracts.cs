namespace Vin.Services.OrderAPI.Models;

public sealed class CreateOrderRequest
{
    public IReadOnlyCollection<CreateOrderLineRequest> Lines { get; init; } = Array.Empty<CreateOrderLineRequest>();
}

public sealed class CreateOrderLineRequest
{
    public int ProductId { get; init; }
    public int Quantity { get; init; }
}

public sealed record OrderResponse(
    Guid OrderId,
    string OrderNumber,
    string UserId,
    decimal TotalAmount,
    OrderStatus Status,
    string? FailureReason,
    DateTimeOffset CreatedOnUtc,
    IReadOnlyCollection<OrderLineResponse> Lines);

public sealed record OrderLineResponse(int ProductId, string ProductName, decimal UnitPrice, int Quantity);
