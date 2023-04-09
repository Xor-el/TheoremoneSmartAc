using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAc.Domain;

namespace SmartAc.Infrastructure.Configurations;

internal sealed class DeviceConfiguration : IEntityTypeConfiguration<Device>
{
    public void Configure(EntityTypeBuilder<Device> builder)
    {
        builder
            .HasMany(x => x.DeviceReadings)
            .WithOne()
            .IsRequired();

        builder
            .HasMany(x => x.DeviceRegistrations)
            .WithOne()
            .IsRequired();

        builder
            .HasMany(x => x.Alerts)
            .WithOne()
            .IsRequired();

        builder
            .HasIndex(x => x.SharedSecret)
            .IsUnique();
    }
}