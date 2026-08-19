using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Transportation.Domain.Entities;

namespace Transportation.Infrastructure.Persistence.Configurations;

public sealed class MonthlyTransportSheetConfiguration : IEntityTypeConfiguration<MonthlyTransportSheet>
{
    public void Configure(EntityTypeBuilder<MonthlyTransportSheet> builder)
    {
        builder.ToTable("monthly_transport_sheets");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Year)
            .IsRequired();

        builder.Property(x => x.Month)
            .IsRequired();

        builder.Property(x => x.IsConfirmed)
            .HasDefaultValue(false);

        builder.Property(x => x.IsPaid)
            .HasDefaultValue(false);

        // One sheet per crew per month is what makes "paid once" enforceable. Deleted
        // sheets are excluded: deleting and generating again is the documented way to
        // correct a month, and an unfiltered index made the second step fail.
        builder.HasIndex(x => new
            {
                x.CrewId,
                x.Year,
                x.Month
            })
            .IsUnique()
            .HasFilter(SoftDelete.NotDeleted);

        builder.HasOne(x => x.Crew)
            .WithMany()
            .HasForeignKey(x => x.CrewId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Recipient)
            .WithMany()
            .HasForeignKey(x => x.RecipientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.PaidBy)
            .WithMany()
            .HasForeignKey(x => x.PaidById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Days)
            .WithOne(x => x.MonthlyTransportSheet)
            .HasForeignKey(x => x.MonthlyTransportSheetId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
