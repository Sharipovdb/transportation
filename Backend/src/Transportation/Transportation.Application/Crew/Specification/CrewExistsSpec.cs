using Ardalis.Specification;

namespace Transportation.Application.Crew.Specification;

/// <summary>
/// Does a live crew already use this name? Deleted crews are excluded — nothing is
/// really removed from this database, and without the filter a crew that had been
/// deleted kept its name reserved forever, so re-creating it was rejected as a duplicate.
/// </summary>
public sealed class CrewExistsSpec : Specification<Domain.Entities.Crew>
{
    public CrewExistsSpec(string name, long? excludeCrewId = null, bool asNoTracking = false)
    {
        if (asNoTracking)
            Query.AsNoTracking();

        if (excludeCrewId.HasValue)
            Query.Where(c => c.Id != excludeCrewId.Value);

        Query.Where(c => c.Name == name && !c.IsDeleted);
    }
}
