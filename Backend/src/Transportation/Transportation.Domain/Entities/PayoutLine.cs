namespace Transportation.Domain.Entities;

public sealed class PayoutLine : BaseEntity
{
    public long MonthlyTransportSheetId { get; set; }
    public MonthlyTransportSheet MonthlyTransportSheet { get; set; } = null!;
    
    public long UserId { get; set; }
    public User User { get; set; } = null!;

    // Distance is reported, never priced: kilometres driven in a member's own car are
    // settled outside this system, so the sheet states them and stops there.
    public double DriverKm { get; set; }
    public double ExtraBusinessKm { get; set; }

    // The only money the sheet owes: taxi fares the member fronted out of pocket.
    public decimal? TaxiCompensation { get; set; }

    // Settlement state. A line is paid at most once per sheet: PaidAt/PaidById record
    // who released the money so a second payout can be refused with an audit trail.
    public bool IsPaid { get; set; }
    public DateTime? PaidAt { get; set; }
    public long? PaidById { get; set; }
    public User? PaidBy { get; set; }

    public List<PayoutLineTaxiExpense> TaxiExpenses { get; set; } = new();

    public decimal TotalAmount => TaxiCompensation ?? 0;
}