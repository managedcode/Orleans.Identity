using ManagedCode.Orleans.Identity.Middlewares;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Orleans.Runtime;

namespace ManagedCode.Orleans.Identity.Tests.Cluster;

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
    
}