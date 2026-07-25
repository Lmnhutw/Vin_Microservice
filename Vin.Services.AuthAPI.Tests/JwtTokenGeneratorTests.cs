using System.Linq;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Options;
using Vin.Services.AuthAPI.Models;
using Vin.Services.AuthAPI.Services;
using Xunit;

namespace Vin.Services.AuthAPI.Tests;

public sealed class JwtTokenGeneratorTests
{
    [Fact]
    public void GenerateToken_contains_identity_and_roles()
    {
        var generator = new JwtTokenGenerator(Options.Create(new JwtOptions
        {
            Secret = "test-secret-with-enough-length-for-signing",
            Issuer = "test-issuer",
            Audience = "test-audience"
        }));

        var token = new JwtSecurityTokenHandler().ReadJwtToken(generator.GenerateToken(
            new ApplicationUser
            {
                Id = "user-1",
                UserName = "user@example.com",
                Email = "user@example.com",
                FullName = "Test User"
            },
            ["CUSTOMER"]));

        Assert.Equal("user-1", token.Subject);
        Assert.Contains(token.Claims, claim => claim.Type == "role" && claim.Value == "CUSTOMER");
        Assert.Equal("test-issuer", token.Issuer);
        Assert.Equal("test-audience", token.Audiences.Single());
    }
}
