using CashControl.Core.CrossCutting;
using CashControl.Core.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;

namespace CashControl.Core.Infra;

public abstract class EFCoreContextBase(IEnvironment environment, ILoggedUserProvider loggedUserProvider) : DbContext, IUnitOfWork
{
    private static readonly ILoggerFactory LoggerFactory = Microsoft.Extensions.Logging.LoggerFactory.Create(
        builder =>
        {
            builder
                .AddFilter((category, level) =>
                    category == DbLoggerCategory.Database.Command.Name
                    && level == LogLevel.Information);
            builder.AddConsole();
        });

    private readonly IEnvironment _environment = environment ?? throw new ArgumentNullException(nameof(environment));
    private readonly ILoggedUserProvider _loggedUserProvider = loggedUserProvider ?? throw new ArgumentNullException(nameof(loggedUserProvider));

    public async Task<bool> CommitAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.Entity is IValidator)
            {
                var domain = entry.Entity as IValidator;
                domain?.Validate();
            }

            switch (entry.State)
            {
                case EntityState.Added:
                    ApplyChangesForAddedEntity(entry);
                    break;
                case EntityState.Modified:
                    ApplyChangesForModifiedEntity(entry);
                    break;
            }
        }

        this.EnsureAutoHistory(() => new __EFAutoHistory() // Salva somente alteração e Exclusão
        {
            LoggedUserId = _loggedUserProvider.IsLogged() ? _loggedUserProvider.IdUsuario : null
        });

        return await SaveChangesAsync(cancellationToken) > 0;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.EnableAutoHistory<__EFAutoHistory>(o => { });

        base.OnModelCreating(modelBuilder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (_environment.IsDevelopment())
            optionsBuilder.UseLoggerFactory(LoggerFactory);
    }

    protected abstract void ApplyChangesForAddedEntity(EntityEntry entry);
    protected abstract void ApplyChangesForModifiedEntity(EntityEntry entry);

    protected void FillAddedAuditableProperties(EntityEntry entry)
    {
        if (entry.Entity.GetType().GetProperty("DataCriacao") != null)
            entry.Property("DataCriacao").CurrentValue = DateTime.UtcNow;

        if (entry.Entity.GetType().GetProperty("IdUsuarioCriacao") != null && _loggedUserProvider.IsLogged())
            entry.Property("IdUsuarioCriacao").CurrentValue = _loggedUserProvider.IdUsuario;
    }

    protected void FillModifiedAuditableProperties(EntityEntry entry)
    {
        if (entry.Entity.GetType().GetProperty("DataAlteracao") != null)
            entry.Property("DataAlteracao").CurrentValue = DateTime.UtcNow;

        if (entry.Entity.GetType().GetProperty("IdUsuarioAlteracao") != null && _loggedUserProvider.IsLogged())
            entry.Property("IdUsuarioAlteracao").CurrentValue = _loggedUserProvider.IdUsuario;
    }
}
