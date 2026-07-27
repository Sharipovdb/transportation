using Ardalis.Specification;

namespace Transportation.Application.Crew.Specification;

public class CrewByLeadId : Specification<Domain.Entities.Crew>
{
    public CrewByLeadId(long? leadId, long? driverLeadId, long? excludeCrewId = null, bool asNoTracking = false)
    {
        if (asNoTracking)
            Query.AsNoTracking();

        if (excludeCrewId.HasValue)
        {
            Query.Where(c => c.Id != excludeCrewId.Value);
        }

        Query.Where(c => (leadId != null && c.CrewLeadId == leadId) || 
                         (driverLeadId != null && c.DriverLeadId == driverLeadId));
    }
}