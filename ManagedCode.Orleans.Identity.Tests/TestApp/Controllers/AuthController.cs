using ManagedCode.Orleans.Identity.Tests.TestApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ManagedCode.Orleans.Identity.Tests.TestApp.Controllers;

[AllowAnonymous]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly SignInManager<TestUser> _signInManager;
    private readonly UserManager<TestUser> _userManager;

    public AuthController(SignInManager<TestUser> signInManager, UserManager<TestUser> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    [AllowAnonymous]
    [HttpGet("login")]
    public async Task<ActionResult> Login([FromQuery]string user)
    {
        var testUser = new TestUser(user);
        var identityResult = await _userManager.CreateAsync(testUser);
        await _signInManager.SignInAsync(testUser, true);
        return Ok();
    }
    
    [Authorize]
    [HttpGet("logout")]
    public async Task<ActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return Ok();
    }
}