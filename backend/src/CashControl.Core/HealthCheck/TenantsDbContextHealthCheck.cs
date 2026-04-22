using CashControl.Core.CrossCutting;
using CashControl.Core.Infra;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace CashControl.Core.HealthCheck;

public class TenantsDbContextHealthCheck(ITenantSource tenantSource, ILoggedUserProvider loggedUserProvider,
    IEnvironment environment) : IHealthCheck
{
    private readonly ITenantSource _tenantSource = tenantSource ?? throw new ArgumentNullException(nameof(tenantSource));
    private readonly ILoggedUserProvider _loggedUserProvider = loggedUserProvider ?? throw new ArgumentNullException(nameof(loggedUserProvider));
    private readonly IEnvironment _environment = environment ?? throw new ArgumentNullException(nameof(environment));

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var tenants = _tenantSource.GetTenants();
            var tenantProvider = new FakeTenantProvider();

            foreach (var tenant in tenants)
            {
                tenantProvider.Tenant = tenant;

                var dbContext = new EFCoreContextMultiTenant(_environment, _loggedUserProvider, tenantProvider);

                var databaseIsAvailable = await dbContext
                    .Database
                    .CanConnectAsync(cancellationToken);

                if (!databaseIsAvailable)
                {
                    return new HealthCheckResult(context.Registration.FailureStatus);
                }
            }

            return HealthCheckResult.Healthy();
        }
        catch (Exception)
        {
            return new HealthCheckResult(context.Registration.FailureStatus);
        }
    }
}