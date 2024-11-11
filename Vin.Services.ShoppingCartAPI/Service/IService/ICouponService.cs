
using Vin.Services.ShoppingCartAPI.Models.DTO;

namespace Vin.Services.ShoppingCartAPI.Service.IService
{
    public interface ICouponService
    {
        Task<CouponDTO> GetCoupon(string couponCode);
    }
}
