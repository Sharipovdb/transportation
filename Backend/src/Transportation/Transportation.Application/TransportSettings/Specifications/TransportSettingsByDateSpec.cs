using Ardalis.Specification;

namespace Transportation.Application.TransportSettings.Specifications;

public sealed class TransportSettingsByDateSpec : Specification<Domain.Entities.TransportSettings>
{
    public DateTime Date { get; }

    public TransportSettingsByDateSpec(DateTime date, bool asNoTracking = false)
    {
        Date = date;

        if (asNoTracking)
            Query.AsNoTracking();

        Query.Where(x =>
                !x.IsDeleted &&
                x.EffectiveFrom <= date)
            .OrderByDescending(x => x.EffectiveFrom)
            .Take(1);
    }
}