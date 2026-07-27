using Ardalis.Specification;

namespace Transportation.Application.Route.Specifications;

public class RouteByIdSpec : Specification<Domain.Entities.Route>
{
    public RouteByIdSpec(long id)
    {
        Query.Where(x => x.Id == id && !x.IsDeleted);
    }
}