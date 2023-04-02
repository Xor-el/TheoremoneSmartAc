using System.Reflection.Metadata;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SmartAc.Domain;

namespace SmartAc.Infrastructure.Repositories;

public sealed class SmartAcContext : DbContext
{
    public SmartAcContext(DbContextOptions<SmartAcContext> options) : base(options)
    {
    }

    public DbSet<Device> Devices => Set<Device>();
    public DbSet<DeviceRegistration> DeviceRegistrations => Set<DeviceRegistration>();
    public DbSet<DeviceReading> DeviceReadings => Set<DeviceReading>();
    public DbSet<Alert> Alerts => Set<Alert>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder
            .Properties<DateTimeOffset>()
            .HaveConversion<DateTimeOffsetToStringConverter>(); // SqlLite workaround for DateTimeOffset sorting

        configurationBuilder
            .Properties<decimal>()
            .HaveConversion<double>(); // SqlLite workaround for decimal aggregations
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AssemblyReference).Assembly);
    }
}
