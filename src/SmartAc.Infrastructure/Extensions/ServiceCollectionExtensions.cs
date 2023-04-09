using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
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

        services.AddDbContext<SmartAcContext>(options =>
        {
            options.UseSqlite(connectionString, builder => builder.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
            //options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTrackingWithIdentityResolution);
            options.LogTo(Console.WriteLine, new[] { RelationalEventId.CommandExecuted }, LogLevel.Information);
        });

        //services.TryAddScoped<IUnitOfWork, UnitOfWork>();
        services.TryAddScoped<ISmartAcJwtService, SmartAcJwtService>();
        services.TryAddScoped(typeof(IRepository<>), typeof(Repository<>));

        return services;
    }
}