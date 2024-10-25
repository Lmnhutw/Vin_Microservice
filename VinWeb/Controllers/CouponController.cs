using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Vin.Web.Models;
using Vin.Web.Service.IService;

namespace Vin.Web.Controllers
{
    public class CouponController : Controller
    {
        private readonly ICouponService _couponService;
        private readonly ILogger<CouponController> _logger; // Add this line

        public CouponController
            (
            ICouponService couponService,
            ILogger<CouponController> logger
            ) // Modify constructor
        {
            _couponService = couponService;
            _logger = logger; // Initialize logger
        }

        public async Task<IActionResult> CouponIndex()
        {
            List<CouponDTO> list = new();
            ResponseDTO? response = await _couponService.GetAllCouponAsync();
            if (response != null && response.IsSuccess)
            {
                list = JsonConvert.DeserializeObject<List<CouponDTO>>(Convert.ToString(response.Result));
            }

            return View(list);
        }

        public async Task<IActionResult> CouponCreate()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CouponCreate(CouponDTO model)
        {
            if (ModelState.IsValid)
            {
                ResponseDTO? response = await _couponService.CreateCouponsAsync(model);
                if (response != null && response.IsSuccess)
                {
                    return RedirectToAction(nameof(CouponIndex));
                }
            }
            return View(model);
        }

        // GET: This shows the delete confirmation page
        [HttpGet]
        public async Task<IActionResult> CouponDelete(int couponId)
        {
            _logger.LogInformation("GET CouponDelete called with couponId: {CouponId}", couponId);

            try
            {
                ResponseDTO? response = await _couponService.GetCouponByIdAsync(couponId);

                if (response != null && response.IsSuccess && response.Result != null)
                {
                    CouponDTO? model = JsonConvert.DeserializeObject<CouponDTO>(Convert.ToString(response.Result));
                    return View(model);
                }

                TempData["Error"] = "Coupon not found";
                return RedirectToAction(nameof(CouponIndex));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GET CouponDelete for ID: {CouponId}", couponId);
                TempData["Error"] = "Error loading coupon details";
                return RedirectToAction(nameof(CouponIndex));
            }
        }

        // POST: This actually performs the delete operation
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CouponDeletePOST(int couponId)
        {
            _logger.LogInformation("POST CouponDelete called with CouponId: {CouponId}", couponId);

            try
            {
                ResponseDTO? response = await _couponService.DeleteCouponAsync(couponId);

                if (response != null && response.IsSuccess)
                {
                    _logger.LogInformation("Coupon with ID {CouponId} deleted successfully", couponId);
                    TempData["Success"] = "Coupon deleted successfully";
                    return RedirectToAction(nameof(CouponIndex));
                }

                _logger.LogWarning("Failed to delete coupon with ID {CouponId}. Message: {Message}",
                    couponId, response?.Message);
                TempData["Error"] = response?.Message ?? "Failed to delete coupon";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting coupon with ID {CouponId}", couponId);
                TempData["Error"] = "An error occurred while deleting the coupon";
            }

            return RedirectToAction(nameof(CouponIndex));
        }
    }
}