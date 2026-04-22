using CashControl.Core.CrossCutting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace CashControl.Core.Infra;

public class EFCoreContext(IEnvironment environment, ILoggedUserProvider loggedUserProvider) : EFCoreContextBase(environment, loggedUserProvider)
{
    private static readonly string connStringKey = "ConnectionStrings:DefaultConnection";
    private readonly IEnvironment _environment = environment ?? throw new ArgumentNullException(nameof(environment));

    public static string ConnStringKey { get => connStringKey; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var connectionString = _environment[ConnStringKey];

        optionsBuilder.UseSqlServer(connectionString, options => options.EnableRetryOnFailure());
    }

    protected override void ApplyChangesForAddedEntity(EntityEntry entry)
    {
        FillAddedAuditableProperties(entry);
    }

    protected override void ApplyChangesForModifiedEntity(EntityEntry entry)
    {
        FillModifiedAuditableProperties(entry);
    }
}