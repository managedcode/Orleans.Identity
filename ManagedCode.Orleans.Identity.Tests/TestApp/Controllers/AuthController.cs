using System.Security.Claims;
using ManagedCode.Orleans.Identity.Tests.Constants;
using ManagedCode.Orleans.Identity.Tests.TestApp.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ManagedCode.Orleans.Identity.Tests.TestApp.Controllers;

[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly IJwtService _jwtService;

    public AuthController(IJwtService jwtService)
    {
        _jwtService = jwtService;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        // Simple authentication - in real app you'd validate against database
        if (string.IsNullOrEmpty(request.Username))
        {
            return BadRequest("Username is required");
        }

        var roles = request.Username.ToLower() switch
        {
            "admin" => new[] { TestRoles.USER, TestRoles.ADMIN },
            "moderator" => new[] { TestRoles.USER, TestRoles.MODERATOR },
            "user" => new[] { TestRoles.USER },
            _ => new[] { TestRoles.USER }
        };

        var token = _jwtService.GenerateToken(request.Username, request.Username, roles);

        return Ok(new { token });
    }

    [AllowAnonymous]
    [HttpPost("login-cookie")]
    public async Task<IActionResult> LoginCookie([FromBody] LoginRequest request)
    {
        // Simple authentication - in real app you'd validate against database
        if (string.IsNullOrEmpty(request.Username))
        {
            return BadRequest("Username is required");
        }

        var roles = request.Username.ToLower() switch
        {
            "admin" => new[] { TestRoles.USER, TestRoles.ADMIN },
            "moderator" => new[] { TestRoles.USER, TestRoles.MODERATOR },
            "user" => new[] { TestRoles.USER },
            _ => new[] { TestRoles.USER }
        };

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, request.Username),
            new Claim(ClaimTypes.NameIdentifier, request.Username),
            new Claim(ClaimTypes.Actor, request.Username)
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(60)
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties);

        return Ok(new { message = "Logged in successfully with cookie" });
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Ok(new { message = "Logged out successfully" });
    }

    [HttpGet("me")]
    [Authorize]
    public IActionResult GetCurrentUser()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var username = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
        var roles = User.FindAll(System.Security.Claims.ClaimTypes.Role).Select(c => c.Value);

        return Ok(new
        {
            UserId = userId,
            Username = username,
            Roles = roles
        });
    }
}

public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
}