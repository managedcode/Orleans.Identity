using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ManagedCode.Orleans.Identity.Tests.Cluster;

[Authorize(Roles = "admin")]
[Route("adminController")]
public class AdminController : ControllerBase
{
    [HttpGet]
    public ActionResult<string> AdminsOnly()
    {
        return "Admins only";
    }

    [HttpGet("adminsList")]
    [AllowAnonymous]
    public ActionResult<string> AdminsList()
    {
        return "adminsList";
    }
}