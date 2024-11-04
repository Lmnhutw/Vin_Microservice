using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
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
        private readonly ITokenProvider _tokenProvider;

        public AuthController(IAuthService authService, IToastNotification toastNotification, ITokenProvider tokenProvider)
        {
            _authService = authService;
            _toastNotification = toastNotification;
            _tokenProvider = tokenProvider;
        }

        [HttpGet]
        public IActionResult Login()
        {
            LoginRequestDTO loginRequestDTO = new();
            return View(loginRequestDTO);
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginRequestDTO login)
        {

            ResponseDTO responseDTO = await _authService.LoginAsync(login);


            if (responseDTO?.IsSuccess == true)
            {
                LoginResponseDTO loginResponseDTO = JsonConvert.DeserializeObject<LoginResponseDTO>(Convert.ToString(responseDTO.Result));
                await SignInUser(loginResponseDTO);
                _tokenProvider.SetToken(loginResponseDTO.Token);
                _toastNotification.AddSuccessToastMessage("Login Successfully!!!");
                return RedirectToAction("Index", "Home");

            }
            else
            {
                _toastNotification.AddErrorToastMessage("Password or Username are Incorrect, Please Try Again!!!");
                return View(login);
            }


        }
        [HttpGet]
        public IActionResult Register()
        {

            return View();
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
                    _toastNotification.AddSuccessToastMessage("Register Successfully!!!");
                    return RedirectToAction(nameof(Login));
                }
            }

            var roleList = new List<SelectListItem>()
            {
                new SelectListItem{Text=StaticDetail.RoleAdmin, Value=StaticDetail.RoleAdmin},
                new SelectListItem{Text=StaticDetail.RoleCustomer, Value=StaticDetail.RoleCustomer},

            };
            ViewBag.RoleList = roleList;
            return View(registration);

        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            _tokenProvider.ClearedToken();
            _toastNotification.AddSuccessToastMessage("Logout Successfully!!!");
            return RedirectToAction("Index", "Home");
        }
        public IActionResult ForgotPassword()
        {
            return View();
        }
        public IActionResult ResetPassword()
        {
            return View();
        }

        private async Task SignInUser(LoginResponseDTO model)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(model.Token);

            var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);

            identity.AddClaim(new Claim(JwtRegisteredClaimNames.Email,
                jwt.Claims.FirstOrDefault(u => u.Type == JwtRegisteredClaimNames.Email).Value));

            identity.AddClaim(new Claim(JwtRegisteredClaimNames.Sub,
                jwt.Claims.FirstOrDefault(u => u.Type == JwtRegisteredClaimNames.Sub).Value));

            identity.AddClaim(new Claim(JwtRegisteredClaimNames.Name,
                jwt.Claims.FirstOrDefault(u => u.Type == JwtRegisteredClaimNames.Name).Value));

            identity.AddClaim(new Claim(ClaimTypes.Name,
                jwt.Claims.FirstOrDefault(u => u.Type == JwtRegisteredClaimNames.Email).Value));

            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
        }
    }
}