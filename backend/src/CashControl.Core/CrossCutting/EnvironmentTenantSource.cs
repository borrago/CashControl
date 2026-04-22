namespace CashControl.Core.CrossCutting;

public class EnvironmentTenantSource(IEnvironment environment) : ITenantSource
{
    private readonly IEnvironment _environment = environment ?? throw new ArgumentNullException(nameof(environment));

    public IEnumerable<Tenant> GetTenants()
    {
        const int TenantKeyPosition = 1;

        var tenants = new List<Tenant>();

        var parameters = _environment.Parameters?
            .Where(x => x.Key.Contains("TenantsConnectionStrings") && x.Value != null);

        if (parameters == default)
            throw new ArgumentNullException(nameof(parameters));

        foreach (var parameter in parameters)
        {
            var splittedKey = parameter.Key.Split(':');
            var tenantKey = int.Parse(splittedKey[TenantKeyPosition]);

            tenants.Add(new Tenant()
            {
                Id = tenantKey,
                ConnectionString = parameter.Value ?? "",
            });
        }

        return tenants;
    }
}
