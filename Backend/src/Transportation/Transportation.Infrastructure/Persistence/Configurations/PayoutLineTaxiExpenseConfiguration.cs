using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Transportation.Domain.Entities;

namespace Transportation.Infrastructure.Persistence.Configurations;

public class PayoutLineTaxiExpenseConfiguration : IEntityTypeConfiguration<PayoutLineTaxiExpense>
{
    public void Configure(EntityTypeBuilder<PayoutLineTaxiExpense> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.HasOne(x => x.PayoutLine)
            .WithMany(x => x.TaxiExpenses)
            .HasForeignKey(x => x.PayoutLineId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(x => x.TaxiExpense)
            .WithMany(x => x.PayoutLines)
            .HasForeignKey(x => x.TaxiExpenseId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}