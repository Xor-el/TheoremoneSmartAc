using System.Runtime.CompilerServices;
using SmartAc.Domain;

namespace SmartAc.Application.Abstractions.Repositories;

public interface IUnitOfWork
{
    ConfiguredTaskAwaitable<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    IRepository<TEntity> GetRepository<TEntity>() where TEntity : EntityBase;
}