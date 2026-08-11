using Transportation.Application.CrewMembership.Repositories;
using Transportation.Application.CrewMembership.Specification;
using Transportation.Application.TaxiExpense;
using Transportation.Application.TransportDay.Models;
using Transportation.Domain.Entities;
using Transportation.Mediator.Helper.Exceptions;

namespace Transportation.Application.TransportDay.Services;

/// <summary>
/// Keeps a transport day's taxi expenses in step with the legs the crew actually took a
/// taxi on. Taxi expenses belong to the day, so they are written through the day rather
/// than logged a second time by hand — that duplicate step is what used to leave a taxi
/// leg with no reimbursable expense behind it.
/// </summary>
public interface ITransportDayTaxiFareService
{
    Task SyncAsync(
        Domain.Entities.TransportDay transportDay,
        IReadOnlyList<TransportDayTaxiFare> fares,
        DateTime now,
        CancellationToken cancellationToken);
}

internal sealed class TransportDayTaxiFareService : ITransportDayTaxiFareService
{
    private readonly ICrewMembershipRepository _crewMembershipRepository;

    public TransportDayTaxiFareService(ICrewMembershipRepository crewMembershipRepository)
    {
        _crewMembershipRepository = crewMembershipRepository;
    }

    public async Task SyncAsync(
        Domain.Entities.TransportDay transportDay,
        IReadOnlyList<TransportDayTaxiFare> fares,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var taxiLegs = GetTaxiLegs(transportDay);

        Validate(taxiLegs, fares);

        if (fares.Count > 0)
            await EnsurePayersAreCrewMembers(transportDay.CrewId, fares, cancellationToken);

        // A leg that is no longer a taxi ride must not keep claiming money.
        foreach (var orphan in transportDay.TaxiExpenses.Where(x => !x.IsDeleted && !taxiLegs.Contains(x.Leg)))
        {
            orphan.IsDeleted = true;
            orphan.UpdatedAt = now;
        }

        foreach (var fare in fares)
        {
            var existing = transportDay.TaxiExpenses
                .FirstOrDefault(x => !x.IsDeleted && x.Leg == fare.Leg);

            if (existing is null)
            {
                transportDay.TaxiExpenses.Add(new Domain.Entities.TaxiExpense
                {
                    Leg = fare.Leg,
                    Amount = fare.Amount,
                    PaidById = fare.PaidById,
                    TaxiExpenseStatus = TaxiExpenseStatus.Pending,
                    CreatedAt = now
                });

                continue;
            }

            // An expense that has already been ruled on is an accounting fact; the daily
            // log may no longer rewrite it.
            if (existing.TaxiExpenseStatus is not TaxiExpenseStatus.Pending)
                throw new BusinessLogicException(TaxiExpenseErrors.ExpenseNotPending);

            existing.Amount = fare.Amount;
            existing.PaidById = fare.PaidById;
            existing.UpdatedAt = now;
        }
    }

    private static HashSet<Leg> GetTaxiLegs(Domain.Entities.TransportDay transportDay)
    {
        var legs = new HashSet<Leg>();

        if (transportDay.MorningMode is TransportMode.Taxi)
            legs.Add(Leg.Morning);

        if (transportDay.AfternoonMode is TransportMode.Taxi)
            legs.Add(Leg.Afternoon);

        return legs;
    }

    private static void Validate(IReadOnlyCollection<Leg> taxiLegs, IReadOnlyList<TransportDayTaxiFare> fares)
    {
        if (fares.Select(x => x.Leg).Distinct().Count() != fares.Count)
            throw new BusinessLogicException(TaxiExpenseErrors.ExpenseThisLegAlreadyExist);

        if (fares.Any(fare => !taxiLegs.Contains(fare.Leg)))
            throw new BusinessLogicException(TransportDayErrors.TaxiFareOnNonTaxiLeg);

        if (taxiLegs.Any(leg => fares.All(fare => fare.Leg != leg)))
            throw new BusinessLogicException(TransportDayErrors.TaxiLegWithoutFare);
    }

    private async Task EnsurePayersAreCrewMembers(
        long crewId,
        IReadOnlyList<TransportDayTaxiFare> fares,
        CancellationToken cancellationToken)
    {
        var memberships = await _crewMembershipRepository
            .ListAsync(new CrewMembershipByCrewIdSpec(crewId), cancellationToken);

        var memberIds = memberships.Select(x => x.UserId).ToHashSet();

        if (fares.Any(fare => !memberIds.Contains(fare.PaidById)))
            throw new BusinessLogicException(TaxiExpenseErrors.ExpensePaidByNonExistentCrewMember);
    }
}
