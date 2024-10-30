using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Vin.Services.AuthAPI.Models;
using Vin.Services.AuthAPI.Services.IServices;

namespace Vin.Services.AuthAPI.Services
{
    public class JwtTokenGenerator : IJwtTokenGenerator
    {
        private readonly JwtOptions _jwtOptions;

        public JwtTokenGenerator(JwtOptions jwtOptions)
        {
            _jwtOptions = jwtOptions;
        }

        public string GenerateToken(ApplicationUser applicationUser)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtOptions.Secret);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Email, applicationUser.Email),
                new Claim(JwtRegisteredClaimNames.Sub, applicationUser.Id),
                new Claim(JwtRegisteredClaimNames.Name, applicationUser.FullName)
            };
        }
    }
}