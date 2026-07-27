using Ardalis.Specification.EntityFrameworkCore;
using Transportation.Application.TransportSettings.Repositories;
using Transportation.Domain.Entities;

namespace Transportation.Infrastructure.Persistence.Repositories;

internal sealed class TransportSettingsRepository : RepositoryBase<TransportSettings>, ITransportSettingsRepository
{
    public TransportSettingsRepository(TransportationDbContext dbContext) : base(dbContext)
    {
    }
}