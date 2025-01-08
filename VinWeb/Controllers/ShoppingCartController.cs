using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NToastNotify;
using Vin.Web.Models.CartModels;
using Vin.Web.Service.IService;

namespace Vin.Web.Controllers
{
    public class ShoppingCartController : Controller
    {
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IToastNotification _toastNotification;
        public ShoppingCartController(IShoppingCartService shoppingCartService, IToastNotification toastNotification)
        {
            _shoppingCartService = shoppingCartService;
            _toastNotification = toastNotification;
        }

        // [Authorize]
        public async Task<IActionResult> CartIndex()
        {
            return View(await LoadCartDTOBasedOnLoggedInUser());
        }

        public async Task<IActionResult> RemoveProduct(int cartDetailsId)
        {
            var userId = User.Claims.Where(u => u.Type == JwtRegisteredClaimNames.Sub)?.FirstOrDefault()?.Value;
            ResponseDTO response = await _shoppingCartService.RemoveFromCartAsync(cartDetailsId);
            if (response != null && response.IsSuccess)
            {
                _toastNotification.AddSuccessToastMessage("Product Removed!!!");
                return RedirectToAction(nameof(CartIndex));
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ApplyCoupon(CartDTO cartDTO)
        {
            ResponseDTO? response = await _shoppingCartService.ApplyCouponAsync(cartDTO);
            if (response != null && response.IsSuccess)
            {
                _toastNotification.AddSuccessToastMessage("Coupon Applied Successfully!");
            }
            else
            {
                _toastNotification.AddErrorToastMessage(response?.Message ?? "Failed to apply coupon");
                return View();
            }
            return RedirectToAction(nameof(CartIndex));
        }

        [HttpPost]
        public async Task<IActionResult> EmailCart(CartDTO cartDTO)
        {
            CartDTO cart = await LoadCartDTOBasedOnLoggedInUser();
            cart.CartHeader.Email = User.Claims.Where(u => u.Type == JwtRegisteredClaimNames.Email)?.FirstOrDefault()?.Value;
            ResponseDTO? response = await _shoppingCartService.EmailCart(cart);
            if (response != null && response.IsSuccess)
            {
                _toastNotification.AddSuccessToastMessage("Email is sending...");
            }
            else
            {
                _toastNotification.AddErrorToastMessage(response?.Message ?? "Failed to sent email");
                return View("CartIndex");
            }
            return RedirectToAction(nameof(CartIndex));
        }

        [HttpPost]
        public async Task<IActionResult> RemoveCoupon(CartDTO cartDTO)
        {
            cartDTO.CartHeader.CouponCode = "";
            ResponseDTO? response = await _shoppingCartService.ApplyCouponAsync(cartDTO);
            if (response != null && response.IsSuccess)
            {
                _toastNotification.AddSuccessToastMessage("Coupon Remove Successfully!");
            }
            else
            {
                _toastNotification.AddErrorToastMessage(response?.Message ?? "Failed to remove coupon");
                return View();
            }
            return RedirectToAction(nameof(CartIndex));
        }

        private async Task<CartDTO> LoadCartDTOBasedOnLoggedInUser()
        {
            var userId = User.Claims.Where(u => u.Type == JwtRegisteredClaimNames.Sub)?.FirstOrDefault()?.Value;
            ResponseDTO response = await _shoppingCartService.GetCartByUserIdAsync(userId);
            if (response != null && response.IsSuccess)
            {
                CartDTO cartDTO = JsonConvert.DeserializeObject<CartDTO>(Convert.ToString(response.Result));
                return cartDTO;
            }
            return new CartDTO();
        }


    }
}
