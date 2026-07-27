using Ardalis.Specification.EntityFrameworkCore;
using Transportation.Application.Crew.Repositories;
using Transportation.Domain.Entities;

namespace Transportation.Infrastructure.Persistence.Repositories;

internal sealed class CrewRepository : RepositoryBase<Crew>, ICrewRepository 
{
    public CrewRepository(TransportationDbContext dbcontext) : base(dbcontext)
    {
        
    }
    
}