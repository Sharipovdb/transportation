using Ardalis.Specification;

namespace Transportation.Application.Crew.Specification;

/// <summary>
/// Does a live crew already have this person as its lead? A lead belongs to one crew at
/// a time, and deleted crews are excluded: a deleted crew used to keep holding its lead,
/// so that person could never be made lead of anything again.
/// </summary>
public sealed class CrewByLeadsId : Specification<Domain.Entities.Crew>
{
    public CrewByLeadsId(long? leadId, long? driverLeadId, long? excludeCrewId = null, bool asNoTracking = false)
    {
        if (asNoTracking)
            Query.AsNoTracking();

        Query.Where(c => !c.IsDeleted);

        if (excludeCrewId.HasValue)
            Query.Where(c => c.Id != excludeCrewId.Value);

        if (leadId == null && driverLeadId == null)
            return;

        Query.Where(c => (leadId != null && c.CrewLeadId == leadId) ||
                         (driverLeadId != null && c.DriverLeadId == driverLeadId));
    }
}
