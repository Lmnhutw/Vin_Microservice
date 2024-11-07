using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NToastNotify;
using Vin.Web.Models;
using Vin.Web.Service.IService;

namespace Vin.Web.Controllers
{
    public class CouponController : Controller
    {
        private readonly ICouponService _couponService;
        private readonly ILogger<CouponController> _logger;
        private readonly IToastNotification _toastNotification;

        public CouponController(
            ICouponService couponService,
            ILogger<CouponController> logger,
            IToastNotification toastNotification
        )
        {
            _couponService = couponService;
            _logger = logger;
            _toastNotification = toastNotification; // Initialize toast notification
        }

        public async Task<IActionResult> CouponIndex()
        {
            List<CouponDTO>? list = new();
            ResponseDTO? response = await _couponService.GetAllCouponAsync();
            if (response != null && response.IsSuccess)
            {
                list = JsonConvert.DeserializeObject<List<CouponDTO>>(Convert.ToString(response.Result));
                //_toastNotification.AddSuccessToastMessage("Loading successfully");
            }
            else
            {
                _toastNotification.AddErrorToastMessage(response?.Message ?? "Failed to load coupons");
            }

            return View(list);
        }

        public async Task<IActionResult> CouponCreate()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                return View("Error", new { message = "Unable to load the Coupon creation page. Please try again later." + ex });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CouponCreate(CouponDTO model)
        {
            if (ModelState.IsValid)
            {
                ResponseDTO? response = await _couponService.CreateCouponsAsync(model);
                if (response != null && response.IsSuccess)
                {
                    _toastNotification.AddSuccessToastMessage("Create successfully");
                    return RedirectToAction(nameof(CouponIndex));
                }
            }
            return View(model);
        }

        // GET: This shows the delete confirmation page
        // [HttpGet]
        /*public async Task<IActionResult> CouponDelete(int couponId)
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
        }*/

        // POST: This actually performs the delete operation
        /* [HttpPost]
         [ValidateAntiForgeryToken]
         public async Task<IActionResult> CouponDelete(CouponDTO couponDTO)
         {
             _logger.LogInformation("POST CouponDelete called with CouponId: {CouponId}", couponDTO.CouponId);

             try
             {
                 ResponseDTO? response = await _couponService.DeleteCouponAsync(couponDTO.CouponId);

                 if (response != null && response.IsSuccess)
                 {
                     _logger.LogInformation("Coupon with ID {CouponId} deleted successfully", couponDTO.CouponId);
                     TempData["Success"] = "Coupon deleted successfully";
                     return RedirectToAction(nameof(CouponIndex));
                 }

                 _logger.LogWarning("Failed to delete coupon with ID {CouponId}. Message: {Message}",
                     couponDTO.CouponId, response?.Message);
                 TempData["Error"] = response?.Message ?? "Failed to delete coupon";
             }
             catch (Exception ex)
             {
                 _logger.LogError(ex, "Error deleting coupon with ID {CouponId}", couponDTO.CouponId);
                 TempData["Error"] = "An error occurred while deleting the coupon";
             }

             return RedirectToAction(nameof(CouponIndex));
         }*/

        public async Task<IActionResult> CouponDelete(int couponId)
        {
            ResponseDTO? response = await _couponService.GetCouponByIdAsync(couponId);

            if (response != null && response.IsSuccess)
            {
                CouponDTO? model = JsonConvert.DeserializeObject<CouponDTO>(Convert.ToString(response.Result));
                return View(model);
            }
            else
            {
                _toastNotification.AddErrorToastMessage(response?.Message ?? "Failed to load coupons");
            }
            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> CouponDelete(CouponDTO couponDto)
        {
            try
            {
                ResponseDTO? response = await _couponService.DeleteCouponAsync(couponDto.CouponId);

                if (response != null && response.IsSuccess)
                {
                    _toastNotification.AddSuccessToastMessage("Coupon deleted successfully");
                    return RedirectToAction(nameof(CouponIndex));
                }
                else
                {
                    _toastNotification.AddErrorToastMessage(response?.Message ?? "Failed to delete coupon");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting coupon with ID {CouponId}", couponDto.CouponId);
                _toastNotification.AddErrorToastMessage("An error occurred while deleting the coupon");
            }

            return View(couponDto);
        }
    }
}