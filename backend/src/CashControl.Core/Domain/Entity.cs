using CashControl.Core.CrossCutting;

namespace CashControl.Core.Domain;

public abstract class Entity : AbstractValidator, IEntity
{
    protected Entity() => Id = Guid.NewGuid();

    protected Entity(Guid id) => Id = id;

    protected override IValidator GetInstance() => this;

    protected override CustomException GetCustomException() => new DomainException();

    public Guid Id { get; }

    public override bool Equals(object? obj)
    {
        var compareTo = obj as IEntity;
        if (ReferenceEquals(this, compareTo))
            return true;
        return !(compareTo is null) && Id.Equals(compareTo.Id);
    }

    public override int GetHashCode()
    {
        return GetType().GetHashCode() * 713 + Id.GetHashCode();
    }

    public override string ToString()
    {
        return GetType().Name + " [Id=" + Id + "]";
    }
}