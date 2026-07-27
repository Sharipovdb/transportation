using Ardalis.Specification;

namespace Transportation.Application.TransportDay.Specifications;

public sealed class TransportDaysByPeriodSpec : Specification<Domain.Entities.TransportDay>
{
    public long CrewId { get; }

    public DateTime From { get; }

    public DateTime To { get; }

    public TransportDaysByPeriodSpec(long crewId, int year, int month, bool asNoTracking = false)
    {
        CrewId = crewId;

        From = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
        To = From.AddMonths(1);

        if (asNoTracking)
            Query.AsNoTracking();

        Query.Where(x =>
            x.CrewId == crewId &&
            !x.IsDeleted &&
            x.Confirmed &&
            x.Date >= From &&
            x.Date < To);

        Query.Include(x => x.TaxiExpenses)
            .ThenInclude(x => x.PaidBy);

        Query.Include(x => x.Driver);
        Query.Include(x => x.Crew);
        Query.Include(x => x.LoggedByUser);
    }
}