using Ardalis.Specification;
using Transportation.Application.MonthlyTransportSheet.Models;
using Transportation.Application.MonthlyTransportSheet.Repositories;
using Transportation.Mediator.Helper.Common.Extensions;
using Transportation.Mediator.Helper.Common.Models;
using Transportation.Mediator.Helper.Persistence;
using Transportation.Mediator.Helper.Queries;

namespace Transportation.Application.MonthlyTransportSheet.Queries;

public sealed record GetAllMonthlyTransportSheets(
    long? CrewId,
    int? Year,
    int? Month,
    PaginationInfo PaginationInfo
) : IQuery<PaginatedResult<MonthlyTransportSheetDto>>;

internal sealed class GetAllMonthlyTransportSheetsHandler
    : IQueryHandler<GetAllMonthlyTransportSheets, PaginatedResult<MonthlyTransportSheetDto>>
{
    private readonly IMonthlyTransportSheetRepository _monthlyTransportSheetRepository;
    private readonly Mappers.MonthlyTransportSheetMapper _mapper;

    public GetAllMonthlyTransportSheetsHandler(
        IMonthlyTransportSheetRepository monthlyTransportSheetRepository,
        Mappers.MonthlyTransportSheetMapper mapper)
    {
        _monthlyTransportSheetRepository = monthlyTransportSheetRepository;
        _mapper = mapper;
    }

    public async Task<PaginatedResult<MonthlyTransportSheetDto>> Handle(
        GetAllMonthlyTransportSheets request,
        CancellationToken cancellationToken)
    {
        var spec = new ReadOnlySpecification<Domain.Entities.MonthlyTransportSheet>();

        if (request.CrewId.HasValue)
            spec.Query.Where(x => x.CrewId == request.CrewId);

        if (request.Month.HasValue)
            spec.Query.Where(x => x.Month == request.Month);

        if (request.Year.HasValue)
            spec.Query.Where(x => x.Year == request.Year);

        spec.Query
            .Where(x => !x.IsDeleted)
            .OrderByDescending(x => x.Month)
            .WithPagination(request.PaginationInfo);

        spec.Query.Include(x => x.PayoutLines)
            .ThenInclude(x => x.User);

        spec.Query.Include(x => x.PayoutLines)
            .ThenInclude(x => x.TaxiExpenses)
            .ThenInclude(x => x.TaxiExpense)
            .ThenInclude(x => x.PaidBy);

        var items = await _monthlyTransportSheetRepository.ListAsync(spec, cancellationToken);
        var totalCount = await _monthlyTransportSheetRepository.CountAsync(spec, cancellationToken);

        var mapped = _mapper.Map(items);

        return new PaginatedResult<MonthlyTransportSheetDto>(mapped, totalCount);
    }
}