using Ardalis.Specification.EntityFrameworkCore;
using Transportation.Application.MonthlyTransportSheet.Repositories;
using Transportation.Domain.Entities;

namespace Transportation.Infrastructure.Persistence.Repositories;

internal sealed class MonthlyTransportSheetRepository : RepositoryBase<MonthlyTransportSheet>, IMonthlyTransportSheetRepository
{
    public MonthlyTransportSheetRepository(TransportationDbContext dbContext) : base(dbContext)
    {
    }
}