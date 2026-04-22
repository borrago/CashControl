namespace CashControl.Core.CrossCutting;

public class FakeTenantProvider : ITenantProvider
{
    public Tenant Tenant { get; set; } = new Tenant();
}
