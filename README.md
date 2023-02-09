<img alt="managed code Identity" src="https://github.com/managed-code-hub/Identity/raw/main/logo.png" width="300px" />

# Orleans.Identity

[![.NET](https://github.com/managed-code-hub/Identity/actions/workflows/dotnet.yml/badge.svg)](https://github.com/managed-code-hub/Identity/actions/workflows/dotnet.yml)
[![nuget](https://github.com/managed-code-hub/Identity/actions/workflows/nuget.yml/badge.svg?branch=main)](https://github.com/managed-code-hub/Identity/actions/workflows/nuget.yml)
[![CodeQL](https://github.com/managed-code-hub/Identity/actions/workflows/codeql-analysis.yml/badge.svg?branch=main)](https://github.com/managed-code-hub/Identity/actions/workflows/codeql-analysis.yml)

| Version                                                                                                                                    | Package                                                                               | Description |
|--------------------------------------------------------------------------------------------------------------------------------------------|---------------------------------------------------------------------------------------|-------------|
| [![NuGet Package](https://img.shields.io/nuget/v/ManagedCode.Identity.Core.svg)](https://www.nuget.org/packages/ManagedCode.Identity.Core) | [ManagedCode.Identity.Core](https://www.nuget.org/packages/ManagedCode.Identity.Core) | Core        |

## Orleans.Identity

Orleans.Identity is a library for managing authorization and authentication in ASP.NET Identity applications using
Orleans.
It provides a simple, easy-to-use interface for managing user accounts and securing access to your application's
resources.

With Orleans.Identity, you can easily add support for user registration, login, logout, and password management to your
ASP.NET Identity applications. Additionally, Orleans.Identity provides support for managing user sessions, ensuring that
user data is kept secure and accessed only by authorized users.

## Features

- Easy integration with ASP.NET Identity applications
- Support for user registration, login, logout, and password management
- Secure session management
- Role-based access control
- Support for multi-tenancy
- Grain authorization filters like in ASP.NET
- Easy token management


## Motivation

The motivation for creating Orleans.Identity is to provide a scalable and performant solution for managing user sessions
and authentication in ASP.NET applications. Orleans provides a powerful actor model that makes it easy to implement
concurrency and scalability in distributed systems. By leveraging the power of Orleans, Orleans.Identity enables you to
easily add authentication and authorization to your ASP.NET applications without sacrificing performance.

## Main concepts

- ### Session storage
    Each session is represented as grain activation, session is stored as persistent grain state. 

- ### Session lifetime
    All sessions have their lifetime, which handled by reminder. When session grain activation become idle and ```OnDeactivateAsync``` grain method is called, then if grain has state, reminder will be registered. That reminder ticks after specified session lifetime and will close session.

- ### ASP.NET Authentication
  Authentication is based on session id that passed as auth token in request headers. We use custom ASP.NET ```AuthenticationHandler``` to verify session and receive its properties. Here some diagram how it works:

- ### Orleans Authorization
  Grains and their methods are authorized by using ```[Authorize], [AllowAnnonymous]``` attributes from ASP.NET, in grain call filter grain implementation will be scanned for these attributes. If grain has attributes it will validate session and continue grain call or reject it.

- ### Tokens
  Token is represented as grain activation and stored as grain state. Each token has their lifetime and reminder or timer will delete expired token. Reminder will be registered if token lifetime is more that 1 minute, either timer with 
## Getting Started

To use Orleans.Identity in your ASP.NET Identity application, follow these steps:

Install the ```ManagedCode.Orleans.Identity``` NuGet package in your project: