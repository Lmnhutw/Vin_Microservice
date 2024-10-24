using Vin.Web.Models;

namespace Vin.Web.Service.IService
{
    public interface ICouponService
    {
        Task<ResponseDTO?> GetCouponAsync(string couponCode);

        Task<ResponseDTO?> GetAllCouponAsync();

        Task<ResponseDTO?> GetCouponByIdAsync(int id);

        Task<ResponseDTO?> CreateCouponsAsync(CouponDTO couponDTO);

        Task<ResponseDTO?> UpdateCouponsAsync(CouponDTO couponDTO);

        Task<ResponseDTO?> DeleteCouponAsync(int id);
    }
}