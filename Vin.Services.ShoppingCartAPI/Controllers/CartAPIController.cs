using AutoMapper;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vin.MessageBus;
using Vin.Services.ShoppingCartAPI.Data;
using Vin.Services.ShoppingCartAPI.Models;
using Vin.Services.ShoppingCartAPI.Models.DTO;
using Vin.Services.ShoppingCartAPI.Service.IService;

[Route("api/Cart")]
[ApiController]
[Authorize]
public class CartAPIController : ControllerBase
{
    private readonly ResponseDTO _res;
    private readonly IMapper _mapper;
    private readonly AppDbContext _db;
    private readonly IProductService _productService;
    private readonly ICouponService _couponService;
    private readonly IMessageBus _messageBus;
    private readonly string _emailShoppingCartQueue;


    public CartAPIController(AppDbContext db, IMapper mapper, IProductService productService, ICouponService couponService, IMessageBus messageBus, IConfiguration configuration)
    {
        _db = db;
        _res = new ResponseDTO();
        _mapper = mapper;
        _productService = productService;
        _couponService = couponService;
        _messageBus = messageBus;
        _emailShoppingCartQueue = configuration["TopicAndQueueNames:EmailShoppingCartQueue"]
            ?? throw new InvalidOperationException("TopicAndQueueNames:EmailShoppingCartQueue is not configured.");
    }

    [HttpGet("GetCart/{userID}")]
    public async Task<ActionResult<ResponseDTO>> GetCart(string userID)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            if (!string.Equals(userID, currentUserId, StringComparison.Ordinal))
            {
                return Forbid();
            }

            var cartHeader = await _db.CartHeaders.FirstOrDefaultAsync(u => u.UserId == currentUserId);
            if (cartHeader is null)
            {
                return NotFound(new ResponseDTO { IsSuccess = false, Message = "Cart not found." });
            }

            CartDTO cart = new()
            {
                CartHeader = _mapper.Map<CartHeaderDTO>(cartHeader),
            };
            cart.CartDetails = _mapper.Map<IEnumerable<CartDetailsDTO>>(_db.CartDetails
                .Where(u => u.CartHeaderId == cart.CartHeader.CartHeaderId));

            IEnumerable<ProductDTO> productDTOs = await _productService.GetProducts();

            foreach (var item in cart.CartDetails)
            {
                item.Product = productDTOs.FirstOrDefault(u => u.ProductId == item.ProductId);
                if (item.Product != null) // Add this check
                {
                    cart.CartHeader.CartTotal += (item.Count * item.Product.Price);
                }


            }

            //check if couponcode null or not 
            if (!string.IsNullOrEmpty(cart.CartHeader.CouponCode))
            {
                CouponDTO coupon = await _couponService.GetCoupon(cart.CartHeader.CouponCode);
                if (coupon != null)
                {
                    if (cart.CartHeader.CartTotal > coupon.MinAmount) //the require amount to use coupon)
                    {
                        cart.CartHeader.CartTotal -= coupon.DiscountAmount;
                        cart.CartHeader.Discount = coupon.DiscountAmount;
                    }
                    else
                    {
                        _res.IsSuccess = false;
                        _res.Message = "Your order does not meet the promotion requirements. Please checks your order and Try again.";
                    }

                }
                _res.Message = "The Coupon you just type in is not available, please try another one.";
            }


            _res.Result = cart;
        }
        catch (Exception ex)
        {

            _res.IsSuccess = false;
            _res.Message = ex.Message;
        }
        return _res;
    }

    [HttpPost("ApplyCoupon")]
    public async Task<ActionResult<ResponseDTO>> ApplyCoupon([FromBody] CartDTO cartDto)
    {
        try
        {
            if (cartDto?.CartHeader is null)
            {
                return BadRequest(new ResponseDTO { IsSuccess = false, Message = "Cart header is required." });
            }

            var currentUserId = GetCurrentUserId();
            var cartFromDb = await _db.CartHeaders.FirstOrDefaultAsync(u => u.UserId == currentUserId);
            if (cartFromDb is null)
            {
                return NotFound(new ResponseDTO { IsSuccess = false, Message = "Cart not found." });
            }

            cartFromDb.CouponCode = cartDto.CartHeader.CouponCode;
            //cartFromDb.CouponCode = cartDto.CartHeader.MinAmount;
            _db.CartHeaders.Update(cartFromDb);
            await _db.SaveChangesAsync();
            _res.Result = true;
        }
        catch (Exception ex)
        {
            _res.IsSuccess = false;
            _res.Message = ex.ToString();
        }
        return _res;
    }

    [HttpPost("EmailCartRequest")]
    public async Task<object> EmailCartRequest([FromBody] CartDTO cartDto)
    {
        try
        {
            await _messageBus.PublishMessageAsync(cartDto, _emailShoppingCartQueue);
            _res.Result = true;
        }
        catch (Exception ex)
        {
            _res.IsSuccess = false;
            _res.Message = "MessageBusFailure: " + ex.ToString();
        }
        return _res;
    }


    [HttpPost("RemoveCoupon")]
    public async Task<ActionResult<ResponseDTO>> RemoveCoupon([FromBody] CartDTO cartDTO)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var cartFromDb = await _db.CartHeaders.FirstOrDefaultAsync(u => u.UserId == currentUserId);
            if (cartFromDb is null)
            {
                return NotFound(new ResponseDTO { IsSuccess = false, Message = "Cart not found." });
            }
            cartFromDb.CouponCode = "";
            _db.CartHeaders.Update(cartFromDb);
            await _db.SaveChangesAsync();
            _res.Result = true;
        }
        catch (Exception ex)
        {

            _res.IsSuccess = false;
            _res.Message = ex.ToString();
        }
        return _res;
    }


    [HttpPost("CartUpsert")]
    public async Task<ActionResult<ResponseDTO>> CartUpsert(CartDTO cartDTO)
    {
        try
        {
            if (cartDTO?.CartHeader is null || cartDTO.CartDetails is null || !cartDTO.CartDetails.Any())
            {
                return BadRequest(new ResponseDTO { IsSuccess = false, Message = "Cart header and at least one cart item are required." });
            }

            cartDTO.CartHeader.UserId = GetCurrentUserId();

            var cartHeaderFromDb = await _db.CartHeaders
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserId == cartDTO.CartHeader.UserId);
            if (cartHeaderFromDb == null)
            {
                // Create new CartHeader and CartDetails
                CartHeader cartHeader = _mapper.Map<CartHeader>(cartDTO.CartHeader);
                _db.CartHeaders.Add(cartHeader);
                await _db.SaveChangesAsync();
                cartDTO.CartDetails.First().CartHeaderId = cartHeader.CartHeaderId;
                _db.CartDetails.Add(_mapper.Map<CartDetails>(cartDTO.CartDetails.First()));
                await _db.SaveChangesAsync();
            }
            else
            {
                // Find CartDetails, check if it has the same Product
                var cartDetailsFromDb = await _db.CartDetails
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u =>
                        u.ProductId == cartDTO.CartDetails.First().ProductId &&
                        u.CartHeaderId == cartHeaderFromDb.CartHeaderId);
                if (cartDetailsFromDb == null)
                {
                    // Create new CartDetails
                    cartDTO.CartDetails.First().CartHeaderId = cartHeaderFromDb.CartHeaderId;
                    _db.CartDetails.Add(_mapper.Map<CartDetails>(cartDTO.CartDetails.First()));
                    await _db.SaveChangesAsync();
                }
                else
                {
                    // Update count in CartDetails
                    cartDTO.CartDetails.First().Count += cartDetailsFromDb.Count;
                    cartDTO.CartDetails.First().CartHeaderId = cartDetailsFromDb.CartHeaderId;
                    cartDTO.CartDetails.First().CartDetailsId = cartDetailsFromDb.CartDetailsId;
                    _db.CartDetails.Update(_mapper.Map<CartDetails>(cartDTO.CartDetails.First()));
                    await _db.SaveChangesAsync();
                }
            }

            // Try to publish message to message bus
            try
            {
                await _messageBus.PublishMessageAsync(cartDTO,
                    _emailShoppingCartQueue);
            }
            catch (Exception messageBusEx)
            {
                // Log the message bus error but don't fail the operation
                return Ok(new ResponseDTO
                {
                    IsSuccess = true,
                    Result = cartDTO,
                    Message = "Cart updated successfully with notification delay",
                    ErrorMessages = new List<string> { "MessageBusFailure: " + messageBusEx.Message }
                });
            }

            return Ok(new ResponseDTO
            {
                IsSuccess = true,
                Result = cartDTO,
                Message = "Cart updated successfully"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ResponseDTO
            {
                IsSuccess = false,
                Message = "Failed to update cart",
                ErrorMessages = new List<string> { ex.Message }
            });
        }
    }



    [HttpPost("RemoveCart")]
    public async Task<ActionResult<ResponseDTO>> RemoveCart([FromBody] int cartDetailsId)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            CartDetails? cartDetails = await _db.CartDetails
                .Include(u => u.CartHeader)
                .FirstOrDefaultAsync(u => u.CartDetailsId == cartDetailsId && u.CartHeader.UserId == currentUserId);

            if (cartDetails is null)
            {
                return NotFound(new ResponseDTO { IsSuccess = false, Message = "Cart item not found." });
            }

            int totalIteminCart = _db.CartDetails
                .Where(u => u.CartHeaderId == cartDetails.CartHeaderId).Count();

            _db.CartDetails.Remove(cartDetails);

            if (totalIteminCart == 1)
            {
                //
                _db.CartHeaders.Remove(cartDetails.CartHeader);
            }
            await _db.SaveChangesAsync();
            _res.Result = true;

        }
        catch (Exception ex)
        {
            _res.Message = ex.Message.ToString();
            _res.IsSuccess = false;
        }
        return _res;
    }



    private string GetCurrentUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? throw new InvalidOperationException("Authenticated user id is missing.");
    }
}
