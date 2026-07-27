using Ardalis.Specification.EntityFrameworkCore;
using Transportation.Application.Route.Repositories;
using Transportation.Domain.Entities;

namespace Transportation.Infrastructure.Persistence.Repositories;

internal sealed class RouteRepository  : RepositoryBase<Route>, IRouteRepository
{
    public RouteRepository(TransportationDbContext dbContext) : base(dbContext)
    {
    }
}