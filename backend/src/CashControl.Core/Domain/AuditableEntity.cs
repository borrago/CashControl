namespace CashControl.Core.Domain;

public abstract class AuditableEntity : Entity, IAuditableEntity
{
    protected AuditableEntity() : base() { }

    protected AuditableEntity(Guid id) : base(id) { }

    public DateTime DataCriacao { get; }

    public Guid? IdUsuarioCriacao { get; }

    public DateTime? DataAlteracao { get; }

    public Guid? IdUsuarioAlteracao { get; }
}
