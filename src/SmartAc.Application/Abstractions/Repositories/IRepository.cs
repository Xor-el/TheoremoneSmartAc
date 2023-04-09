using SmartAc.Application.Specifications.Shared;
using SmartAc.Domain;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace SmartAc.Application.Abstractions.Repositories;

public interface IRepository<TEntity> where TEntity : EntityBase
{
    ConfiguredValueTaskAwaitable<TEntity?> FindByIdAsync(object id, CancellationToken cancellationToken = default);

    IQueryable<TEntity> GetQueryable(ISpecification<TEntity> specification);

    void Add(TEntity entity);

    void AddRange(IEnumerable<TEntity> entities);

    void Update(TEntity entity);

    void Remove(TEntity entity);

    void RemoveRange(IEnumerable<TEntity> entities);

    ConfiguredTaskAwaitable<bool> ContainsAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default);

    ConfiguredTaskAwaitable<bool> ContainsAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    ConfiguredTaskAwaitable<int> CountAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    ConfiguredTaskAwaitable<int> CountAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}