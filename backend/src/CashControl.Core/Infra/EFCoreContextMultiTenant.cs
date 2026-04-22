using CashControl.Core.CrossCutting;
using CashControl.Core.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Reflection;

namespace CashControl.Core.Infra;

public class EFCoreContextMultiTenant : EFCoreContextBase
{
    private readonly IEnvironment _environment;
    private readonly Tenant _tenant;

    private readonly MethodInfo ConfigureQueryFilterMethodInfo = typeof(EFCoreContextMultiTenant)
        .GetMethod(nameof(ConfigureQueryFilter), BindingFlags.Instance | BindingFlags.NonPublic) ?? throw new DomainException("Falha ao configurar ConfigureQueryFilterMethodInfo.");

    public EFCoreContextMultiTenant(IEnvironment environment, ILoggedUserProvider loggedUserProvider, ITenantProvider tenantProvider)
        : base(environment, loggedUserProvider)
    {
        _environment = environment ?? throw new ArgumentNullException(nameof(environment));

        ArgumentNullException.ThrowIfNull(tenantProvider);

        _tenant = tenantProvider.Tenant;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            ConfigureQueryFilterMethodInfo
                .MakeGenericMethod(entityType.ClrType)
                .Invoke(this, [modelBuilder, entityType]);
        }
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(_tenant.ConnectionString, options => options.EnableRetryOnFailure());
    }

    protected void ConfigureQueryFilter<TEntity>(ModelBuilder modelBuilder, IMutableEntityType mutableEntityType) where TEntity : class
    {
        if (mutableEntityType.BaseType != null)
        {
            return;
        }

        if (!typeof(ITenantEntity).IsAssignableFrom(typeof(TEntity)))
        {
            return;
        }

        modelBuilder.Entity<TEntity>().HasQueryFilter(e => ((ITenantEntity)e).Tenant == _tenant.Id);
    }

    protected override void ApplyChangesForAddedEntity(EntityEntry entry)
    {
        FillAddedAuditableProperties(entry);
        CheckAndSetTenantIdProperty(entry);
    }

    protected override void ApplyChangesForModifiedEntity(EntityEntry entry)
    {
        FillModifiedAuditableProperties(entry);
    }

    private void CheckAndSetTenantIdProperty(EntityEntry entry)
    {
        var entity = entry.Entity as ITenantEntity;

        if (entity is null)
        {
            return;
        }

        entity.Tenant = _tenant.Id;
    }
}
