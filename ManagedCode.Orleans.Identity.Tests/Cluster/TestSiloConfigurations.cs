using ManagedCode.Orleans.Identity.Server.Extensions;
using ManagedCode.Orleans.Identity.Tests.Constants;
using Orleans.TestingHost;

namespace ManagedCode.Orleans.Identity.Tests.Cluster;

public class TestSiloConfigurations : ISiloConfigurator
{
    public void Configure(ISiloBuilder siloBuilder)
    {
        siloBuilder.AddOrleansIdentity(options =>
        {
            options.AddPolicy(Grains.StreamingAuthorizationGrain.ResourcePolicy, policy =>
                policy.RequireAssertion(async context =>
                {
                    if (context.Resource is not IIncomingGrainCallContext call ||
                        call.Grain is not Grains.StreamingAuthorizationGrain ||
                        call.InterfaceMethod.DeclaringType != typeof(Grains.IStreamingAuthorizationGrain) ||
                        call.ImplementationMethod.DeclaringType != typeof(Grains.StreamingAuthorizationGrain) ||
                        call.MethodName != nameof(Grains.IStreamingAuthorizationGrain.ResourceProtected))
                    {
                        return false;
                    }

                    var argument = call.Request.GetArgument(0);
                    if (argument is "mutate")
                    {
                        await Task.Yield();
                        context.User.Claims.First().Properties["tenant"] = "changed-by-policy";
                        return true;
                    }

                    return argument is "allowed";
                }));
            options.AddPolicy(TestAuthorizationPolicies.RequireScheme, policy =>
                policy.AddAuthenticationSchemes(TestAuthorizationPolicies.UnsupportedAuthenticationScheme).RequireAuthenticatedUser());
            options.AddPolicy(
                TestAuthorizationPolicies.RequireAdminDepartment,
                policy =>
                {
                    policy.RequireClaim(
                        TestAuthorizationPolicies.DepartmentClaimType,
                        TestAuthorizationPolicies.AdminDepartment
                    );
                }
            );
        });

        siloBuilder.AddOrleansIdentity();

        // For test purpose - in-memory reminder service
        siloBuilder.UseInMemoryReminderService();
    }
}
