using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vin.Services.OrderAPI.Models;
using Vin.Services.OrderAPI.Services;

namespace Vin.Services.OrderAPI.Controllers;

[ApiController]
[Authorize]
[Route("api/orders")]
public sealed class OrdersController : ControllerBase
{
    private readonly OrderApplicationService _orders;

    public OrdersController(OrderApplicationService orders) => _orders = orders;

    [HttpPost]
    [ProducesResponseType<OrderResponse>(StatusCodes.Status201Created)]
    public async Task<ActionResult<OrderResponse>> Create(
        [FromBody] CreateOrderRequest request,
        CancellationToken cancellationToken)
    {
        var userId = CurrentUserId();
        var order = await _orders.SubmitAsync(userId, request, HttpContext.TraceIdentifier, cancellationToken);
        return CreatedAtAction(nameof(Get), new { orderId = order.OrderId }, order);
    }

    [HttpGet("{orderId:guid}")]
    [ProducesResponseType<OrderResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderResponse>> Get(Guid orderId, CancellationToken cancellationToken)
    {
        var order = await _orders.GetAsync(orderId, CurrentUserId(), cancellationToken);
        return order is null ? NotFound() : Ok(order);
    }

    private string CurrentUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub)
        ?? throw new BadHttpRequestException("Authenticated user id is missing.");
}
