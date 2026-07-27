using Ardalis.Specification;

namespace Transportation.Application.MonthlyTransportSheet.Specifications;

public sealed class MonthlyTransportSheetByPeriodSpec : Specification<Domain.Entities.MonthlyTransportSheet>
{
    public long CrewId { get; }
    public int Year { get; }
    public int Month { get; }

    public MonthlyTransportSheetByPeriodSpec(long crewId, int year, int month, bool asNoTracking = false)
    {
        CrewId = crewId;
        Year = year;
        Month = month;

        if (asNoTracking)
            Query.AsNoTracking();

        Query.Where(x =>
            x.CrewId == crewId &&
            x.Year == year &&
            x.Month == month &&
            !x.IsDeleted);
    }
}