using Ardalis.Specification;

namespace Transportation.Application.TransportDay.Specifications;

public sealed class TransportDayByCrewAndDateSpec : Specification<Domain.Entities.TransportDay>
{
    public TransportDayByCrewAndDateSpec(long crewId, DateOnly date, bool asNoTracking = false)
    {
        if (asNoTracking)
            Query.AsNoTracking();

        Query.Where(x => x.CrewId == crewId && x.Date == date && !x.IsDeleted);
    }
}