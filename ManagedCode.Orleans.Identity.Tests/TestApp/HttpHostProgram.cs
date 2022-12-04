using ManagedCode.Orleans.Identity.Middlewares;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace ManagedCode.Orleans.Identity.Tests.Cluster;

public class HttpHostProgram
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        builder.Services.AddControllers();
        builder.Services.AddSignalR();
        
        builder.Services.AddAuthenticationHandler(); /////////
        
        var app = builder.Build();

        //Authentication should always be placed before Authorization.
        app.UseAuthentication();
        app.UseAuthorization();
        
        app.UseOrleansAuthorization();/////////

        app.MapControllers();
        app.MapHub<TestAnonymousHub>(nameof(TestAnonymousHub));
        app.MapHub<TestAuthorizeHub>(nameof(TestAuthorizeHub));

        app.Run();
    }
}