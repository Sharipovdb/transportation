using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Transportation.Domain.Entities;

namespace Transportation.Infrastructure.Persistence.Configurations;

public class TransportDayConfiguration : IEntityTypeConfiguration<TransportDay>
{
    public void Configure(EntityTypeBuilder<TransportDay> builder)
    {
        builder.ToTable("transport_days");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Date)
            .IsRequired();

        builder.Property(x => x.ExtraCommuteKm)
            .HasPrecision(10, 2);

        builder.Property(x => x.ExtraBusinessKm)
            .HasPrecision(10, 2);

        builder.Property(x => x.Notes)
            .HasMaxLength(500);

        builder.Property(x => x.Confirmed)
            .HasDefaultValue(false);

        builder.HasOne(x => x.Crew)
            .WithMany()
            .HasForeignKey(x => x.CrewId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.LoggedByUser)
            .WithMany()
            .HasForeignKey(x => x.LoggedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Driver)
            .WithMany()
            .HasForeignKey(x => x.DriverId);
    }
}