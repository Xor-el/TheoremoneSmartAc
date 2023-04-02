using SmartAc.Application.Specifications.Shared;
using SmartAc.Domain;
using System.Linq.Expressions;

namespace SmartAc.Application.Abstractions.Repositories;

public interface IRepository<TEntity> where TEntity : EntityBase
{
    ValueTask<TEntity?> FindByIdAsync(object id, CancellationToken cancellationToken = default);

    IQueryable<TEntity> Find(ISpecification<TEntity> specification);

    void Add(TEntity entity);

    void AddRange(IEnumerable<TEntity> entities);

    void Update(TEntity entity);

    void Remove(TEntity entity);

    void RemoveRange(IEnumerable<TEntity> entities);

    Task<bool> ContainsAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default);

    Task<bool> ContainsAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    Task<int> CountAsync(ISpecification<TEntity> specification, CancellationToken cancellation = default);

    Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellation = default);
}