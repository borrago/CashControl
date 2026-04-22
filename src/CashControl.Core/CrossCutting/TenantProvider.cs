namespace CashControl.Core.CrossCutting;

public class TenantProvider : ITenantProvider
{
    private const int TenantDefaultKey = 1;

    public TenantProvider(ILoggedUserProvider loggedUserProvider, ITenantSource tenantSource, IEnvironment environment)
    {
        _ = loggedUserProvider ?? throw new ArgumentNullException(nameof(loggedUserProvider));
        _ = tenantSource ?? throw new ArgumentNullException(nameof(tenantSource));
        _ = environment ?? throw new ArgumentNullException(nameof(environment));

        var tenants = tenantSource.GetTenants();
        var tenantId = loggedUserProvider.Tenant;

        if (tenantId == 0)
        {
            if (!environment.IsDevelopment())
            {
                throw new Exception($"Missing tenant configuration in environment: {environment.Name}");
            }

            tenantId = TenantDefaultKey;
        }

        var tenant = tenants.FirstOrDefault(x => x.Id == tenantId);

        if (tenant is null)
        {
            throw new Exception($"Tenant not found with Id: {tenantId}");
        }

        Tenant = tenant;
    }

    public Tenant Tenant { get; }
}
