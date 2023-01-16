using System;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using ManagedCode.Orleans.Identity.Constants;
using ManagedCode.Orleans.Identity.Extensions;
using ManagedCode.Orleans.Identity.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans;

namespace ManagedCode.Orleans.Identity.Client.Middlewares;

public class OrleansIdentityAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly IClusterClient _client;

    public OrleansIdentityAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock,
        IClusterClient client
    ) : base(options, logger, encoder, clock)
    {
        _client = client;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        string sessionId;
        if (!Request.Headers.TryGetValue(OrleansIdentityConstants.AUTH_TOKEN, out var values))
        {
            if (Request.Headers.TryGetValue("Authorization", out var jwt))
            {
                sessionId = jwt.ToString().Replace("Bearer", "").Trim();
            }
            else if (Request.Query.TryGetValue(OrleansIdentityConstants.AUTH_TOKEN, out var queryValues))
            {
                sessionId = queryValues.ToString().Trim();
            }
            else
            {
                return AuthenticateResult.NoResult();
            }
        }
        else
        {
            sessionId = values.ToString().Trim();
        }

        if (string.IsNullOrEmpty(sessionId))
        {
            return AuthenticateResult.NoResult();
        }

        try
        {
            var sessionGrain = _client.GetGrain<ISessionGrain>(sessionId);
            var result = await sessionGrain.ValidateAndGetClaimsAsync();

            if (result.IsSuccess)
            {
                ClaimsIdentity claimsIdentity = new(OrleansIdentityConstants.AUTHENTICATION_TYPE);

                foreach (var claim in result.Value)
                    claimsIdentity.ParseClaims(claim.Key, claim.Value);

                claimsIdentity.AddClaim(new Claim(OrleansIdentityConstants.SESSION_ID_CLAIM_NAME, sessionId));

                var ticket = new AuthenticationTicket(new ClaimsPrincipal(claimsIdentity), Scheme.Name);
                return AuthenticateResult.Success(ticket);
            }
        }
        catch (Exception e)
        {
            Logger.LogError(e, "HandleAuthenticateAsync.SessionId Validation");
        }

        return AuthenticateResult.Fail($"Unauthorized request. SessionId: {sessionId};");
    }
}