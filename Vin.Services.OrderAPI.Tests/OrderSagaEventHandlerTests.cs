using Microsoft.EntityFrameworkCore;
using Vin.Contracts;
using Vin.Contracts.Events;
using Vin.Services.OrderAPI.Data;
using Vin.Services.OrderAPI.Messaging;
using Vin.Services.OrderAPI.Models;
using Xunit;

namespace Vin.Services.OrderAPI.Tests;

public sealed class OrderSagaEventHandlerTests
{
    [Fact]
    public async Task Inventory_rejection_requests_refund_then_rejects_order_after_refund()
    {
        await using var db = CreateDbContext();
        var order = await AddOrder(db, OrderStatus.PendingInventory);
        var handler = new OrderSagaEventHandler(db);
        const string correlationId = "checkout-42";

        await handler.HandleAsync(IntegrationEventEnvelope.From(new InventoryRejectedV1(
            Guid.NewGuid(), correlationId, DateTimeOffset.UtcNow, order.OrderId, "Product 7 is out of stock")), CancellationToken.None);
        await db.SaveChangesAsync();

        var compensationState = await db.Orders.SingleAsync();
        Assert.Equal(OrderStatus.CompensatingPayment, compensationState.Status);
        Assert.Equal("Product 7 is out of stock", compensationState.FailureReason);
        Assert.Contains(db.OutboxMessages, message => message.EventType == EventTypes.PaymentRefundRequestedV1 && message.CorrelationId == correlationId);

        await handler.HandleAsync(IntegrationEventEnvelope.From(new PaymentRefundedV1(
            Guid.NewGuid(), correlationId, DateTimeOffset.UtcNow, order.OrderId, "refund-42")), CancellationToken.None);
        await db.SaveChangesAsync();

        var rejected = await db.Orders.SingleAsync();
        Assert.Equal(OrderStatus.Rejected, rejected.Status);
        Assert.Contains(db.OutboxMessages, message => message.EventType == EventTypes.OrderRejectedV1 && message.CorrelationId == correlationId);
    }

    [Fact]
    public async Task Payment_rejection_short_circuits_the_saga()
    {
        await using var db = CreateDbContext();
        var order = await AddOrder(db, OrderStatus.PendingPayment);
        var handler = new OrderSagaEventHandler(db);

        await handler.HandleAsync(IntegrationEventEnvelope.From(new PaymentRejectedV1(
            Guid.NewGuid(), "checkout-declined", DateTimeOffset.UtcNow, order.OrderId, "Card declined")), CancellationToken.None);
        await db.SaveChangesAsync();

        var rejected = await db.Orders.SingleAsync();
        Assert.Equal(OrderStatus.Rejected, rejected.Status);
        Assert.Equal("Card declined", rejected.FailureReason);
        Assert.DoesNotContain(db.OutboxMessages, message => message.EventType == EventTypes.PaymentRefundRequestedV1);
    }

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;
        return new AppDbContext(options);
    }

    private static async Task<Order> AddOrder(AppDbContext db, OrderStatus status)
    {
        var order = new Order
        {
            OrderId = Guid.NewGuid(),
            OrderNumber = $"ORD-{Guid.NewGuid():N}"[..20],
            UserId = "learning-user",
            TotalAmount = 120_000m,
            Status = status,
            CreatedOnUtc = DateTimeOffset.UtcNow,
            UpdatedOnUtc = DateTimeOffset.UtcNow
        };
        db.Orders.Add(order);
        await db.SaveChangesAsync();
        return order;
    }
}
