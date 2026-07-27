using Ardalis.Specification;
using Transportation.Domain.Entities;

namespace Transportation.Application.AbsenceNotice.Specifications;

public sealed class AbsenceNoticeByUserDateSpec : Specification<Domain.Entities.AbsenceNotice>
{
    public AbsenceNoticeByUserDateSpec(long userId, DateOnly date, AbsenceScope scope, bool asNoTracking = false)
    {
        if (asNoTracking)
            Query.AsNoTracking();

        Query.Where(x =>
            x.UserId == userId &&
            x.Date == date &&
            x.Scope == scope &&
            !x.IsDeleted);
    }
}