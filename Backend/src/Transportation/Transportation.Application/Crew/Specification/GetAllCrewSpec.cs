using Ardalis.Specification;
using Transportation.Mediator.Helper.Common.Extensions;
using Transportation.Mediator.Helper.Common.Models;

namespace Transportation.Application.Crew.Specification;

public sealed class GetAllCrewSpec : Specification<Domain.Entities.Crew>
{
    public GetAllCrewSpec(
        string? name,
        long? routeId,
        long? crewLeadId,
        long? driverLeadId,
        int? seatCapacity,
        PaginationInfo? paginationInfo = null,
        long[]? visibleCrewIds = null)
    {
        Query.AsNoTracking();

        Query.Where(x => !x.IsDeleted);

        // Null means the caller may read every crew; an empty array means they lead none
        // and so see nothing. Both are answered by ICrewVisibility, never re-derived here.
        if (visibleCrewIds is not null)
            Query.Where(x => visibleCrewIds.Contains(x.Id));

        if (!string.IsNullOrWhiteSpace(name))
            Query.Where(x => x.Name.Contains(name));

        if (routeId.HasValue)
            Query.Where(x => x.RouteId == routeId);

        if (crewLeadId.HasValue)
            Query.Where(x => x.CrewLeadId == crewLeadId);

        if (driverLeadId.HasValue)
            Query.Where(x => x.DriverLeadId == driverLeadId);

        if (seatCapacity.HasValue)
            Query.Where(x => x.SeatCapacity == seatCapacity);

        Query.OrderBy(x => x.Name);

        Query.Include(x => x.Route)
            .Include(x => x.CrewLead)
            .Include(x => x.DriverLead);

        if (paginationInfo is not null)
            Query.WithPagination(paginationInfo);
    }
}