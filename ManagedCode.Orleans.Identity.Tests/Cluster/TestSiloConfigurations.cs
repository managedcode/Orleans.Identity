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
