using Ardalis.Specification;

namespace Transportation.Application.TransportDay.Specifications;

public sealed class TransportDayByIdSpec : Specification<Domain.Entities.TransportDay>
{
    public long Id { get; set; }

    public TransportDayByIdSpec(long id, bool asNoTracking = false)
    {
        Id = id;

        if (asNoTracking)
            Query.AsNoTracking();

        Query.Where(d => d.Id == id && !d.IsDeleted)
            .Include(x => x.TaxiExpenses);
    }
}