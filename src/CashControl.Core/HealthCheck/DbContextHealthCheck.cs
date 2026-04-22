using CashControl.Core.CrossCutting;
using CashControl.Core.Infra;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace CashControl.Core.HealthCheck;

public class DbContextHealthCheck(ILoggedUserProvider loggedUserProvider,
    IEnvironment environment) : IHealthCheck
{
    private readonly ILoggedUserProvider _loggedUserProvider = loggedUserProvider ?? throw new ArgumentNullException(nameof(loggedUserProvider));
    private readonly IEnvironment _environment = environment ?? throw new ArgumentNullException(nameof(environment));

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var dbContext = new EFCoreContext(_environment, _loggedUserProvider);

            var databaseIsAvailable = await dbContext
                .Database
                .CanConnectAsync(cancellationToken);

            if (!databaseIsAvailable)
            {
                return new HealthCheckResult(context.Registration.FailureStatus);
            }

            return HealthCheckResult.Healthy();
        }
        catch (Exception)
        {
            return new HealthCheckResult(context.Registration.FailureStatus);
        }
    }
}