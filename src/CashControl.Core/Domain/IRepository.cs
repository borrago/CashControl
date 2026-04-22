namespace CashControl.Core.Domain;

public interface IRepository
{
    public IUnitOfWork UnitOfWork { get; }
}