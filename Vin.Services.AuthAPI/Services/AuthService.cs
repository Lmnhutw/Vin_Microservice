using Microsoft.AspNetCore.Identity;
using Vin.Services.AuthAPI.Data;
using Vin.Services.AuthAPI.Models;
using Vin.Services.AuthAPI.Models.DTO;
using Vin.Services.AuthAPI.Services.IServices;


namespace Vin.Services.AuthAPI.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _db;
        private readonly IJwtTokenGenerator _jwtTokenGenerateor;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;

        public AuthService
            (
            AppDbContext db,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IJwtTokenGenerator jwtTokenGenerator,
            IConfiguration configuration
            )
        {
            _db = db;
            _jwtTokenGenerateor = jwtTokenGenerator;
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
        }

        public async Task<bool> AssignRole(string email, string rolename)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(rolename))
            {
                return false;
            }

            var user = await _userManager.FindByEmailAsync(email.Trim());
            if (user is null)
            {
                return false;
            }

            var normalizedRole = rolename.Trim().ToUpperInvariant();
            if (!await _roleManager.RoleExistsAsync(normalizedRole))
            {
                var roleResult = await _roleManager.CreateAsync(new IdentityRole(normalizedRole));
                if (!roleResult.Succeeded)
                {
                    return false;
                }
            }

            var result = await _userManager.AddToRoleAsync(user, normalizedRole);
            return result.Succeeded;
        }

        public async Task<LoginResponseDTO> Login(LoginRequestDTO loginRequestDTO)
        {
            if (loginRequestDTO is null || string.IsNullOrWhiteSpace(loginRequestDTO.UserName) ||
                string.IsNullOrWhiteSpace(loginRequestDTO.Password))
            {
                return new LoginResponseDTO
                {
                    User = null!,
                    Token = ""
                };
            }

            var user = await _userManager.FindByNameAsync(loginRequestDTO.UserName.Trim());
            if (user is null || !await _userManager.CheckPasswordAsync(user, loginRequestDTO.Password))
            {
                return new LoginResponseDTO
                {
                    User = null!,
                    Token = ""
                };
            }



            //Generate Jwt token if user was found

            var roles = await _userManager.GetRolesAsync(user);
            var token = _jwtTokenGenerateor.GenerateToken(user, roles);

            UserDTO userDTO = new()
            {
                Email = user.Email,
                ID = user.Id,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber
            };

            LoginResponseDTO loginResponseDTO = new()
            {
                User = userDTO,
                Token = token
            };
            return loginResponseDTO;
        }

        public async Task<UserDTO> Register(RegistrationRequestDTO registrationRequestDTO)
        {
            if (registrationRequestDTO is null)
            {
                return new UserDTO();
            }

            ApplicationUser user = new()
            {
                UserName = registrationRequestDTO.Email.Trim(),
                Email = registrationRequestDTO.Email.Trim(),
                NormalizedEmail = registrationRequestDTO.Email.Trim().ToUpperInvariant(),
                FullName = registrationRequestDTO.FullName?.Trim() ?? string.Empty,
                PhoneNumber = registrationRequestDTO.PhoneNumber
            };

            var result = await _userManager.CreateAsync(user, registrationRequestDTO.Password);
            if (!result.Succeeded)
            {
                return new UserDTO();
            }

            const string customerRole = "CUSTOMER";
            if (!await _roleManager.RoleExistsAsync(customerRole))
            {
                await _roleManager.CreateAsync(new IdentityRole(customerRole));
            }

            await _userManager.AddToRoleAsync(user, customerRole);
            return new UserDTO
            {
                Email = user.Email,
                ID = user.Id,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber
            };
        }

        public async Task<bool> ForgotPassword(ForgotPasswordDTO forgotPasswordDTO)
        {
            var user = await _userManager.FindByEmailAsync(forgotPasswordDTO.Email);
            if (user == null)

                return false;

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            // Send this token to the user via email 
            //sendgrid


            return true;
        }

        public async Task<bool> ResetPassword(ResetPasswordDTO resetPasswordDTO)
        {
            var user = await _userManager.FindByEmailAsync(resetPasswordDTO.Email);
            if (user == null)
            {
                return false;
            }
            var result = await _userManager.ResetPasswordAsync(user, resetPasswordDTO.Token, resetPasswordDTO.NewPassword);
            return result.Succeeded;
        }

    }
}
