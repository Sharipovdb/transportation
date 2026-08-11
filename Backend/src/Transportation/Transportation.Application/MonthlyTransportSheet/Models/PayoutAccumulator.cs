namespace Transportation.Application.MonthlyTransportSheet.Models;

public sealed class PayoutAccumulator
{
    public long UserId { get; init; }

    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;

    // Kilometres driven in a member's own car are reported as distance and priced
    // manually outside this system — they never turn into an amount here.
    public double DriverKm { get; set; }

    public double ExtraBusinessKm { get; set; }

    public decimal TaxiCompensation { get; set; }

    public List<TaxiExpenseSummaryDto> TaxiExpenses { get; } = [];

    public decimal TotalAmount => TaxiCompensation;
}