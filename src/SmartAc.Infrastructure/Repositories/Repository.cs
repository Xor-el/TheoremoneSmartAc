using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using SmartAc.Application.Abstractions.Repositories;
using SmartAc.Application.Specifications.Shared;
using SmartAc.Domain;

namespace SmartAc.Infrastructure.Repositories;

public class Repository<TEntity> : IRepository<TEntity> where TEntity : EntityBase
{
    private readonly DbSet<TEntity> _table;

    public Repository(DbContext context) => _table = context.Set<TEntity>();

    public ConfiguredValueTaskAwaitable<TEntity?> FindByIdAsync(object id, CancellationToken cancellationToken = default)
        => _table.FindAsync(new[] { id }, cancellationToken).ConfigureAwait(false);

    public IQueryable<TEntity> Find(ISpecification<TEntity> specification) => ApplySpecification(specification);

    public void Add(TEntity entity) => _table.Add(entity);

    public void AddRange(IEnumerable<TEntity> entities) => _table.AddRange(entities);

    public void Update(TEntity entity) => _table.Update(entity);

    public void Remove(TEntity entity) => _table.Remove(entity);

    public void RemoveRange(IEnumerable<TEntity> entities) => _table.RemoveRange(entities);

    public ConfiguredTaskAwaitable<bool> ContainsAsync(
        ISpecification<TEntity> specification,
        CancellationToken cancellationToken = default)
    {
        return ApplySpecification(specification).AnyAsync(cancellationToken).ConfigureAwait(false);
    }

    public ConfiguredTaskAwaitable<bool> ContainsAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return _table.Where(predicate).AnyAsync(cancellationToken).ConfigureAwait(false);
    }

    public ConfiguredTaskAwaitable<int> CountAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return _table.Where(predicate).CountAsync(cancellationToken).ConfigureAwait(false);
    }

    public ConfiguredTaskAwaitable<int> CountAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default)
    {
        return ApplySpecification(specification).CountAsync(cancellationToken).ConfigureAwait(false);
    }

    private IQueryable<TEntity> ApplySpecification(ISpecification<TEntity> specification)
        => SpecificationEvaluator<TEntity>.GetQuery(_table.AsQueryable(), specification);
}