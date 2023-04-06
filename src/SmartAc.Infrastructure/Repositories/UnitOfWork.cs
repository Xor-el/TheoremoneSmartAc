using SmartAc.Application.Abstractions.Repositories;
using System.Collections;
using System.Runtime.CompilerServices;
using SmartAc.Domain;

namespace SmartAc.Infrastructure.Repositories;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly SmartAcContext _context;
    private readonly Hashtable _repositories;

    public UnitOfWork(SmartAcContext context)
    {
        _context = context;
        _repositories = new Hashtable();
    }

    public ConfiguredTaskAwaitable<int> SaveChangesAsync(CancellationToken cancellationToken = default) 
        => _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

    public IRepository<TEntity> GetRepository<TEntity>() where TEntity : EntityBase
    {
        var type = typeof(TEntity).Name;

        if (_repositories.ContainsKey(type))
        {
            return (IRepository<TEntity>) _repositories[type]!;
        }

        var repositoryType = typeof(Repository<>);

        var instance = Activator.CreateInstance(repositoryType.MakeGenericType(typeof(TEntity)), _context);

        _repositories.Add(type, instance);

        return (IRepository<TEntity>) _repositories[type]!;
    }

    
}
