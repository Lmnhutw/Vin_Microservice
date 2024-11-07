using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NToastNotify;
using Vin.Web.Models;
using Vin.Web.Service.IService;

namespace VinWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IToastNotification _toastNotification;
        private readonly IProductService _productService;

        public HomeController(ILogger<HomeController> logger, IToastNotification toastNotification, IProductService productService)
        {
            _logger = logger;
            _toastNotification = toastNotification;
            _productService = productService;
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