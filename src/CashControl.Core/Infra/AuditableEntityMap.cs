using CashControl.Core.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CashControl.Core.Infra;

public abstract class AuditableEntityMap<TEntity> : EntityMap<TEntity> where TEntity : AuditableEntity
{
    public override void Configure(EntityTypeBuilder<TEntity> builder)
    {
        builder
            .Property(x => x.DataCriacao)
            .HasColumnName("DataCriacao")
            .HasColumnType("datetime2")
            .IsRequired();

        builder
            .Property(x => x.IdUsuarioCriacao)
            .HasColumnName("IdUsuarioCriacao")
            .HasColumnType("uniqueidentifier")
            .ValueGeneratedNever();

        builder
            .Property(x => x.DataAlteracao)
            .HasColumnName("DataAlteracao")
            .HasColumnType("datetime2");

        builder
            .Property(x => x.IdUsuarioAlteracao)
            .HasColumnName("IdUsuarioAlteracao")
            .HasColumnType("uniqueidentifier")
            .ValueGeneratedNever();

        base.Configure(builder);
    }
}