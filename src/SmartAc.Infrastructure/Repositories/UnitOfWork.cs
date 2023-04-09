using System.Runtime.CompilerServices;
using SmartAc.Application.Abstractions.Repositories;
using Microsoft.Extensions.DependencyInjection;
using SmartAc.Domain;

namespace SmartAc.Infrastructure.Repositories;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly IServiceProvider _provider;

    public UnitOfWork(IServiceProvider provider) => _provider = provider;

    public ConfiguredTaskAwaitable<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _provider.GetRequiredService<SmartAcContext>().SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public IRepository<TEntity> GetRepository<TEntity>() where TEntity : EntityBase
    {
        var repositoryType = typeof(Repository<>);
        var context = _provider.GetRequiredService<SmartAcContext>();
        var instance = Activator.CreateInstance(repositoryType.MakeGenericType(typeof(TEntity)), context);
        return (IRepository<TEntity>) instance!;
    }
}
