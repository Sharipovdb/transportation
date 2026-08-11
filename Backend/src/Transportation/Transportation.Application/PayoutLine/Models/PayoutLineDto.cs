using Transportation.Application.MonthlyTransportSheet.Models;

namespace Transportation.Application.PayoutLine.Models;

public sealed class PayoutLineDto
{
    public long Id { get; set; }

    public long UserId { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    // Distance is reported, not priced — see PayoutLine for why there is no km rate.
    public double DriverKm { get; set; }

    public double ExtraBusinessKm { get; set; }

    public decimal TaxiCompensation { get; set; }

    public bool IsPaid { get; set; }

    public DateTime? PaidAt { get; set; }

    public decimal TotalAmount => TaxiCompensation;

    public IEnumerable<TaxiExpenseSummaryDto> TaxiExpenses { get; set; } = [];
}