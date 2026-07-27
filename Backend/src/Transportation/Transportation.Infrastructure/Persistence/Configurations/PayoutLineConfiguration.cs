using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Transportation.Domain.Entities;

namespace Transportation.Infrastructure.Persistence.Configurations;

public sealed class PayoutLineConfiguration : IEntityTypeConfiguration<PayoutLine>
{
    public void Configure(EntityTypeBuilder<PayoutLine> builder)
    {
        builder.ToTable("payout_lines");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.DriverPayment)
            .HasPrecision(18, 2);

        builder.Property(x => x.ExtraKmPayment)
            .HasPrecision(18, 2);

        builder.Property(x => x.TaxiCompensation)
            .HasPrecision(18, 2);

        builder.HasOne(x => x.MonthlyTransportSheet)
            .WithMany(x => x.PayoutLines)
            .HasForeignKey(x => x.MonthlyTransportSheetId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}