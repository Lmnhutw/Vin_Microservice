using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vin.Services.InventoryAPI.Data;
using Vin.Services.InventoryAPI.Models;

namespace Vin.Services.InventoryAPI.Controllers;

[ApiController]
[Authorize(Roles = "ADMIN")]
[Route("api/inventory/items")]
public sealed class InventoryController : ControllerBase
{
    private readonly AppDbContext _db;

    public InventoryController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IReadOnlyCollection<InventoryItemResponse>> Get(CancellationToken cancellationToken)
        => await _db.InventoryItems.AsNoTracking()
            .OrderBy(item => item.ProductId)
            .Select(item => new InventoryItemResponse(item.ProductId, item.AvailableQuantity, item.UpdatedOnUtc))
            .ToArrayAsync(cancellationToken);

    [HttpPut("{productId:int}")]
    public async Task<ActionResult<InventoryItemResponse>> Set(
        int productId,
        [FromBody] SetInventoryRequest request,
        CancellationToken cancellationToken)
    {
        if (productId <= 0 || request.AvailableQuantity < 0)
        {
            return BadRequest("Product id and available quantity must be non-negative.");
        }
        var item = await _db.InventoryItems.SingleOrDefaultAsync(stock => stock.ProductId == productId, cancellationToken);
        if (item is null)
        {
            item = new InventoryItem { ProductId = productId };
            _db.InventoryItems.Add(item);
        }
        item.AvailableQuantity = request.AvailableQuantity;
        item.UpdatedOnUtc = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return Ok(new InventoryItemResponse(item.ProductId, item.AvailableQuantity, item.UpdatedOnUtc));
    }
}
