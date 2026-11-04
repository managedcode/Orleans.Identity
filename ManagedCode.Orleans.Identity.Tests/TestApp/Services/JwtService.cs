using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace ManagedCode.Orleans.Identity.Tests.TestApp.Services;

public class JwtService(TimeProvider timeProvider) : IJwtService
{
    private readonly string _secretKey = "your-super-secret-key-with-at-least-32-characters";
    private readonly string _issuer = "Orleans.Identity.Test";
    private readonly string _audience = "Orleans.Identity.Test";

    public string GenerateToken(string userId, string username, IEnumerable<string> roles)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Name, username),
            new(ClaimTypes.Actor, userId) // For grain ID
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: timeProvider.GetUtcNow().AddHours(1).UtcDateTime,
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    internal static LifetimeValidator CreateLifetimeValidator(TimeProvider timeProvider) =>
        (notBefore, expires, _, parameters) =>
        {
            if (!expires.HasValue && parameters.RequireExpirationTime)
            {
                throw new SecurityTokenNoExpirationException("The token has no expiration.");
            }

            if (notBefore > expires)
            {
                throw new SecurityTokenInvalidLifetimeException("The token expires before it becomes valid.")
                {
                    NotBefore = notBefore,
                    Expires = expires
                };
            }

            var now = timeProvider.GetUtcNow().UtcDateTime;
            if (notBefore is { } starts && starts - now > parameters.ClockSkew)
            {
                throw new SecurityTokenNotYetValidException("The token is not yet valid.") { NotBefore = starts };
            }

            if (expires is { } ends && now - ends > parameters.ClockSkew)
            {
                throw new SecurityTokenExpiredException("The token expired.") { Expires = ends };
            }

            return true;
        };

    public ClaimsPrincipal? ValidateToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_secretKey);

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _issuer,
                ValidateAudience = true,
                ValidAudience = _audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
                LifetimeValidator = CreateLifetimeValidator(timeProvider)
            };

            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out _);
            return principal;
        }
        catch
        {
            return null;
        }
    }
}