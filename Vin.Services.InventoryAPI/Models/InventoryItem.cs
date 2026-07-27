namespace Vin.Services.InventoryAPI.Models;

public sealed class InventoryItem
{
    public int ProductId { get; set; }
    public int AvailableQuantity { get; set; }
    public DateTimeOffset UpdatedOnUtc { get; set; }
}
