using Transportation.Application.CrewMembership.Repositories;
using Transportation.Application.CrewMembership.Specification;
using Transportation.Application.MonthlyTransportSheet.Models;
using Transportation.Application.TransportDay;
using Transportation.Application.TransportDay.Repositories;
using Transportation.Application.TransportDay.Specifications;
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

/// <summary>
/// Turns a crew's confirmed transport days into one payout line per person.
///
/// Two units of account are kept strictly apart. Kilometres driven in a member's own
/// car are <em>reported</em> — commute legs and extra business km are summed as distance
/// and priced manually outside this system. Taxi legs are <em>settled</em> — they produce
/// no distance at all, only the fare the member fronted, which the company owes back.
/// </summary>
internal sealed class MonthlyTransportCalculator : IMonthlyTransportCalculator
{
    private readonly ITransportDayRepository _transportDayRepository;
    private readonly ICrewMembershipRepository _crewMembershipRepository;

    public MonthlyTransportCalculator(
        ITransportDayRepository transportDayRepository,
        ICrewMembershipRepository crewMembershipRepository)
    {
        _transportDayRepository = transportDayRepository;
        _crewMembershipRepository = crewMembershipRepository;
    }

    public async Task<MonthlyTransportCalculationResult> CalculateAsync(
        long crewId, int year, int month,
        CancellationToken cancellationToken)
    {
        var transportDays = await _transportDayRepository
            .ListAsync(new TransportDaysByPeriodSpec(crewId, year, month), cancellationToken);

        if (transportDays.Count == 0)
            throw new ResourceNotFoundException(TransportDayErrors.NotFound);

        var memberships = await _crewMembershipRepository
            .ListAsync(new ActiveCrewMembershipsWithUserByCrewIdSpec(crewId), cancellationToken);

        var payouts = CalculatePayouts(transportDays, memberships);

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
        List<Domain.Entities.CrewMembership> memberships)
    {
        var payouts = new Dictionary<long, PayoutAccumulator>();

        // Seed every active member first so the sheet lists the whole crew in a stable
        // order, including members who neither drove nor fronted taxi cash this month.
        // Anyone else who earned something (a driver who has since left the crew, an
        // outside payer) is appended by the passes below.
        foreach (var membership in memberships.OrderBy(x => x.User.FirstName).ThenBy(x => x.User.LastName))
            GetAccumulator(payouts, membership.User);

        foreach (var transportDay in transportDays)
        {
            ProcessDrivenKm(transportDay, payouts);

            ProcessExtraBusinessKm(transportDay, payouts);

            ProcessTaxiExpenses(transportDay, payouts);
        }

        return payouts;
    }

    private static void ProcessDrivenKm(
        Domain.Entities.TransportDay transportDay,
        IDictionary<long, PayoutAccumulator> payouts)
    {
        if (transportDay.Driver is null)
            return;

        if (transportDay.DrivenCommuteKm <= 0)
            return;

        var accumulator = GetAccumulator(payouts, transportDay.Driver);

        accumulator.DriverKm += transportDay.DrivenCommuteKm;
    }

    private static void ProcessExtraBusinessKm(
        Domain.Entities.TransportDay transportDay,
        IDictionary<long, PayoutAccumulator> payouts)
    {
        if (transportDay.Driver is null)
            return;

        if (transportDay.ExtraBusinessKm <= 0)
            return;

        var accumulator = GetAccumulator(payouts, transportDay.Driver);

        accumulator.ExtraBusinessKm += transportDay.ExtraBusinessKm;
    }

    private static void ProcessTaxiExpenses(
        Domain.Entities.TransportDay transportDay,
        IDictionary<long, PayoutAccumulator> payouts)
    {
        foreach (var taxiExpense in transportDay.TaxiExpenses
                     .Where(x => !x.IsDeleted && x.TaxiExpenseStatus == TaxiExpenseStatus.Approved))
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
