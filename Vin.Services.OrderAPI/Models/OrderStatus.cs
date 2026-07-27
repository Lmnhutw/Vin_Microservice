namespace Vin.Services.OrderAPI.Models;

public enum OrderStatus
{
    PendingPayment,
    PendingInventory,
    CompensatingPayment,
    Confirmed,
    Rejected
}
