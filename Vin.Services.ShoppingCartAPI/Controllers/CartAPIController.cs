using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vin.Services.ShoppingCartAPI.Data;
using Vin.Services.ShoppingCartAPI.Models;
using Vin.Services.ShoppingCartAPI.Models.DTO;

[Route("api/cart")]
[ApiController]
public class CartAPIController : ControllerBase
{
    private readonly ResponseDTO _res;
    private readonly IMapper _mapper;
    private readonly AppDbContext _db;

    public CartAPIController(AppDbContext db, IMapper mapper)
    {
        _db = db;
        _res = new ResponseDTO();
        _mapper = mapper;
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
}