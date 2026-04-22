namespace CashControl.Core.Domain;

public abstract class AuditableTenantEntity : AuditableEntity, ITenantEntity
{
    protected AuditableTenantEntity() : base()
    {
    }

    protected AuditableTenantEntity(Guid id) : base(id)
    {
    }

    public int Tenant { get; set; }
}
