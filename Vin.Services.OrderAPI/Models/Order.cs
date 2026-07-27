namespace Vin.Services.OrderAPI.Models;

public sealed class Order
{
    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public OrderStatus Status { get; set; }
    public string? FailureReason { get; set; }
    public DateTimeOffset CreatedOnUtc { get; set; }
    public DateTimeOffset UpdatedOnUtc { get; set; }
    public ICollection<OrderLine> Lines { get; set; } = new List<OrderLine>();
}

public sealed class OrderLine
{
    public long OrderLineId { get; set; }
    public Guid OrderId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public Order? Order { get; set; }
}
