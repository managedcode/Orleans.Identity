using ManagedCode.Orleans.Identity.Core.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ManagedCode.Orleans.Identity.Tests.TestApp.Controllers;

[Authorize]
public class TestController : ControllerBase
{
    [HttpGet("authorize")]
    public ActionResult<string> Authorize()
    {
        User.SetOrleansContext();
        return "Authorize";
    }

    [AllowAnonymous]
    [HttpGet("anonymous")]
    public ActionResult<string> Anonymous()
    {
        return "Anonymous";
    }

    [Authorize(Roles = "admin")]
    [HttpGet("admin")]
    public ActionResult<string> Admin()
    {
        return "admin";
    }

    [Authorize(Roles = "moderator")]
    [HttpGet("moderator")]
    public ActionResult<string> Moderator()
    {
        return "moderator";
    }

    //[Authorize(Roles = "admin, moderator")]
    [HttpGet("common")]
    public ActionResult<string> Common()
    {
        return "common";
    }
}