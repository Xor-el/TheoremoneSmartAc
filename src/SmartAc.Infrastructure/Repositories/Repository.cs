using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using SmartAc.Application.Abstractions.Repositories;
using SmartAc.Application.Specifications.Shared;
using SmartAc.Domain;

namespace SmartAc.Infrastructure.Repositories;

public class Repository<TEntity> : IRepository<TEntity> where TEntity : EntityBase
{
    private readonly SmartAcContext _context;

    public Repository(SmartAcContext context) => _context = context;

    public ConfiguredValueTaskAwaitable<TEntity?> FindByIdAsync(object id, CancellationToken cancellationToken = default)
        => _context.Set<TEntity>().FindAsync(new[] { id }, cancellationToken).ConfigureAwait(false);

    public IQueryable<TEntity> GetQueryable(ISpecification<TEntity> specification) => ApplySpecification(specification);

    public void Add(TEntity entity) => _context.Set<TEntity>().Add(entity);

    public void AddRange(IEnumerable<TEntity> entities) => _context.Set<TEntity>().AddRange(entities);

    public void Update(TEntity entity) => _context.Set<TEntity>().Update(entity);

    public void Remove(TEntity entity) => _context.Set<TEntity>().Remove(entity);

    public void RemoveRange(IEnumerable<TEntity> entities) => _context.Set<TEntity>().RemoveRange(entities);

    public ConfiguredTaskAwaitable<bool> ContainsAsync(
        ISpecification<TEntity> specification,
        CancellationToken cancellationToken = default)
    {
        return ApplySpecification(specification).AnyAsync(cancellationToken).ConfigureAwait(false);
    }

    public ConfiguredTaskAwaitable<bool> ContainsAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return _context.Set<TEntity>().Where(predicate).AnyAsync(cancellationToken).ConfigureAwait(false);
    }

    public ConfiguredTaskAwaitable<int> CountAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return _context.Set<TEntity>().Where(predicate).CountAsync(cancellationToken).ConfigureAwait(false);
    }

    public ConfiguredTaskAwaitable<int> CountAsync(
        ISpecification<TEntity> specification,
        CancellationToken cancellationToken = default)
    {
        return ApplySpecification(specification).CountAsync(cancellationToken).ConfigureAwait(false);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _context.SaveChangesAsync(cancellationToken);

    private IQueryable<TEntity> ApplySpecification(ISpecification<TEntity> specification)
        => SpecificationEvaluator<TEntity>.GetQuery(_context.Set<TEntity>().AsQueryable(), specification);
}