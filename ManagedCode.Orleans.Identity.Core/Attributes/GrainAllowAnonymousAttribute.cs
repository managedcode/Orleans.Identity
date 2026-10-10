using Microsoft.AspNetCore.Authorization;

namespace ManagedCode.Orleans.Identity.Core.Attributes;

/// <summary>
/// Allows anonymous grain calls by default, including on a grain interface.
/// Method-level authorization can require an authenticated caller for an exact operation.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Method,
    AllowMultiple = false, Inherited = true)]
public sealed class GrainAllowAnonymousAttribute : AllowAnonymousAttribute;
