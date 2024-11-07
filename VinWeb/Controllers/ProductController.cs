using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NToastNotify;
using Vin.Web.Models;
using Vin.Web.Service.IService;

namespace Vin.Web.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly ILogger<ProductController> _logger;
        private readonly IToastNotification _toastNotification;

        public ProductController(
            IProductService productService,
            ILogger<ProductController> logger,
            IToastNotification toastNotification
        )
        {
            _productService = productService;
            _logger = logger;
            _toastNotification = toastNotification; // Initialize toast notification
        }

        public async Task<IActionResult> ProductIndex()
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

        public async Task<IActionResult> ProductCreate()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                return View("Error", new { message = "Unable to load the Product creation page. Please try again later." + ex });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ProductCreate(ProductDTO model)
        {
            if (ModelState.IsValid)
            {
                ResponseDTO? response = await _productService.CreateProductsAsync(model);
                if (response != null && response.IsSuccess)
                {
                    _toastNotification.AddSuccessToastMessage("Create successfully");
                    return RedirectToAction(nameof(ProductIndex));
                }
            }
            return View(model);
        }

        // GET: This shows the delete confirmation page
        // [HttpGet]
        /*public async Task<IActionResult> ProductDelete(int productId)
        {
            _logger.LogInformation("GET ProductDelete called with productId: {ProductId}", productId);

            try
            {
                ResponseDTO? response = await _productService.GetProductByIdAsync(productId);

                if (response != null && response.IsSuccess && response.Result != null)
                {
                    ProductDTO? model = JsonConvert.DeserializeObject<ProductDTO>(Convert.ToString(response.Result));
                    return View(model);
                }

                TempData["Error"] = "Product not found";
                return RedirectToAction(nameof(ProductIndex));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GET ProductDelete for ID: {ProductId}", productId);
                TempData["Error"] = "Error loading product details";
                return RedirectToAction(nameof(ProductIndex));
            }
        }*/

        // POST: This actually performs the delete operation
        /* [HttpPost]
         [ValidateAntiForgeryToken]
         public async Task<IActionResult> ProductDelete(ProductDTO productDTO)
         {
             _logger.LogInformation("POST ProductDelete called with ProductId: {ProductId}", productDTO.ProductId);

             try
             {
                 ResponseDTO? response = await _productService.DeleteProductAsync(productDTO.ProductId);

                 if (response != null && response.IsSuccess)
                 {
                     _logger.LogInformation("Product with ID {ProductId} deleted successfully", productDTO.ProductId);
                     TempData["Success"] = "Product deleted successfully";
                     return RedirectToAction(nameof(ProductIndex));
                 }

                 _logger.LogWarning("Failed to delete product with ID {ProductId}. Message: {Message}",
                     productDTO.ProductId, response?.Message);
                 TempData["Error"] = response?.Message ?? "Failed to delete product";
             }
             catch (Exception ex)
             {
                 _logger.LogError(ex, "Error deleting product with ID {ProductId}", productDTO.ProductId);
                 TempData["Error"] = "An error occurred while deleting the product";
             }

             return RedirectToAction(nameof(ProductIndex));
         }*/

        public async Task<IActionResult> ProductDelete(int productId)
        {
            ResponseDTO? response = await _productService.GetProductByIdAsync(productId);

            if (response != null && response.IsSuccess)
            {
                ProductDTO? model = JsonConvert.DeserializeObject<ProductDTO>(Convert.ToString(response.Result));
                return View(model);
            }
            else
            {
                _toastNotification.AddErrorToastMessage(response?.Message ?? "Failed to load products");
            }
            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> ProductDelete(ProductDTO productDto)
        {
            try
            {
                ResponseDTO? response = await _productService.DeleteProductAsync(productDto.ProductId);

                if (response != null && response.IsSuccess)
                {
                    _toastNotification.AddSuccessToastMessage("Product deleted successfully");
                    return RedirectToAction(nameof(ProductIndex));
                }
                else
                {
                    _toastNotification.AddErrorToastMessage(response?.Message ?? "Failed to delete product");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product with ID {ProductId}", productDto.ProductId);
                _toastNotification.AddErrorToastMessage("An error occurred while deleting the product");
            }

            return View(productDto);
        }


        public async Task<IActionResult> ProductEdit(int productId)
        {
            ResponseDTO? response = await _productService.GetProductByIdAsync(productId);

            if (response != null && response.IsSuccess)
            {
                ProductDTO? model = JsonConvert.DeserializeObject<ProductDTO>(Convert.ToString(response.Result));
                return View(model);
            }
            else
            {
                _toastNotification.AddErrorToastMessage(response?.Message ?? "Failed to load products");
            }
            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> ProductEdit(ProductDTO productDto)
        {
            try
            {
                ResponseDTO? response = await _productService.UpdateProductsAsync(productDto);

                if (response != null && response.IsSuccess)
                {
                    _toastNotification.AddSuccessToastMessage("Product edited successfully");
                    return RedirectToAction(nameof(ProductIndex));
                }
                else
                {
                    _toastNotification.AddErrorToastMessage(response?.Message ?? "Failed to edit product");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error editing product with ID {ProductId}", productDto.ProductId);
                _toastNotification.AddErrorToastMessage("An error occurred while editing the product");
            }

            return View(productDto);
        }
    }
}