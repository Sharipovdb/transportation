namespace Transportation.Application.TaxiExpense.Models;

public class TaxiExpenseDto
{
    public long Id { get; set; }

    public long TransportDayId { get; set; }

    public long PaidById { get; set; }

    public string Leg { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public string TaxiExpenseStatus { get; set; } = string.Empty;
}