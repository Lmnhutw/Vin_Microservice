namespace Vin.Services.InventoryAPI.Models;

public sealed class SetInventoryRequest
{
    public int AvailableQuantity { get; init; }
}

public sealed record InventoryItemResponse(int ProductId, int AvailableQuantity, DateTimeOffset UpdatedOnUtc);
