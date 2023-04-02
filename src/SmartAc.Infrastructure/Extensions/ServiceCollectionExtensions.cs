using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SmartAc.Application.Abstractions.Repositories;
using SmartAc.Application.Abstractions.Services;
using SmartAc.Infrastructure.Repositories;
using SmartAc.Infrastructure.Services;

namespace SmartAc.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString("smartac") ?? "Data Source=SmartAc.db";

        services.AddSqlite<SmartAcContext>(connectionString, optionsAction: builder =>
        {
            builder.LogTo(Console.WriteLine, new[] { RelationalEventId.CommandExecuted });
        });

        services.TryAddScoped<IUnitOfWork, UnitOfWork>();

        services.TryAddScoped(typeof(IRepository<>), typeof(Repository<>));

        services.TryAddTransient<ISmartAcJwtService, SmartAcJwtService>();

        return services;
    }
}