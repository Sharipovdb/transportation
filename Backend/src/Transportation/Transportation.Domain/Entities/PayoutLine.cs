namespace Transportation.Domain.Entities;

public sealed class PayoutLine : BaseEntity
{
    public long MonthlyTransportSheetId { get; set; }
    public MonthlyTransportSheet MonthlyTransportSheet { get; set; } = null!;
    
    public long UserId { get; set; }
    public User User { get; set; } = null!;

    public decimal? DriverPayment { get; set; }
    public decimal? ExtraKmPayment { get; set; }
    public decimal? TaxiCompensation { get; set; }

    public List<PayoutLineTaxiExpense> TaxiExpenses { get; set; } = new();
    public decimal TotalAmount => (decimal)(DriverPayment + ExtraKmPayment + TaxiCompensation)!;
}