using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using NToastNotify;
using Vin.Web.Models;

namespace VinWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IToastNotification _toastNotification;

        public HomeController(ILogger<HomeController> logger, IToastNotification toastNotification)
        {
            _logger = logger;
            _toastNotification = toastNotification;
        }

        public IActionResult Index()
        {
            _toastNotification.AddSuccessToastMessage("Welcome to the homepage!");
            return View();
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