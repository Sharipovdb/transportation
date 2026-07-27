using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Transportation.Domain.Entities;

namespace Transportation.Infrastructure.Persistence.Configurations;

public sealed class MonthlyTransportSheetConfiguration : IEntityTypeConfiguration<MonthlyTransportSheet>
{
    public void Configure(EntityTypeBuilder<MonthlyTransportSheet> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Year)
            .IsRequired();

        builder.Property(x => x.Month)
            .IsRequired();

        builder.Property(x => x.IsConfirmed)
            .HasDefaultValue(false);

        builder.HasIndex(x => new
        {
            x.CrewId,
            x.Year,
            x.Month
        }).IsUnique();

        builder.HasMany(x => x.PayoutLines)
            .WithOne(x => x.MonthlyTransportSheet)
            .HasForeignKey(x => x.MonthlyTransportSheetId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}