using ManagedCode.Orleans.Identity.Client;
using ManagedCode.Orleans.Identity.Client.Extensions;
using ManagedCode.Orleans.Identity.Core.Constants;
using ManagedCode.Orleans.Identity.Tests.TestApp.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ManagedCode.Orleans.Identity.Tests.TestApp;

public class HttpHostProgram
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers();
        builder.Services.AddSignalR();

        
        // Add services to the container.
        builder.Services.AddDbContext<TestUserIdentityDbContext>(options =>
            options.UseInMemoryDatabase("InMemoryDbForTesting"));

        builder.Services.AddIdentity<TestUser, IdentityRole>()
            .AddEntityFrameworkStores<TestUserIdentityDbContext>()
            .AddDefaultTokenProviders();
            
        // AddProperty it for using Orleans Identity
        builder.Services.AddOrleansIdentity();
        

        
        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy(OrleansIdentityConstants.AUTHENTICATION_TYPE, policy =>
            {
                policy.AuthenticationSchemes.Add(OrleansIdentityConstants.AUTH_TOKEN);
                policy.Requirements.Add(new SessionRequirement());
            });
        });
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultScheme = OrleansIdentityConstants.AUTHENTICATION_TYPE;
        });


        var app = builder.Build();

        // AddProperty it for using Orleans Identity
        // Authentication and Authorization already use
        //app.UseAuthenticationAndOrleansIdentity();

        app.UseAuthentication();
        app.UseAuthorization();
        
        app.MapControllers();
        app.MapHub<TestAnonymousHub>(nameof(TestAnonymousHub));
        app.MapHub<TestAuthorizeHub>(nameof(TestAuthorizeHub));

        app.Run();
    }
}