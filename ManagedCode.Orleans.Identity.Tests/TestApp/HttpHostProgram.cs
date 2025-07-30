using ManagedCode.Orleans.Identity.Client.Extensions;
using ManagedCode.Orleans.Identity.Tests.TestApp.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.Text;

namespace ManagedCode.Orleans.Identity.Tests.TestApp;

public class HttpHostProgram
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers();
        builder.Services.AddSignalR();

        // Add Orleans Identity
        builder.Services.AddOrleansIdentity();

        // Add JWT Authentication
        builder.Services.AddAuthentication("Bearer")
            .AddJwtBearer("Bearer", options =>
            {
                options.TokenValidationParameters = new()
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = "Orleans.Identity.Test",
                    ValidAudience = "Orleans.Identity.Test",
                    IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes("your-super-secret-key-with-at-least-32-characters"))
                };

                options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/TestAuthorizeHub"))
                        {
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    }
                };
            });

        builder.Services.AddAuthorization();

        // Add JWT service
        builder.Services.AddScoped<IJwtService, JwtService>();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        app.UseAuthentication();
        app.UseAuthorization();

        // Add Orleans Identity middleware
        app.UseOrleansIdentity();

        app.MapControllers();
        app.MapHub<TestAnonymousHub>(nameof(TestAnonymousHub));
        app.MapHub<TestAuthorizeHub>(nameof(TestAuthorizeHub));

        app.Run();
    }
}