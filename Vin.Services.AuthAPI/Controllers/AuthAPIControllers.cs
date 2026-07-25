using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Vin.Services.AuthAPI.Models;
using Vin.Services.AuthAPI.Models.DTO;
using Vin.Services.AuthAPI.Services.IServices;

namespace Vin.Services.AuthAPI.Controllers;

[Route("api/auth")]
[ApiController]
public sealed class AuthAPIControllers : ControllerBase
{
    private readonly IAuthService _authService;
    public AuthAPIControllers(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("Register")]
    public async Task<IActionResult> Register([FromBody] RegistrationRequestDTO model)
    {
        var user = await _authService.Register(model);
        if (user.ID is null)
        {
            return BadRequest(new ResponseDTO
            {
                IsSuccess = false,
                Message = "Registration failed."
            });
        }

        return Ok(new ResponseDTO { Result = user });
    }

    [HttpPost("Login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDTO model)
    {
        var login = await _authService.Login(model);
        if (login.User is null)
        {
            return BadRequest(new ResponseDTO
            {
                IsSuccess = false,
                Message = "Username or password incorrect."
            });
        }

        return Ok(new ResponseDTO { Result = login });
    }

    [HttpPost("AssignRole")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> AssignRole([FromBody] RegistrationRequestDTO model)
    {
        var success = await _authService.AssignRole(model.Email, model.Role);
        return success
            ? Ok(new ResponseDTO { Result = true })
            : BadRequest(new ResponseDTO { IsSuccess = false, Message = "Role assignment failed." });
    }

    [HttpPost("ForgotPassword")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDTO model)
    {
        await _authService.ForgotPassword(model);
        return Ok(new ResponseDTO
        {
            Message = "If the account exists, reset instructions will be sent."
        });
    }

    [HttpPost("ResetPassword")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDTO model)
    {
        var success = await _authService.ResetPassword(model);
        return success
            ? Ok(new ResponseDTO { Message = "Password reset successfully." })
            : BadRequest(new ResponseDTO { IsSuccess = false, Message = "Invalid token or email." });
    }
}
