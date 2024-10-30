using Microsoft.AspNetCore.Mvc;
using Vin.Services.AuthAPI.Models.DTO;
using Vin.Services.AuthAPI.Services.IServices;

namespace Vin.Services.AuthAPI.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthAPIControllers : ControllerBase
    {
        private readonly IAuthService _authService;
        protected ResponseDTO _res;

        public AuthAPIControllers(IAuthService authService)
        {
            _authService = authService;
            _res = new();
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegistrationRequestDTO model)
        {
            var errorMessage = await _authService.Register(model);
            if (!string.IsNullOrEmpty(errorMessage))
            {
                _res.IsSuccess = false;
                _res.Message = errorMessage;
                return BadRequest(_res);
            }
            return Ok(_res);
        }

        [HttpPost("logn")]
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
    }
}