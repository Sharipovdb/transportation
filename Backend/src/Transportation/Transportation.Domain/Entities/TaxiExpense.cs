namespace Transportation.Domain.Entities;

public class TaxiExpense : BaseEntity
{
    public long TransportDayId { get; set; }
    public TransportDay TransportDay { get; set; } = null!;

    public Leg Leg { get; set; }
    public decimal Amount { get; set; }
    public long PaidById { get; set; }
    public User PaidBy { get; set; } = null!;
    public TaxiExpenseStatus TaxiExpenseStatus { get; set; }
    
    public List<PayoutLineTaxiExpense> PayoutLines { get; set; } = new();
}

public enum Leg
{
    Morning = 1,
    Afternoon = 2
}

public enum TaxiExpenseStatus
{
    Pending = 1,
    Approved = 2,
    Rejected = 3,
    Paid = 4
}