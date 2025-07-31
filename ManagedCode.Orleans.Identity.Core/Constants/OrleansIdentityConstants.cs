namespace ManagedCode.Orleans.Identity.Core.Constants;

public static class OrleansIdentityConstants
{
    /// <summary>
    /// Key used to store user claims in Orleans RequestContext.
    /// Works with any ASP.NET Core authentication method (JWT, Cookie, etc.)
    /// </summary>
    public const string USER_CLAIMS = "MC-UserClaims";
}