namespace Transportation.Application.MonthlyTransportSheet.Models;

public sealed class PayoutAccumulator
{
    public long UserId { get; init; }

    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;

    public decimal DriverPayment { get; set; }

    public decimal ExtraKmPayment { get; set; }

    public decimal TaxiCompensation { get; set; }

    public List<TaxiExpenseSummaryDto> TaxiExpenses { get; } = [];

    public decimal TotalAmount => DriverPayment + ExtraKmPayment + TaxiCompensation;
}