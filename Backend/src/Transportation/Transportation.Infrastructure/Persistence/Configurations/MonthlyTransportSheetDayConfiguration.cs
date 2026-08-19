using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Transportation.Domain.Entities;

namespace Transportation.Infrastructure.Persistence.Configurations;

public sealed class MonthlyTransportSheetDayConfiguration : IEntityTypeConfiguration<MonthlyTransportSheetDay>
{
    public void Configure(EntityTypeBuilder<MonthlyTransportSheetDay> builder)
    {
        builder.ToTable("monthly_transport_sheet_days");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Date)
            .IsRequired();

        builder.Property(x => x.DrivenKm)
            .HasPrecision(10, 2);

        builder.Property(x => x.ExtraBusinessKm)
            .HasPrecision(10, 2);

        builder.Property(x => x.TaxiAmount)
            .HasPrecision(18, 2);

        // A day appears at most once on a sheet — the report grid has one cell per date.
        builder.HasIndex(x => new
        {
            x.MonthlyTransportSheetId,
            x.TransportDayId
        }).IsUnique();

        builder.HasOne(x => x.TransportDay)
            .WithMany()
            .HasForeignKey(x => x.TransportDayId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
