using Ardalis.Specification;

namespace Transportation.Application.Crew.Specification;

public class CrewByLeadsId : Specification<Domain.Entities.Crew>
{
    public CrewByLeadsId(long? leadId, long? driverLeadId, long? excludeCrewId = null, bool asNoTracking = false)
    {
        if (asNoTracking)
            Query.AsNoTracking();

        if (excludeCrewId.HasValue)
            Query.Where(c => c.Id != excludeCrewId.Value);

        if (leadId == null && driverLeadId == null)
            return;

        Query.Where(c => (leadId != null && c.CrewLeadId == leadId) || 
                         (driverLeadId != null && c.DriverLeadId == driverLeadId));
    }
}