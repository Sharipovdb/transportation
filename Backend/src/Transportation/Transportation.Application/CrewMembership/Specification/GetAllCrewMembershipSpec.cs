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
        PaginationInfo? paginationInfo = null)
    {
        Query.AsNoTracking(); 

        Query.Where(x => !x.IsDeleted && x.IsActive);

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