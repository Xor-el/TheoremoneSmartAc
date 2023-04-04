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
            .WithOne(x => x.Device)
            .IsRequired();

        builder
            .HasMany(x => x.DeviceRegistrations)
            .WithOne(x => x.Device)
            .IsRequired();

        builder
            .HasMany(x => x.Alerts)
            .WithOne(x => x.Device)
            .IsRequired();

        builder
            .HasIndex(x => x.SharedSecret)
            .IsUnique();
    }
}