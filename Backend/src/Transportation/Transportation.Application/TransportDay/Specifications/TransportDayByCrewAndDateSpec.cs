using Ardalis.Specification;
using Microsoft.EntityFrameworkCore;

namespace Transportation.Application.TransportDay.Specifications;


public sealed class TransportDayByCrewAndDateSpec : Specification<Domain.Entities.TransportDay>
{

    public TransportDayByCrewAndDateSpec(long crewId, DateTime date, bool asNoTracking = false)
    {
        {
            if (asNoTracking)
                Query.AsNoTracking();
            Query.Where(x => x.CrewId == crewId && x.Date.Date == date.ToUniversalTime().Date);
        }
    }
}