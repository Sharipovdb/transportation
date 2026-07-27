using Transportation.Application.MonthlyTransportSheet.Models;
using Transportation.Mediator.Helper.Common.Extensions;

namespace Transportation.Application.MonthlyTransportSheet.Factories;

public interface IMonthlyTransportSheetFactory
{
    Domain.Entities.MonthlyTransportSheet Create(MonthlyTransportCalculationResult calculation);
}

internal sealed class MonthlyTransportSheetFactory : IMonthlyTransportSheetFactory
{
    private readonly TimeProvider _timeProvider;

    public MonthlyTransportSheetFactory(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }

    public Domain.Entities.MonthlyTransportSheet Create(MonthlyTransportCalculationResult calculation)
    {
        var now = _timeProvider.GetLocalDateTimeNowKindUtc();

        var sheet = new Domain.Entities.MonthlyTransportSheet
        {
            CrewId = calculation.CrewId,
            Year = calculation.Year,
            Month = calculation.Month,
            IsConfirmed = false,
            CreatedAt = now
        };

        foreach (var payout in calculation.Payouts)
        {
            var payoutLine = new Domain.Entities.PayoutLine
            {
                UserId = payout.UserId,
                DriverPayment = payout.DriverPayment,
                ExtraKmPayment = payout.ExtraKmPayment,
                TaxiCompensation = payout.TaxiCompensation,
                CreatedAt = now
            };

            foreach (var taxiExpense in payout.TaxiExpenses)
            {
                payoutLine.TaxiExpenses.Add(new Domain.Entities.PayoutLineTaxiExpense
                {
                    TaxiExpenseId = taxiExpense.Id,
                    CreatedAt = now
                });
            }

            sheet.PayoutLines.Add(payoutLine);
        }

        return sheet;
    }
}