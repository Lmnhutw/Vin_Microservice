using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NToastNotify;
using Vin.Web.Models;
using Vin.Web.Models.AuthModels;
using Vin.Web.Service.IService;
using Vin.Web.Utility;

namespace Vin.Web.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;
        private readonly IToastNotification _toastNotification;

        public AuthController(IAuthService authService, IToastNotification toastNotification)
        {
            _authService = authService;
            _toastNotification = toastNotification;
        }

        [HttpGet]
        public IActionResult Login()
        {
            LoginRequestDTO loginRequestDTO = new();
            return View(loginRequestDTO);
        }

        [HttpGet]
        public IActionResult RegisterRoleView()
        {
            var roleList = new List<SelectListItem>()
            {
                new SelectListItem{Text=StaticDetail.RoleAdmin, Value=StaticDetail.RoleAdmin},
                new SelectListItem{Text=StaticDetail.RoleCustomer, Value=StaticDetail.RoleCustomer},

            };
            ViewBag.RoleList = roleList;
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Register(RegistrationRequestDTO registration)
        {

            ResponseDTO result = await _authService.RegisterAsync(registration);
            ResponseDTO assigningRole;

            if (result != null && result.IsSuccess)
            {
                if (string.IsNullOrEmpty(registration.Role))
                {
                    registration.Role = StaticDetail.RoleCustomer;
                }
                assigningRole = await _authService.AssignRoleAsync(registration);
                if (assigningRole != null && assigningRole.IsSuccess)
                {
                    _toastNotification.AddSuccessToastMessage("Register Successfullyt!");
                    return RedirectToAction(nameof(Login));
                }
            }

            var roleList = new List<SelectListItem>()
            {
                new SelectListItem{Text=StaticDetail.RoleAdmin, Value=StaticDetail.RoleAdmin},
                new SelectListItem{Text=StaticDetail.RoleCustomer, Value=StaticDetail.RoleCustomer},

            };
            ViewBag.RoleList = roleList;
            return View();

        }

        public IActionResult Logout()
        {
            return View();
        }
        public IActionResult ForgotPassword()
        {
            return View();
        }
        public IActionResult ResetPassword()
        {
            return View();
        }
    }
}