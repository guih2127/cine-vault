using System.IdentityModel.Tokens.Jwt;
using System.Text;
using CineVault.Domain.Users;
using CineVault.Infrastructure.Security;
using FluentAssertions;
using Microsoft.IdentityModel.Tokens;

namespace CineVault.Infrastructure.Tests.Security;

public class JwtTokenServiceTests
{
    private readonly JwtOptions _options = new()
    {
        Key = "super-secret-signing-key-that-is-long-enough",
        Issuer = "cinevault",
        Audience = "cinevault-clients",
        ExpiryMinutes = 60
    };

    [Fact]
    public void Generate_ProducesTokenWithUserClaims()
    {
        var service = new JwtTokenService(_options);
        var user = User.Create("Jane Doe", "jane@email.com", "hash");

        var token = service.Generate(user);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        jwt.Subject.Should().Be(user.Id.ToString());
        jwt.Claims.Should().Contain(claim => claim.Type == JwtRegisteredClaimNames.Email && claim.Value == "jane@email.com");
        jwt.Claims.Should().Contain(claim => claim.Type == JwtRegisteredClaimNames.Name && claim.Value == "Jane Doe");
    }

    [Fact]
    public void Generate_TokenValidatesAgainstConfiguredKey()
    {
        var service = new JwtTokenService(_options);
        var token = service.Generate(User.Create("Jane Doe", "jane@email.com", "hash"));

        var parameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = _options.Issuer,
            ValidateAudience = true,
            ValidAudience = _options.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key)),
            ValidateLifetime = true
        };

        var act = () => new JwtSecurityTokenHandler().ValidateToken(token, parameters, out _);

        act.Should().NotThrow();
    }
}
