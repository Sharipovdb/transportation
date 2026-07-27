using Ardalis.Specification;

namespace Transportation.Application.Crew.Specification;

public sealed class CrewExistsSpec : Specification<Domain.Entities.Crew>
{
    public CrewExistsSpec(string name, long? excludeCrewId = null, bool asNoTracking = false)
    {
        if (asNoTracking)
            Query.AsNoTracking();

        if (excludeCrewId.HasValue)
            Query.Where(c => c.Id != excludeCrewId.Value);

        Query.Where(c => c.Name == name);
    }
}