using CashControl.Core.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CashControl.Core.Infra;

public abstract class AuditableTenantEntityMap<TEntity> : AuditableEntityMap<TEntity> where TEntity : AuditableTenantEntity
{
    public override void Configure(EntityTypeBuilder<TEntity> builder)
    {
        builder
            .Property(x => x.Tenant)
            .HasColumnType("int");

        base.Configure(builder);
    }
}
