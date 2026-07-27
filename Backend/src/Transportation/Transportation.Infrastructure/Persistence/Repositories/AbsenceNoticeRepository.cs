using Ardalis.Specification.EntityFrameworkCore;
using Transportation.Application.AbsenceNotice.Repositories;
using Transportation.Domain.Entities;

namespace Transportation.Infrastructure.Persistence.Repositories;

internal sealed class AbsenceNoticeRepository : RepositoryBase<AbsenceNotice>, IAbsenceNoticeRepository
{
    public AbsenceNoticeRepository(TransportationDbContext dbContext) : base(dbContext)
    {
    }
}