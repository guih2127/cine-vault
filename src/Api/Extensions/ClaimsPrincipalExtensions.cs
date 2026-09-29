using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace CineVault.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var subject = user.FindFirstValue(JwtRegisteredClaimNames.Sub);
        return Guid.Parse(subject!);
    }
}
