using System.Diagnostics;
using IdentityModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NToastNotify;
using Vin.Web.Models;
using Vin.Web.Models.CartModels;
using Vin.Web.Service.IService;

namespace VinWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IToastNotification _toastNotification;
        private readonly IProductService _productService;
        private readonly IShoppingCartService _shoppingCartService;

        public HomeController(ILogger<HomeController> logger, IToastNotification toastNotification, IProductService productService, IShoppingCartService shoppingCartService)
        {
            _logger = logger;
            _toastNotification = toastNotification;
            _productService = productService;
            _shoppingCartService = shoppingCartService;
        }

        public async Task<IActionResult> Index()
        {
            List<ProductDTO>? list = new();
            ResponseDTO? response = await _productService.GetAllProductAsync();
            if (response != null && response.IsSuccess)
            {
                list = JsonConvert.DeserializeObject<List<ProductDTO>>(Convert.ToString(response.Result));
                //_toastNotification.AddSuccessToastMessage("Loading successfully");
            }
            else
            {
                _toastNotification.AddErrorToastMessage(response?.Message ?? "Failed to load products");
            }

            return View(list);
        }


        [Authorize]
        public async Task<IActionResult> ProductDetails(int productId)
        {
            ProductDTO model = new();
            ResponseDTO? response = await _productService.GetProductByIdAsync(productId);
            if (response != null && response.IsSuccess)
            {
                model = JsonConvert.DeserializeObject<ProductDTO>(Convert.ToString(response.Result));
                //_toastNotification.AddSuccessToastMessage("Loading successfully");
            }
            else
            {
                _toastNotification.AddErrorToastMessage(response?.Message ?? "Failed to load product information");
            }

            return View(model);
        }

        [Authorize]
        [HttpPost]
        [ActionName("ProductDetails")]
        public async Task<IActionResult> ProductDetails(ProductDTO productDTO)
        {
            CartDTO cartDTO = new CartDTO()
            {
                CartHeader = new CartHeaderDTO
                {
                    UserId = User.Claims.Where(u => u.Type == JwtClaimTypes.Subject)?.FirstOrDefault()?.Value
                }
            };

            CartDetailsDTO cartDetails = new CartDetailsDTO()
            {
                Count = productDTO.Count,
                ProductId = productDTO.ProductId,
            };

            List<CartDetailsDTO> cartDetailsDTOs = new()
            {
                cartDetails
            };
            cartDTO.CartDetails = cartDetailsDTOs;

            ResponseDTO? response = await _shoppingCartService.UpsertCartAsync(cartDTO);

            if (response != null && response.IsSuccess)
            {
                _toastNotification.AddSuccessToastMessage(response?.Message ?? "Loading successfully");
                return RedirectToAction(nameof(Index));
            }
            else
            {
                _toastNotification.AddErrorToastMessage(response?.Message ?? "Failed to load product information");
            }

            return View(productDTO);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}