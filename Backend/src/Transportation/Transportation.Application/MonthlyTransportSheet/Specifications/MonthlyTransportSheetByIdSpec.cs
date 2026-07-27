using Ardalis.Specification;

namespace Transportation.Application.MonthlyTransportSheet.Specifications;

public sealed class MonthlyTransportSheetByIdSpec
    : Specification<Domain.Entities.MonthlyTransportSheet>
{
    public MonthlyTransportSheetByIdSpec(long id, bool asNoTracking = false)
    {
        if (asNoTracking)
            Query.AsNoTracking();

        Query.Where(x =>
                x.Id == id && !x.IsDeleted)

            .Include(x => x.PayoutLines)
            .ThenInclude(x => x.User)

            .Include(x => x.PayoutLines)
            .ThenInclude(x => x.TaxiExpenses)
            .ThenInclude(x => x.TaxiExpense)
            .ThenInclude(x => x.PaidBy);
    }
}