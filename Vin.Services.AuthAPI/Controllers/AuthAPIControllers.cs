using System.Net;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Vin.Services.AuthAPI.Models;
using Vin.Services.AuthAPI.Models.DTO;
using Vin.Services.AuthAPI.Services.IServices;

namespace Vin.Services.AuthAPI.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthAPIControllers : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly UserManager<ApplicationUser> _userManager;
        protected ResponseDTO _res;

        public AuthAPIControllers(IAuthService authService, UserManager<ApplicationUser> userManager)
        {
            _authService = authService;
            _res = new();
            _userManager = userManager;
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] RegistrationRequestDTO model)
        {
            var userDTO = await _authService.Register(model);
            if (userDTO == null)
            {
                _res.IsSuccess = false;
                _res.Message = "Registration failed.";
                return BadRequest(_res);
            }
            _res.Result = userDTO;
            _res.IsSuccess = true;
            return Ok(_res);
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO model)
        {
            var loginResponse = await _authService.Login(model);
            if (loginResponse.User == null)
            {
                _res.IsSuccess = false;
                _res.Message = "Username or password incorrect";
                return BadRequest(_res);
            }
            _res.Result = loginResponse;
            return Ok(_res);
        }

        [HttpPost("AssignRole")]
        public async Task<IActionResult> AssignRole([FromBody] RegistrationRequestDTO model)
        {
            var assignRoleSuccessful = await _authService.AssignRole(model.Email, model.Role.ToUpper());
            if (!assignRoleSuccessful)
            {
                _res.IsSuccess = false;
                _res.Message = "Error Encoutered";
                return BadRequest(_res);
            }

            return Ok(_res);
        }
        [HttpPost("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDTO model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                _res.IsSuccess = false;
                _res.Message = "Invalid email address.";
                return BadRequest(_res);
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetLink = $"https://localhost:7163/reset-password?email={user.Email}&token={WebUtility.UrlEncode(token)}";

            //email

            var success = await _authService.ForgotPassword(model);
            if (!success == null)
            {
                _res.IsSuccess = false;
                _res.Message = "Invalid email address.";
                return BadRequest(_res);
            }
            _res.IsSuccess = true;
            return Ok(_res);
        }

        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDTO model)
        {

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                _res.IsSuccess = false;
                _res.Message = "Invalid email address.";
                return BadRequest(_res);
            }
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetLink = $"https://localhost:7163/reset-password?email={user.Email}&token={WebUtility.UrlEncode(token)}";

            //email 

            var success = await _authService.ResetPassword(model);
            if (!success)
            {
                _res.IsSuccess = false;
                _res.Message = "Invalid token or email.";
                return BadRequest(_res);
            }
            _res.IsSuccess = true;
            return Ok(_res);
        }


    }
}