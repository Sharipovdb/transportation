using Ardalis.Specification;

namespace Transportation.Application.TransportDay.Specifications;

/// <summary>
/// The days a crew's month is built from — every day logged in the period.
///
/// Kilometres need no sign-off: the crew's route is attached to the crew, so the distance
/// a driven day is worth is known the moment the day is logged. Money is the part that is
/// reviewed, and that review happens on the taxi fare itself (Pending → Approved), not on
/// the day.
/// </summary>
public sealed class TransportDaysByPeriodSpec : Specification<Domain.Entities.TransportDay>
{
    public TransportDaysByPeriodSpec(long crewId, int year, int month, bool asNoTracking = false)
    {
        var from = new DateOnly(year, month, 1);
        var to = from.AddMonths(1);

        if (asNoTracking)
            Query.AsNoTracking();

        Query.Where(x =>
            x.CrewId == crewId &&
            !x.IsDeleted &&
            x.Date >= from &&
            x.Date < to);

        Query.Include(x => x.TaxiExpenses);

        Query.OrderBy(x => x.Date);
    }
}
