using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vin.MessageBus;
using Vin.Services.ShoppingCartAPI.Data;
using Vin.Services.ShoppingCartAPI.Models;
using Vin.Services.ShoppingCartAPI.Models.DTO;
using Vin.Services.ShoppingCartAPI.Service.IService;

[Route("api/Cart")]
[ApiController]
public class CartAPIController : ControllerBase
{
    private readonly ResponseDTO _res;
    private readonly IMapper _mapper;
    private readonly AppDbContext _db;
    private readonly IProductService _productService;
    private readonly ICouponService _couponService;
    private readonly IMessageBus _messageBus;
    private readonly IConfiguration _configuration;


    public CartAPIController(AppDbContext db, IMapper mapper, IProductService productService, ICouponService couponService, IMessageBus messageBus, IConfiguration configuration)
    {
        _db = db;
        _res = new ResponseDTO();
        _mapper = mapper;
        _productService = productService;
        _couponService = couponService;
        _messageBus = messageBus;
        _configuration = configuration;
    }

    [HttpGet("GetCart/{userID}")]
    public async Task<ResponseDTO> GetCart(string userID)
    {
        try
        {
            CartDTO cart = new()
            {
                CartHeader = _mapper.Map<CartHeaderDTO>(_db.CartHeaders.First(u => u.UserId == userID)),
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
    public async Task<object> ApplyCoupon([FromBody] CartDTO cartDto)
    {
        try
        {
            var cartFromDb = await _db.CartHeaders.FirstAsync(u => u.UserId == cartDto.CartHeader.UserId);

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

            await _messageBus.PublishMessage(cartDto, _configuration.GetValue<string>("TopicAndQueueNames:EmailShoppingCartQueue"));
            _res.Result = true;
        }
        catch (Exception ex)
        {
            _res.IsSuccess = false;
            _res.Message = ex.ToString();
        }
        return _res;
    }

    [HttpPost("RemoveCoupon")]
    public async Task<object> RemoveCoupon([FromBody] CartDTO cartDTO)
    {
        try
        {
            var cartFromDb = await _db.CartHeaders.FirstAsync(u => u.UserId == cartDTO.CartHeader.UserId);
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
    public async Task<ResponseDTO> CartUpsert(CartDTO cartDTO)
    {
        try
        {
            var cartHeaderFromDb = await _db.CartHeaders
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserId == cartDTO.CartHeader.UserId);
            if (cartHeaderFromDb == null)
            {
                //Create new CH and CD
                CartHeader cartHeader = _mapper.Map<CartHeader>(cartDTO.CartHeader);
                _db.CartHeaders.Add(cartHeader);
                await _db.SaveChangesAsync();
                cartDTO.CartDetails.First().CartHeaderId = cartHeader.CartHeaderId;
                _db.CartDetails.Add(_mapper.Map<CartDetails>(cartDTO.CartDetails.First()));
                await _db.SaveChangesAsync();
            }
            else
            {
                //find CD, check if D has same Prd     
                var cartDetailsFromDb = await _db.CartDetails
                    .AsNoTracking().
                    FirstOrDefaultAsync(u =>
                    u.ProductId == cartDTO.CartDetails.First().ProductId &&
                    u.CartHeaderId == cartHeaderFromDb.CartHeaderId);
                if (cartDetailsFromDb == null)
                {
                    //create new CD
                    cartDTO.CartDetails.First().CartHeaderId = cartHeaderFromDb.CartHeaderId;
                    _db.CartDetails.Add(_mapper.Map<CartDetails>(cartDTO.CartDetails.First()));
                    await _db.SaveChangesAsync();
                }
                else
                {
                    //update count in CD
                    cartDTO.CartDetails.First().Count += cartDetailsFromDb.Count;
                    cartDTO.CartDetails.First().CartHeaderId = cartDetailsFromDb.CartHeaderId;
                    cartDTO.CartDetails.First().CartDetailsId = cartDetailsFromDb.CartDetailsId;
                    _db.CartDetails.Update(_mapper.Map<CartDetails>(cartDTO.CartDetails.First()));
                    await _db.SaveChangesAsync();
                }
            }
            _res.Result = cartDTO;
        }
        catch (Exception ex)
        {
            _res.Message = ex.Message.ToString();
            _res.IsSuccess = false;
        }
        return _res;
    }


    [HttpPost("RemoveCart")]
    public async Task<ResponseDTO> RemoveCart([FromBody] int cartDetailsId)
    {
        try
        {
            CartDetails cartDetails = await _db.CartDetails.FirstOrDefaultAsync(u => u.CartDetailsId == cartDetailsId);

            int totalIteminCart = _db.CartDetails
                .Where(u => u.CartHeaderId == cartDetails.CartHeaderId).Count();

            _db.CartDetails.Remove(cartDetails);

            if (totalIteminCart == 1)
            {
                //
                var cartHeaderToRemove = await _db.CartHeaders.FirstOrDefaultAsync(u => u.CartHeaderId == cartDetails.CartHeaderId);
                _db.CartHeaders.Remove(cartHeaderToRemove);
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



}