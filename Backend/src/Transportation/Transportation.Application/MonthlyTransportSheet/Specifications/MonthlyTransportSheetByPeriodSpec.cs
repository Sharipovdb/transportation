using Ardalis.Specification;

namespace Transportation.Application.MonthlyTransportSheet.Specifications;

public sealed class MonthlyTransportSheetByPeriodSpec : Specification<Domain.Entities.MonthlyTransportSheet>
{
    public MonthlyTransportSheetByPeriodSpec(long crewId, int year, int month, bool asNoTracking = false)
    {
        if (asNoTracking)
            Query.AsNoTracking();

        Query.Where(x =>
                x.CrewId == crewId &&
                x.Year == year &&
                x.Month == month &&
                !x.IsDeleted)
            .Include(x => x.Recipient)
            .Include(x => x.Days);
    }
}
