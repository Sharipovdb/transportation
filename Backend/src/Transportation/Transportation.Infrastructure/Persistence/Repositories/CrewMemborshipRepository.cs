using Ardalis.Specification.EntityFrameworkCore;
using Transportation.Application.CrewMembership.Repositories;
using Transportation.Domain.Entities;

namespace Transportation.Infrastructure.Persistence.Repositories;

internal sealed class CrewMemborshipRepository : RepositoryBase<CrewMembership>,ICrewMembershipRepository
{
    public CrewMemborshipRepository(TransportationDbContext dbContext) : base(dbContext)
    {
        
    }
}