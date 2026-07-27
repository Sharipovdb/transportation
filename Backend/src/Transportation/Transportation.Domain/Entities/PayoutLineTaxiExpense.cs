namespace Transportation.Domain.Entities;

public sealed class PayoutLineTaxiExpense : BaseEntity
{
    public long PayoutLineId { get; set; }
    public PayoutLine PayoutLine { get; set; } = null!;

    public long TaxiExpenseId { get; set; }
    public TaxiExpense TaxiExpense { get; set; } = null!;
}