using Ardalis.Specification;
using AbsenceNoticeEntity = Transportation.Domain.Entities.AbsenceNotice;

namespace Transportation.Application.AbsenceNotice.Specifications;

public sealed class AbsenceNoticeByIdSpec : Specification<AbsenceNoticeEntity>
{
    public long Id { get; set; }

    public AbsenceNoticeByIdSpec(long id, bool asNoTracking = false)
    {
        Id = id;

        if (asNoTracking)
            Query.AsNoTracking();

        Query.Where(x => x.Id == id && !x.IsDeleted);
    }
}