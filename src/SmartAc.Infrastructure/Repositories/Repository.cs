using Microsoft.EntityFrameworkCore;
using SmartAc.Application.Abstractions.Repositories;
using SmartAc.Application.Specifications.Shared;
using SmartAc.Domain;
using System.Linq.Expressions;

namespace SmartAc.Infrastructure.Repositories;

public class Repository<TEntity> : IRepository<TEntity> where TEntity : EntityBase
{
    private readonly DbSet<TEntity> _table;

    public Repository(DbContext context) => _table = context.Set<TEntity>();

    public ValueTask<TEntity?> FindByIdAsync(object id, CancellationToken cancellationToken = default) 
        => _table.FindAsync(new[] { id }, cancellationToken: cancellationToken);

    public IQueryable<TEntity> Find(ISpecification<TEntity> specification) => ApplySpecification(specification);

    public void Add(TEntity entity) => _table.Add(entity);

    public void AddRange(IEnumerable<TEntity> entities) => _table.AddRange(entities);

    public void Update(TEntity entity) => _table.Update(entity);

    public void Remove(TEntity entity) => _table.Remove(entity);

    public void RemoveRange(IEnumerable<TEntity> entities) => _table.RemoveRange(entities);

    public async Task<bool> ContainsAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default)
        => await CountAsync(specification, cancellationToken).ConfigureAwait(false) > 0;

    public async Task<bool> ContainsAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        => await CountAsync(predicate, cancellationToken).ConfigureAwait(false) > 0;

    public Task<int> CountAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default)
        => ApplySpecification(specification).CountAsync(cancellationToken);

    public Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        => _table.Where(predicate).CountAsync(cancellationToken);

    private IQueryable<TEntity> ApplySpecification(ISpecification<TEntity> specification)
        => SpecificationEvaluator<TEntity>.GetQuery(_table.AsQueryable(), specification);
}