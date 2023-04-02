using SmartAc.Domain;

namespace SmartAc.Application.Abstractions.Repositories;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    IRepository<TEntity> GetRepository<TEntity>() where TEntity : EntityBase;
}