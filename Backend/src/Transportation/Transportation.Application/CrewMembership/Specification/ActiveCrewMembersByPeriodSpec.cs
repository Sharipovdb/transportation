using Ardalis.Specification;

namespace Transportation.Application.CrewMembership.Specification;

public sealed class ActiveCrewMembersByPeriodSpec : Specification<Domain.Entities.CrewMembership>
{
    public ActiveCrewMembersByPeriodSpec(
        long crewId,
        int year,
        int month,
        bool asNoTracking = false)
    {
        var from = new DateTime(year, month, 1);
        var to = from.AddMonths(1);

        if (asNoTracking)
            Query.AsNoTracking();

        Query.Where(x =>
            x.CrewId == crewId &&
            !x.IsDeleted &&
            x.ActiveFrom < to &&
            (x.ActiveTo == null || x.ActiveTo >= from));

        Query.Include(x => x.User);
    }
}