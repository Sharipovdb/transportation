using Transportation.Application.MonthlyTransportSheet.Models;
using Transportation.Application.TransportDay;
using Transportation.Application.TransportDay.Repositories;
using Transportation.Application.TransportDay.Specifications;
using Transportation.Application.TransportSettings;
using Transportation.Application.TransportSettings.Repositories;
using Transportation.Application.TransportSettings.Specifications;
using Transportation.Domain.Entities;
using Transportation.Mediator.Helper.Exceptions;

namespace Transportation.Application.MonthlyTransportSheet.Calculations;

public interface IMonthlyTransportCalculator
{
    Task<MonthlyTransportCalculationResult> CalculateAsync(
        long crewId, int year, int month,
        CancellationToken cancellationToken
    );
}

internal sealed class MonthlyTransportCalculator : IMonthlyTransportCalculator
{
    private readonly ITransportDayRepository _transportDayRepository;
    private readonly ITransportSettingsRepository _settingsRepository;

    public MonthlyTransportCalculator(
        ITransportDayRepository transportDayRepository,
        ITransportSettingsRepository settingsRepository)
    {
        _transportDayRepository = transportDayRepository;
        _settingsRepository = settingsRepository;
    }

    public async Task<MonthlyTransportCalculationResult> CalculateAsync(
        long crewId, int year, int month,
        CancellationToken cancellationToken)
    {
        var date = DateTime.SpecifyKind(new DateTime(year, month, 1), DateTimeKind.Utc);

        var settings = await _settingsRepository
            .FirstOrDefaultAsync(new TransportSettingsByDateSpec(date), cancellationToken);

        if (settings is null)
            throw new ResourceNotFoundException(TransportSettingsErrors.NotFound);

        var transportDays = await _transportDayRepository
            .ListAsync(new TransportDaysByPeriodSpec(crewId, year, month), cancellationToken);

        if (transportDays.Count == 0)
            throw new ResourceNotFoundException(TransportDayErrors.NotFound);

        var payouts = CalculatePayouts(transportDays, settings);

        return new MonthlyTransportCalculationResult
        {
            CrewId = crewId,
            Year = year,
            Month = month,
            Payouts = payouts.Values.ToList()
        };
    }

    private static Dictionary<long, PayoutAccumulator> CalculatePayouts(
        List<Domain.Entities.TransportDay> transportDays,
        Domain.Entities.TransportSettings settings)
    {
        var payouts = new Dictionary<long, PayoutAccumulator>();

        foreach (var transportDay in transportDays)
        {
            ProcessDriver(transportDay, settings, payouts);

            ProcessExtraBusinessKm(transportDay, settings, payouts);

            ProcessTaxiExpenses(transportDay, payouts);
        }

        return payouts;
    }

    private static void ProcessDriver(
        Domain.Entities.TransportDay transportDay,
        Domain.Entities.TransportSettings settings,
        IDictionary<long, PayoutAccumulator> payouts)
    {
        if (transportDay.Driver is null)
            return;

        var commuteCount = 0;

        if (transportDay.MorningMode is TransportMode.Driven)
            commuteCount++;

        if (transportDay.AfternoonMode is TransportMode.Driven)
            commuteCount++;

        var commuteKm = transportDay.BaseRouteKm + transportDay.ExtraCommuteKm;

        var payment = commuteCount * (decimal)commuteKm * settings.CommuteKmRate;

        GetAccumulator(payouts, transportDay.Driver).DriverPayment += payment;
    }

    private static void ProcessExtraBusinessKm(
        Domain.Entities.TransportDay transportDay,
        Domain.Entities.TransportSettings settings,
        IDictionary<long, PayoutAccumulator> payouts)
    {
        if (transportDay.Driver is null)
            return;

        if (transportDay.ExtraBusinessKm <= 0)
            return;

        var payment = (decimal)transportDay.ExtraBusinessKm * settings.ExtraBusinessKmRate;

        GetAccumulator(payouts, transportDay.Driver).ExtraKmPayment += payment;
    }

    private static void ProcessTaxiExpenses(
        Domain.Entities.TransportDay transportDay,
        IDictionary<long, PayoutAccumulator> payouts)
    {
        foreach (var taxiExpense in transportDay.TaxiExpenses
                     .Where(x => x.TaxiExpenseStatus == TaxiExpenseStatus.Approved))
        {
            var accumulator = GetAccumulator(payouts, taxiExpense.PaidBy);

            accumulator.TaxiCompensation += taxiExpense.Amount;

            accumulator.TaxiExpenses.Add(new TaxiExpenseSummaryDto
            {
                Id = taxiExpense.Id,
                Amount = taxiExpense.Amount,
                Leg = taxiExpense.Leg,
                TaxiExpenseStatus = taxiExpense.TaxiExpenseStatus
            });
        }
    }

    private static PayoutAccumulator GetAccumulator(
        IDictionary<long, PayoutAccumulator> payouts,
        Domain.Entities.User user)
    {
        if (payouts.TryGetValue(user.Id, out var accumulator))
            return accumulator;

        accumulator = new PayoutAccumulator
        {
            UserId = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
        };

        payouts.Add(user.Id, accumulator);

        return accumulator;
    }
}