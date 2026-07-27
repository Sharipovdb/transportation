using Transportation.Application.MonthlyTransportSheet.Models;

namespace Transportation.Application.PayoutLine.Models;

public sealed class PayoutLineDto
{
    public long Id { get; set; }

    public long UserId { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public decimal DriverPayment { get; set; }

    public decimal ExtraKmPayment { get; set; }

    public decimal TaxiCompensation { get; set; }

    public decimal TotalAmount => DriverPayment + ExtraKmPayment + TaxiCompensation;

    public IEnumerable<TaxiExpenseSummaryDto> TaxiExpenses { get; set; } = [];
}