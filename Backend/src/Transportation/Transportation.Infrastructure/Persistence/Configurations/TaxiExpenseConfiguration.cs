using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Transportation.Domain.Entities;

namespace Transportation.Infrastructure.Persistence.Configurations;

public class TaxiExpenseConfiguration : IEntityTypeConfiguration<TaxiExpense>
{
    public void Configure(EntityTypeBuilder<TaxiExpense> builder)
    {
        builder.ToTable("TaxiExpense");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Amount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.Leg)
            .IsRequired();

        builder.Property(x => x.TaxiExpenseStatus)
            .IsRequired();

        builder.HasOne(x => x.TransportDay)
            .WithMany(x => x.TaxiExpenses)
            .HasForeignKey(x => x.TransportDayId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.TransportDayId);
    }
}