using Ardalis.Specification;
using Transportation.Domain.Entities;

namespace Transportation.Application.TaxiExpense.Specifications;

public class TaxiExpenseByTransportDayIdSpec : Specification<Domain.Entities.TaxiExpense>
{
    public TaxiExpenseByTransportDayIdSpec(long transportDayId, Leg leg, bool asNoTracking = false)
    {
        if (asNoTracking)
            Query.AsNoTracking();

        Query.Where(x => x.TransportDayId == transportDayId && x.Leg == leg && !x.IsDeleted);
    }
}