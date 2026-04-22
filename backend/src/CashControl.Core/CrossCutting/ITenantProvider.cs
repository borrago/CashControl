namespace CashControl.Core.CrossCutting;

public interface ITenantProvider
{
    Tenant Tenant { get; }
}
