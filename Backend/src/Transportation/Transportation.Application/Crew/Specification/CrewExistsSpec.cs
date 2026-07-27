using Ardalis.Specification;

namespace Transportation.Application.Crew.Specification;

public sealed class CrewExistsSpec : Specification<Transportation.Domain.Entities.Crew>
{
    public string Name { get; set; }
    public long? LeadId { get; set; }
    public long? DriverLeadId { get; set; }
    public long RouteId { get; set; }

    public CrewExistsSpec(string name, long? leadId, long? driverLeadId, long routeId, bool asNoTracking = false)
    {
        if (asNoTracking)
            Query.AsNoTracking();

        Name = name;
        LeadId = leadId;
        DriverLeadId = driverLeadId;
        RouteId = routeId;

        Query.Where(c =>
            c.Name == name &&
            c.CrewLeadId == leadId &&
            c.DriverLeadId == driverLeadId &&
            c.RouteId == routeId);
    }
}