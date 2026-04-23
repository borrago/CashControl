using CashControl.Identity.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CashControl.Identity.Infra;

public class Context(DbContextOptions<Context> options) : IdentityDbContext<User>(options)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<User>(entity =>
        {
            entity.Property(x => x.FullName).HasMaxLength(200);
            entity.Property(x => x.Tenant).IsRequired();
            entity.Property(x => x.IsSuperUser).IsRequired();
            entity.Property(x => x.RefreshToken).HasMaxLength(500);
            entity.HasIndex(x => x.Tenant);
        });
    }
}
