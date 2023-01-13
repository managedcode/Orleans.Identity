using ManagedCode.Orleans.Identity.Extensions;
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

        // AddProperty it for using Orleans Identity
        builder.Services.AddOrleansIdentity(); 

        var app = builder.Build();

        // AddProperty it for using Orleans Identity
        // Authentication and Authorization already use
        app.UseAuthenticationAndOrleansIdentity(); 

        app.MapControllers();
        app.MapHub<TestAnonymousHub>(nameof(TestAnonymousHub));
        app.MapHub<TestAuthorizeHub>(nameof(TestAuthorizeHub));

        app.Run();
    }
}