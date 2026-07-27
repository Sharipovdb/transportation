using Ardalis.Specification;

namespace Transportation.Application.MonthlyTransportSheet.Specifications;

public sealed class MonthlyTransportSheetWithPayoutsSpec
    : Specification<Domain.Entities.MonthlyTransportSheet>
{
    public MonthlyTransportSheetWithPayoutsSpec(long id, bool asNoTracking = false)
    {
        if (asNoTracking)
            Query.AsNoTracking();

        Query
            .Where(x => x.Id == id && !x.IsDeleted)
            .Include(x => x.PayoutLines)
                .ThenInclude(x => x.User);
        
        Query
            .Include(x => x.PayoutLines)
                .ThenInclude(x => x.TaxiExpenses)
                    .ThenInclude(x => x.TaxiExpense);
    }
}