using Ardalis.Specification;

namespace Transportation.Application.TransportSettings.Specifications;

public sealed class TransportSettingsByEffectiveDateSpec : Specification<Domain.Entities.TransportSettings>
{
    public TransportSettingsByEffectiveDateSpec(DateTime effectiveFrom, bool asNoTracking = false)
    {
        if (asNoTracking)
            Query.AsNoTracking();

        Query.Where(x =>
            !x.IsDeleted &&
            x.EffectiveFrom.Date == effectiveFrom.Date);
    }
}