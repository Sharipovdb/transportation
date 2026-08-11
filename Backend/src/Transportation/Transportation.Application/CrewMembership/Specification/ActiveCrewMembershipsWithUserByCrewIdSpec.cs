using Ardalis.Specification;

namespace Transportation.Application.CrewMembership.Specification;

/// <summary>
/// Active memberships of a crew with their <see cref="Domain.Entities.User"/> loaded.
/// The monthly sheet needs the person behind each membership so that every crew member
/// gets a payout line, including those who neither drove nor paid for a taxi.
/// </summary>
public sealed class ActiveCrewMembershipsWithUserByCrewIdSpec : Specification<Domain.Entities.CrewMembership>
{
    public ActiveCrewMembershipsWithUserByCrewIdSpec(long crewId, bool asNoTracking = false)
    {
        if (asNoTracking)
            Query.AsNoTracking();

        Query.Where(x => x.CrewId == crewId && x.IsActive && !x.IsDeleted);

        Query.Include(x => x.User);
    }
}
