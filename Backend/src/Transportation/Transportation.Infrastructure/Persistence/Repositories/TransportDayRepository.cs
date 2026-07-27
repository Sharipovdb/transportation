using Ardalis.Specification.EntityFrameworkCore;
using Transportation.Application.TransportDay.Repositories;
using Transportation.Domain.Entities;

namespace Transportation.Infrastructure.Persistence.Repositories;

internal sealed class TransportDayRepository : RepositoryBase<TransportDay>,ITransportDayRepository
{
    public TransportDayRepository(TransportationDbContext dbContext) : base(dbContext)
    {
    }
}