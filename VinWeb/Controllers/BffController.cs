using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Vin.Web.Models;
using Vin.Web.Models.AuthModels;
using Vin.Web.Models.CartModels;
using Vin.Web.Models.OrderModels;
using Vin.Web.Service.IService;

namespace Vin.Web.Controllers;

[ApiController]
[Route("api/bff")]
public sealed class BffController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IProductService _productService;
    private readonly ICouponService _couponService;
    private readonly IShoppingCartService _cartService;
    private readonly IOrderService _orderService;
    private readonly ITokenProvider _tokenProvider;

    public BffController(
        IAuthService authService,
        IProductService productService,
        ICouponService couponService,
        IShoppingCartService cartService,
        IOrderService orderService,
        ITokenProvider tokenProvider)
    {
        _authService = authService;
        _productService = productService;
        _couponService = couponService;
        _cartService = cartService;
        _orderService = orderService;
        _tokenProvider = tokenProvider;
    }

    [HttpPost("auth/login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequestDTO request)
    {
        var response = await _authService.LoginAsync(request);
        if (response is null || !response.IsSuccess || response.Result is null)
        {
            return Unauthorized(new ResponseDTO { IsSuccess = false, Message = "Invalid username or password." });
        }

        var login = JsonConvert.DeserializeObject<LoginResponseDTO>(Convert.ToString(response.Result)!);
        if (login?.User is null || string.IsNullOrWhiteSpace(login.Token))
        {
            return Unauthorized(new ResponseDTO { IsSuccess = false, Message = "Invalid username or password." });
        }

        await SignInUser(login.Token);
        _tokenProvider.SetToken(login.Token);
        return Ok(new ResponseDTO { Result = login.User });
    }

    [HttpPost("auth/register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegistrationRequestDTO request)
        => Ok(await _authService.RegisterAsync(request));

    [HttpPost("auth/forgot-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDTO request)
        => Ok(await _authService.ForgotPasswordAsync(request));

    [HttpPost("auth/reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDTO request)
        => Ok(await _authService.ResetPasswordAsync(request));

    [HttpPost("auth/logout")]
    [AllowAnonymous]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        _tokenProvider.ClearedToken();
        return Ok(new ResponseDTO { Result = true });
    }

    [HttpGet("auth/me")]
    [Authorize]
    public IActionResult Me() => Ok(new ResponseDTO
    {
        Result = new
        {
            Id = CurrentUserId(),
            Email = User.FindFirstValue(JwtRegisteredClaimNames.Email) ?? User.FindFirstValue(ClaimTypes.Email),
            Name = User.Identity?.Name,
            Roles = User.FindAll(ClaimTypes.Role).Select(x => x.Value).Distinct().ToArray()
        }
    });

    [HttpGet("products")]
    [AllowAnonymous]
    public async Task<IActionResult> Products() => Ok(await _productService.GetAllProductAsync());

    [HttpGet("products/{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> Product(int id) => Ok(await _productService.GetProductByIdAsync(id));

    [HttpPost("admin/products")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> CreateProduct([FromBody] ProductDTO product)
        => Ok(await _productService.CreateProductsAsync(product));

    [HttpPut("admin/products/{id:int}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> UpdateProduct(int id, [FromBody] ProductDTO product)
    {
        if (id != product.ProductId)
        {
            return BadRequest(new ResponseDTO
            {
                IsSuccess = false,
                Message = "The route product id does not match the request."
            });
        }

        return Ok(await _productService.UpdateProductsAsync(product));
    }

    [HttpDelete("admin/products/{id:int}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> DeleteProduct(int id)
        => Ok(await _productService.DeleteProductAsync(id));

    [HttpGet("admin/coupons")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> Coupons() => Ok(await _couponService.GetAllCouponAsync());

    [HttpPost("admin/coupons")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> CreateCoupon([FromBody] CouponDTO coupon)
        => Ok(await _couponService.CreateCouponsAsync(coupon));

    [HttpPut("admin/coupons/{id:int}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> UpdateCoupon(int id, [FromBody] CouponDTO coupon)
    {
        if (id != coupon.CouponId)
        {
            return BadRequest(new ResponseDTO
            {
                IsSuccess = false,
                Message = "The route coupon id does not match the request."
            });
        }

        return Ok(await _couponService.UpdateCouponsAsync(coupon));
    }

    [HttpDelete("admin/coupons/{id:int}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> DeleteCoupon(int id)
        => Ok(await _couponService.DeleteCouponAsync(id));

    [HttpGet("cart")]
    [Authorize]
    public async Task<IActionResult> Cart() => Ok(await _cartService.GetCartByUserIdAsync(CurrentUserId()));

    [HttpPost("cart")]
    [Authorize]
    public async Task<IActionResult> UpsertCart([FromBody] CartDTO cart) => Ok(await _cartService.UpsertCartAsync(cart));

    [HttpDelete("cart/items/{id:int}")]
    [Authorize]
    public async Task<IActionResult> RemoveCartItem(int id) => Ok(await _cartService.RemoveFromCartAsync(id));

    [HttpPost("cart/coupon")]
    [Authorize]
    public async Task<IActionResult> ApplyCoupon([FromBody] CartDTO cart) => Ok(await _cartService.ApplyCouponAsync(cart));

    [HttpDelete("cart/coupon")]
    [Authorize]
    public async Task<IActionResult> RemoveCoupon()
    {
        var cart = new CartDTO
        {
            CartHeader = new() { UserId = CurrentUserId(), CouponCode = string.Empty }
        };
        return Ok(await _cartService.ApplyCouponAsync(cart));
    }

    [HttpPost("cart/email")]
    [Authorize]
    public async Task<IActionResult> EmailCart([FromBody] CartDTO cart) => Ok(await _cartService.EmailCart(cart));

    [HttpPost("checkout")]
    [Authorize]
    public async Task<IActionResult> Checkout(CancellationToken cancellationToken)
    {
        var cartResponse = await _cartService.GetCartByUserIdAsync(CurrentUserId());
        if (cartResponse is null || !cartResponse.IsSuccess || cartResponse.Result is null)
        {
            return Conflict(new ResponseDTO { IsSuccess = false, Message = "Your cart could not be prepared for checkout." });
        }

        var cart = JsonConvert.DeserializeObject<CartDTO>(Convert.ToString(cartResponse.Result)!);
        if (cart?.CartDetails is null || !cart.CartDetails.Any())
        {
            return BadRequest(new ResponseDTO { IsSuccess = false, Message = "Your cart is empty." });
        }

        var order = await _orderService.CreateOrderAsync(new CreateOrderRequestDTO
        {
            Lines = cart.CartDetails.Select(item => new CreateOrderLineRequestDTO
            {
                ProductId = item.ProductId,
                Quantity = item.Count
            }).ToArray()
        }, cancellationToken);

        return order.IsSuccess
            ? StatusCode(StatusCodes.Status202Accepted, order)
            : StatusCode(StatusCodes.Status502BadGateway, order);
    }

    [HttpGet("orders/{orderId:guid}")]
    [Authorize]
    public async Task<IActionResult> Order(Guid orderId, CancellationToken cancellationToken)
    {
        var response = await _orderService.GetOrderAsync(orderId, cancellationToken);
        return response.IsSuccess ? Ok(response) : NotFound(response);
    }

    private string CurrentUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub)
        ?? throw new InvalidOperationException("Authenticated user id is missing.");

    private async Task SignInUser(string token)
    {
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
        foreach (var claim in jwt.Claims)
        {
            if (claim.Type is JwtRegisteredClaimNames.Email or JwtRegisteredClaimNames.Sub or
                JwtRegisteredClaimNames.Name)
            {
                identity.AddClaim(claim);
            }

            if (claim.Type is ClaimTypes.Role or "role")
            {
                identity.AddClaim(new Claim(ClaimTypes.Role, claim.Value));
            }
        }

        var email = jwt.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Email)?.Value;
        if (!string.IsNullOrWhiteSpace(email))
        {
            identity.AddClaim(new Claim(ClaimTypes.Name, email));
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier,
                jwt.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Sub)?.Value ?? string.Empty));
        }

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity));
    }
}
