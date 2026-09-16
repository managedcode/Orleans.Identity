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
                policy.RequireAssertion(context => context.Resource is IIncomingGrainCallContext call &&
                    call.Grain is Grains.StreamingAuthorizationGrain &&
                    call.InterfaceMethod.DeclaringType == typeof(Grains.IStreamingAuthorizationGrain) &&
                    call.ImplementationMethod.DeclaringType == typeof(Grains.StreamingAuthorizationGrain) &&
                    call.Request.GetArgument(0) is "allowed" &&
                    call.MethodName == nameof(Grains.IStreamingAuthorizationGrain.ResourceProtected)));
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

        // For test purpose - in-memory reminder service
        siloBuilder.UseInMemoryReminderService();
    }
}
