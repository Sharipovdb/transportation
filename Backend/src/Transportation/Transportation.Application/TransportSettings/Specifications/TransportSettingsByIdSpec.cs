using Ardalis.Specification;

namespace Transportation.Application.TransportSettings.Specifications;

public sealed class TransportSettingsByIdSpec : Specification<Domain.Entities.TransportSettings>
{
    public long Id { get; }

    public TransportSettingsByIdSpec(long id, bool asNoTracking = false)
    {
        Id = id;

        if (asNoTracking)
            Query.AsNoTracking();

        Query.Where(x =>
            x.Id == id &&
            !x.IsDeleted);
    }
}