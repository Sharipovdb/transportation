using Transportation.Domain.Entities;

namespace Transportation.Application.MonthlyTransportSheet.Models;

public sealed class TaxiExpenseSummaryDto
{
    public long Id { get; set; }

    public decimal Amount { get; set; }

    public Leg Leg { get; set; }

    public TaxiExpenseStatus TaxiExpenseStatus { get; set; }
}