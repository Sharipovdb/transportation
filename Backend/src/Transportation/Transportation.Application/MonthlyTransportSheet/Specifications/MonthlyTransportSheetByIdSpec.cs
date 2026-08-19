using Ardalis.Specification;

namespace Transportation.Application.MonthlyTransportSheet.Specifications;

/// <summary>
/// A sheet with everything the confirm/pay commands touch: its days, and the taxi
/// expenses behind them whose status paying the sheet moves on to Paid.
/// </summary>
public sealed class MonthlyTransportSheetByIdSpec
    : Specification<Domain.Entities.MonthlyTransportSheet>
{
    public MonthlyTransportSheetByIdSpec(long id, bool asNoTracking = false)
    {
        if (asNoTracking)
            Query.AsNoTracking();

        Query.Where(x => x.Id == id && !x.IsDeleted)
            .Include(x => x.Recipient);

        Query.Include(x => x.Days)
            .ThenInclude(x => x.TransportDay)
            .ThenInclude(x => x.TaxiExpenses);
    }
}
