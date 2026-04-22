using CashControl.Core.Domain;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace CashControl.Core.Infra;

public abstract class GenericRepository<TEntity>(EFCoreContextBase context) : IGenericRepository<TEntity> where TEntity : Entity
{
    private readonly EFCoreContextBase _context = context ?? throw new ArgumentNullException(nameof(context));

    public IUnitOfWork UnitOfWork => _context;

    public IQueryable<TEntity> Get(Expression<Func<TEntity, bool>>? predicate = null)
    {
        var query = _context
            .Set<TEntity>()
            .AsQueryable();

        if (predicate != null)
            query = query.Where(predicate);

        return query;
    }

    public IQueryable<TEntity> GetAsNoTracking(Expression<Func<TEntity, bool>>? predicate = null)
        => Get(predicate).AsNoTracking();

    public void Add(TEntity entity) => _context.Add(entity);

    public async Task AddAsync(TEntity entity, CancellationToken cancellationToken)
        => await _context.AddAsync(entity, cancellationToken);

    public void Update(TEntity entity) => _context.Entry(entity).State = EntityState.Modified;

    public void Remove(Guid id) => _context.Remove(id);

    public void Remove(TEntity entity) => _context.Remove(entity);
}