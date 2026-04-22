namespace CashControl.Core.Domain;

public interface IAuditableEntity : IEntity
{
    DateTime DataCriacao { get; }

    Guid? IdUsuarioCriacao { get; }

    DateTime? DataAlteracao { get; }

    Guid? IdUsuarioAlteracao { get; }
}