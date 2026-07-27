using Microsoft.EntityFrameworkCore;
using Vin.Contracts;
using Vin.Contracts.Events;
using Vin.Eventing.Abstractions;
using Vin.Eventing.Outbox;
using Vin.Services.OrderAPI.Data;
using Vin.Services.OrderAPI.Models;

namespace Vin.Services.OrderAPI.Messaging;

public sealed class OrderSagaEventHandler : IIntegrationEventHandler
{
    private readonly AppDbContext _db;

    public OrderSagaEventHandler(AppDbContext db) => _db = db;

    public bool CanHandle(string eventType, int version) => version == 1 && eventType is
        EventTypes.PaymentAuthorizedV1 or EventTypes.PaymentRejectedV1 or EventTypes.InventoryReservedV1 or
        EventTypes.InventoryRejectedV1 or EventTypes.PaymentRefundedV1;

    public async Task HandleAsync(IntegrationEventEnvelope envelope, CancellationToken cancellationToken)
    {
        switch (envelope.EventType)
        {
            case EventTypes.PaymentAuthorizedV1:
                await OnPaymentAuthorized(envelope.Deserialize<PaymentAuthorizedV1>(), cancellationToken);
                break;
            case EventTypes.PaymentRejectedV1:
                await Reject(envelope.Deserialize<PaymentRejectedV1>().OrderId, envelope.Deserialize<PaymentRejectedV1>().Reason, envelope, cancellationToken);
                break;
            case EventTypes.InventoryReservedV1:
                await Confirm(envelope.Deserialize<InventoryReservedV1>().OrderId, envelope, cancellationToken);
                break;
            case EventTypes.InventoryRejectedV1:
                await RequestPaymentCompensation(envelope.Deserialize<InventoryRejectedV1>(), envelope, cancellationToken);
                break;
            case EventTypes.PaymentRefundedV1:
                var refunded = envelope.Deserialize<PaymentRefundedV1>();
                await Reject(refunded.OrderId, "Inventory could not reserve the requested items; payment was refunded.", envelope, cancellationToken);
                break;
        }
    }

    private async Task OnPaymentAuthorized(PaymentAuthorizedV1 payment, CancellationToken cancellationToken)
    {
        var order = await _db.Orders.SingleOrDefaultAsync(item => item.OrderId == payment.OrderId, cancellationToken);
        if (order is null || order.Status != OrderStatus.PendingPayment) return;
        order.Status = OrderStatus.PendingInventory;
        order.UpdatedOnUtc = DateTimeOffset.UtcNow;
    }

    private async Task Confirm(Guid orderId, IntegrationEventEnvelope envelope, CancellationToken cancellationToken)
    {
        var order = await _db.Orders.SingleOrDefaultAsync(item => item.OrderId == orderId, cancellationToken);
        if (order is null || order.Status != OrderStatus.PendingInventory) return;
        order.Status = OrderStatus.Confirmed;
        order.UpdatedOnUtc = DateTimeOffset.UtcNow;
        _db.Enqueue(new OrderConfirmedV1(Guid.NewGuid(), envelope.CorrelationId, DateTimeOffset.UtcNow, order.OrderId));
    }

    private async Task RequestPaymentCompensation(InventoryRejectedV1 inventory, IntegrationEventEnvelope envelope, CancellationToken cancellationToken)
    {
        var order = await _db.Orders.SingleOrDefaultAsync(item => item.OrderId == inventory.OrderId, cancellationToken);
        if (order is null || order.Status != OrderStatus.PendingInventory) return;
        order.Status = OrderStatus.CompensatingPayment;
        order.FailureReason = inventory.Reason;
        order.UpdatedOnUtc = DateTimeOffset.UtcNow;
        _db.Enqueue(new PaymentRefundRequestedV1(Guid.NewGuid(), envelope.CorrelationId, DateTimeOffset.UtcNow, order.OrderId, inventory.Reason));
    }

    private async Task Reject(Guid orderId, string reason, IntegrationEventEnvelope envelope, CancellationToken cancellationToken)
    {
        var order = await _db.Orders.SingleOrDefaultAsync(item => item.OrderId == orderId, cancellationToken);
        if (order is null || order.Status is OrderStatus.Confirmed or OrderStatus.Rejected) return;
        order.Status = OrderStatus.Rejected;
        order.FailureReason = reason;
        order.UpdatedOnUtc = DateTimeOffset.UtcNow;
        _db.Enqueue(new OrderRejectedV1(Guid.NewGuid(), envelope.CorrelationId, DateTimeOffset.UtcNow, order.OrderId, reason));
    }
}
