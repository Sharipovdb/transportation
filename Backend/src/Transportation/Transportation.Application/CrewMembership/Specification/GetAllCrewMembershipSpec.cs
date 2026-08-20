using Ardalis.Specification;
using Transportation.Mediator.Helper.Common.Extensions;
using Transportation.Mediator.Helper.Common.Models;

namespace Transportation.Application.CrewMembership.Specification;

public sealed class GetAllCrewMembershipSpec : Specification<Domain.Entities.CrewMembership>
{
    public GetAllCrewMembershipSpec(
        long? crewId,
        long? userId,
        DateTime? activeFrom,
        DateTime? activeTo,
        PaginationInfo? paginationInfo = null,
        long[]? visibleCrewIds = null)
    {
        Query.AsNoTracking(); 

        Query.Where(x => !x.IsDeleted && x.IsActive);

        // Null means the caller may read every crew; an empty array means they lead none
        // and so see nothing. Both are answered by ICrewVisibility, never re-derived here.
        if (visibleCrewIds is not null)
            Query.Where(x => visibleCrewIds.Contains(x.CrewId));

        if (crewId.HasValue)
            Query.Where(x => x.CrewId == crewId.Value);

        if (userId.HasValue)
            Query.Where(x => x.UserId == userId.Value);

        if (activeFrom.HasValue)
            Query.Where(x => x.ActiveFrom >= activeFrom.Value);

        if (activeTo.HasValue)
            Query.Where(x => x.ActiveTo <= activeTo.Value);

        Query.OrderBy(x => x.Id);

        Query.Include(x => x.User)
            .Include(x => x.Crew);

        if (paginationInfo is not null)
        {
            Query.WithPagination(paginationInfo);
        }
    }
}