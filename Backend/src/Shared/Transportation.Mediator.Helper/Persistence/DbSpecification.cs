using Ardalis.Specification;

namespace Transportation.Mediator.Helper.Persistence;

public class ReadOnlySpecification<T> : Specification<T>
    where T : class
{
    public ReadOnlySpecification()
    {
        Query.AsNoTracking();
    }
}

public class ReadOnlySpecification<T, TResult> : Specification<T, TResult>
    where T : class
{
    public ReadOnlySpecification()
    {
        Query.AsNoTracking();
    }
}

public class DbSpecification<T> : Specification<T>
    where T : class;

public class DbSpecification<T, TResult> : Specification<T, TResult>
    where T : class;