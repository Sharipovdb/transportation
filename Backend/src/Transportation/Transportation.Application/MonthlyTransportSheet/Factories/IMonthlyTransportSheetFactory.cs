using Transportation.Application.MonthlyTransportSheet.Models;
using Transportation.Mediator.Helper.Common.Extensions;
using Transportation.Shared;
using Transportation.Shared.Extensions;
using Transportation.Shared.Middlewares;

namespace Transportation.Application.MonthlyTransportSheet.Factories;

public interface IMonthlyTransportSheetFactory
{
    Domain.Entities.MonthlyTransportSheet Create(MonthlyTransportCalculationResult calculation);
}

internal sealed class MonthlyTransportSheetFactory : IMonthlyTransportSheetFactory
{
    private readonly TimeProvider _timeProvider;
    private readonly ICurrentUserAccessor _currentUserAccessor;

    public MonthlyTransportSheetFactory(TimeProvider timeProvider, ICurrentUserAccessor currentUserAccessor)
    {
        _timeProvider = timeProvider;
        _currentUserAccessor = currentUserAccessor;
    }

    public Domain.Entities.MonthlyTransportSheet Create(MonthlyTransportCalculationResult calculation)
    {
        var now = _timeProvider.GetLocalDateTimeNowKindUtc();
        var currentUserId = _currentUserAccessor.GetRequiredUser().GetUserId();

        var sheet = new Domain.Entities.MonthlyTransportSheet
        {
            CrewId = calculation.CrewId,
            Year = calculation.Year,
            Month = calculation.Month,
            IsConfirmed = false,
            CreatedAt = now,
            CreatedById = currentUserId,
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