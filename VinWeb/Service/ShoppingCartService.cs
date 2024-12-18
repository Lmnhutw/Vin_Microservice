using Vin.Web.Models;
using Vin.Web.Models.CartModels;
using Vin.Web.Service.IService;
using Vin.Web.Utility;

namespace Vin.Web.Service
{
    public class ShoppingCartService : IShoppingCartService
    {
        private readonly IBaseService _baseService;

        public ShoppingCartService(IBaseService baseService)
        {
            _baseService = baseService;
        }

        public async Task<ResponseDTO?> ApplyCouponAsync(CartDTO cartDTO)
        {
            return await _baseService.SendAsync(new RequestDTO()
            {
                ApiType = StaticDetail.ApiType.POST,
                Data = cartDTO,
                Url = $"{StaticDetail.ShoppingCartAPIBase}/api/Cart/ApplyCoupon"
            });
        }

        public async Task<ResponseDTO?> EmailCart(CartDTO cartDTO)
        {
            return await _baseService.SendAsync(new RequestDTO()
            {
                ApiType = StaticDetail.ApiType.POST,
                Data = cartDTO,
                Url = $"{StaticDetail.ShoppingCartAPIBase}/api/Cart/EmailCartRequest"
            });
        }

        public async Task<ResponseDTO?> GetCartByUserIdAsync(string userId)
        {
            return await _baseService.SendAsync(new RequestDTO()
            {
                ApiType = StaticDetail.ApiType.GET,
                Url = $"{StaticDetail.ShoppingCartAPIBase}/api/Cart/GetCart/{userId}"
            });
        }



        public async Task<ResponseDTO?> RemoveFromCartAsync(int cartDetailsId)
        {
            return await _baseService.SendAsync(new RequestDTO()
            {
                ApiType = StaticDetail.ApiType.POST,
                Data = cartDetailsId,
                Url = $"{StaticDetail.ShoppingCartAPIBase}/api/Cart/RemoveCart"
            });
        }


        public async Task<ResponseDTO?> UpsertCartAsync(CartDTO cartDTO)
        {
            return await _baseService.SendAsync(new RequestDTO()
            {
                ApiType = StaticDetail.ApiType.POST,
                Data = cartDTO,
                Url = $"{StaticDetail.ShoppingCartAPIBase}/api/Cart/CartUpsert"
            });
        }
    }
}