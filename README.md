<img alt="managed code Identity" src="https://github.com/managed-code-hub/Identity/raw/main/logo.png" width="300px" />

# Orleans.Identity

A simplified Orleans library for handling authorization context propagation from ASP.NET Core controllers and SignalR hubs to Orleans grains.

## Overview

This library provides a simple way to pass user authorization context from your ASP.NET Core application to Orleans grains, allowing you to implement authorization at the grain level using standard ASP.NET Core authorization attributes.

## Features

- **JWT-based authentication**: Works with standard JWT tokens
- **Controller authorization**: Automatically passes user claims to grains called from controllers
- **SignalR authorization**: Supports authorization in SignalR hubs
- **Grain-level authorization**: Use `[Authorize]` and `[Authorize(Roles = "RoleName")]` attributes on grains
- **Simple grain extension**: Use `this.GetCurrentUser().Claims` to access user claims in grains

## Quick Start

### 1. Orleans Cluster Setup

```csharp
var builder = Host.CreateDefaultBuilder(args)
    .UseOrleans(siloBuilder =>
    {
        siloBuilder
            .UseLocalhostClustering()
            .AddOrleansIdentity() // Add the authorization filter
            .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(YourGrain).Assembly));
    });
```

### 2. ASP.NET Core API Setup

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add Orleans client
builder.Services.AddOrleansClient(client =>
{
    client.UseLocalhostClustering();
});

// Add Orleans Identity
builder.Services.AddOrleansIdentity();

// Add JWT authentication
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options => { /* JWT configuration */ });

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
app.UseOrleansIdentity(); // Add the middleware

app.MapControllers();
app.MapHub<YourHub>("/hub");
```

### 3. Using in Grains

```csharp
[Authorize]
public class MyGrain : Grain, IMyGrain
{
    [AllowAnonymous]
    public Task<string> GetPublicInfo()
    {
        return Task.FromResult("Public info");
    }

    [Authorize]
    public Task<string> GetUserInfo()
    {
        var user = this.GetCurrentUser();
        var username = user.FindFirst(ClaimTypes.Name)?.Value;
        return Task.FromResult($"Hello, {username}!");
    }

    [Authorize(Roles = "Admin")]
    public Task<string> GetAdminInfo()
    {
        return Task.FromResult("Admin only info");
    }
}
```

## Testing

The library includes comprehensive integration tests in the `ManagedCode.Orleans.Identity.Tests` project that demonstrate:

- JWT token generation and validation
- Controller → Grain authorization flow
- SignalR → Grain authorization flow
- Role-based access control
- Grain authorization with user claims

### Running Tests

```bash
dotnet test
```

### Test Structure

The tests use the existing integration test infrastructure with:
- **TestApp**: ASP.NET Core application with controllers and SignalR hubs
- **Cluster**: Orleans test cluster with grains
- **Integration Tests**: Comprehensive tests covering all scenarios

## Architecture

The library works by:

1. **Middleware**: Extracts user claims from JWT tokens and stores them in Orleans `RequestContext`
2. **SignalR Filter**: Handles authorization in SignalR hubs and stores claims in `RequestContext`
3. **Grain Filter**: Intercepts grain calls and validates authorization based on `[Authorize]` attributes
4. **Grain Extension**: Provides `this.GetCurrentUser()` method to access claims in grains

## License

MIT License

