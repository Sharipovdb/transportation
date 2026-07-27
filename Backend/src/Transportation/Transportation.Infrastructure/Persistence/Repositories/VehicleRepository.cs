using Ardalis.Specification.EntityFrameworkCore;
using Transportation.Application.Vehicle.Repositories;
using Transportation.Domain.Entities;

namespace Transportation.Infrastructure.Persistence.Repositories;

internal sealed class VehicleRepository : RepositoryBase<Vehicle>, IVehicleRepository
{
    public VehicleRepository(TransportationDbContext dbContext) : base(dbContext)
    {
        
    }
}