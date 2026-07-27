using Ardalis.Specification;

namespace Transportation.Application.Route.Specifications;

public class RouteByNameSpec : Specification<Domain.Entities.Route>
{
    public RouteByNameSpec(string name)
    {
        Query.Where(x => x.Name == name && !x.IsDeleted);
    }
}