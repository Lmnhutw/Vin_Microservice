using Microsoft.EntityFrameworkCore;
using Vin.Contracts.Events;
using Vin.Eventing.Outbox;
using Vin.Services.OrderAPI.Data;
using Vin.Services.OrderAPI.Models;

namespace Vin.Services.OrderAPI.Services;

public sealed class OrderApplicationService
{
    private readonly AppDbContext _db;
    private readonly IProductCatalogClient _catalog;

    public OrderApplicationService(AppDbContext db, IProductCatalogClient catalog)
    {
        _db = db;
        _catalog = catalog;
    }

    public async Task<OrderResponse> SubmitAsync(
        string userId,
        CreateOrderRequest request,
        string correlationId,
        CancellationToken cancellationToken)
    {
        if (request.Lines.Count == 0 || request.Lines.Any(line => line.ProductId <= 0 || line.Quantity is < 1 or > 20))
        {
            throw new BadHttpRequestException("An order needs one or more valid product lines with quantities from 1 to 20.");
        }

        var mergedLines = request.Lines
            .GroupBy(line => line.ProductId)
            .Select(group => new CreateOrderLineRequest { ProductId = group.Key, Quantity = group.Sum(line => line.Quantity) })
            .ToArray();
        if (mergedLines.Any(line => line.Quantity > 20))
        {
            throw new BadHttpRequestException("A product quantity cannot exceed 20.");
        }

        var products = await _catalog.GetProductsAsync(mergedLines.Select(line => line.ProductId), cancellationToken);
        var now = DateTimeOffset.UtcNow;
        var order = new Order
        {
            OrderId = Guid.NewGuid(),
            OrderNumber = $"ORD-{Guid.NewGuid():N}"[..12].ToUpperInvariant(),
            UserId = userId,
            Status = OrderStatus.PendingPayment,
            CreatedOnUtc = now,
            UpdatedOnUtc = now,
            Lines = mergedLines.Select(line =>
            {
                if (!products.TryGetValue(line.ProductId, out var product))
                {
                    throw new KeyNotFoundException($"Product {line.ProductId} was not found.");
                }
                return new OrderLine
                {
                    ProductId = line.ProductId,
                    ProductName = product.Name,
                    UnitPrice = product.Price,
                    Quantity = line.Quantity
                };
            }).ToArray()
        };
        order.TotalAmount = order.Lines.Sum(line => line.UnitPrice * line.Quantity);

        _db.Orders.Add(order);
        _db.Enqueue(new OrderSubmittedV1(
            Guid.NewGuid(),
            correlationId,
            now,
            order.OrderId,
            order.UserId,
            order.TotalAmount,
            order.Lines.Select(line => new OrderLineV1(line.ProductId, line.ProductName, line.UnitPrice, line.Quantity)).ToArray()));
        await _db.SaveChangesAsync(cancellationToken);
        return ToResponse(order);
    }

    public async Task<OrderResponse?> GetAsync(Guid orderId, string userId, CancellationToken cancellationToken)
    {
        var order = await _db.Orders.AsNoTracking().Include(item => item.Lines)
            .SingleOrDefaultAsync(item => item.OrderId == orderId && item.UserId == userId, cancellationToken);
        return order is null ? null : ToResponse(order);
    }

    public static OrderResponse ToResponse(Order order) => new(
        order.OrderId,
        order.OrderNumber,
        order.UserId,
        order.TotalAmount,
        order.Status,
        order.FailureReason,
        order.CreatedOnUtc,
        order.Lines.Select(line => new OrderLineResponse(line.ProductId, line.ProductName, line.UnitPrice, line.Quantity)).ToArray());
}
