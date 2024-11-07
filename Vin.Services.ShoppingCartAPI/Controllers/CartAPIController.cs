using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vin.Services.ShoppingCartAPI.Data;
using Vin.Services.ShoppingCartAPI.Models;
using Vin.Services.ShoppingCartAPI.Models.DTO;
using Vin.Services.ShoppingCartAPI.Service.IService;

[Route("api/cart")]
[ApiController]
public class CartAPIController : ControllerBase
{
    private readonly ResponseDTO _res;
    private readonly IMapper _mapper;
    private readonly AppDbContext _db;
    private readonly IProductService _productService;


    public CartAPIController(AppDbContext db, IMapper mapper, IProductService productService)
    {
        _db = db;
        _res = new ResponseDTO();
        _mapper = mapper;
        _productService = productService;
    }

    [HttpGet("GetCart/{userID}")]
    public async Task<ResponseDTO> GetCart(string userID)
    {
        try
        {
            CartDTO cart = new()
            {
                CartHeader = _mapper.Map<CartHeaderDTO>(_db.CartHeader.First(u => u.UserId == userID)),
            };
            cart.CartDetails = _mapper.Map<IEnumerable<CartDetailsDTO>>(_db.CartDetails
                .Where(u => u.CartHeaderId == cart.CartHeader.CartHeaderId));

            IEnumerable<ProductDTO> productDTOs = await _productService.GetProducts();

            foreach (var item in cart.CartDetails)
            {
                item.Product = productDTOs.FirstOrDefault(u => u.ProductId == item.ProductId);
                cart.CartHeader.CartTotal += (item.Count * item.Product.Price);
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
    public async Task<object> ApplyCoupon([FromBody] CartDTO cartDTO)
    {
        try
        {
            var cartFromDb = await _db.CartHeader.FirstAsync(u => u.UserId == cartDTO.CartHeader.UserId);
            cartFromDb.CouponCode = cartDTO.CartHeader.CouponCode;
            _db.CartHeader.Update(cartFromDb);
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

    [HttpPost("RemoveCoupon")]
    public async Task<object> RemoveCoupon([FromBody] CartDTO cartDTO)
    {
        try
        {
            var cartFromDb = await _db.CartHeader.FirstAsync(u => u.UserId == cartDTO.CartHeader.UserId);
            cartFromDb.CouponCode = "";
            _db.CartHeader.Update(cartFromDb);
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
            var cartHeaderFromDb = await _db.CartHeader
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserId == cartDTO.CartHeader.UserId);
            if (cartHeaderFromDb == null)
            {
                //Create new CH and CD
                CartHeader cartHeader = _mapper.Map<CartHeader>(cartDTO.CartHeader);
                _db.CartHeader.Add(cartHeader);
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


    [HttpPost("CartRemove")]
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
                var cartHeaderToRemove = await _db.CartHeader.FirstOrDefaultAsync(u => u.CartHeaderId == cartDetails.CartHeaderId);
                _db.CartHeader.Remove(cartHeaderToRemove);
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