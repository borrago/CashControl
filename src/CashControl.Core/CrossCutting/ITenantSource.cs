namespace CashControl.Core.CrossCutting;

public interface ITenantSource
{
    IEnumerable<Tenant> GetTenants();
}
