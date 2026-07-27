using Ardalis.Specification.EntityFrameworkCore;
using Transportation.Application.PayoutLine.Repositories;
using Transportation.Domain.Entities;

namespace Transportation.Infrastructure.Persistence.Repositories;

internal class PayoutLineRepository : RepositoryBase<PayoutLine>, IPayoutLineRepository
{
    public PayoutLineRepository(TransportationDbContext dbContext) : base(dbContext)
    {
    }
}