using Ardalis.Specification;
using Transportation.Application.TransportDay.Models;
using Transportation.Application.TransportDay.Repositories;
using Transportation.Mediator.Helper.Common.Extensions;
using Transportation.Mediator.Helper.Common.Models;
using Transportation.Mediator.Helper.Persistence;
using Transportation.Mediator.Helper.Queries;

namespace Transportation.Application.TransportDay.Queries;

public sealed record GetAllTransportDays(
    long? CrewId,
    DateOnly? DateFrom,
    DateOnly? DateTo,
    PaginationInfo PaginationInfo
) : IQuery<PaginatedResult<TransportDayDto>>;

internal sealed class GetAllTransportDaysHandler : IQueryHandler<GetAllTransportDays, PaginatedResult<TransportDayDto>>
{
    private readonly ITransportDayRepository _transportDayRepository;
    private readonly TransportDayMapper _mapper;

    public GetAllTransportDaysHandler(ITransportDayRepository transportDayRepository, TransportDayMapper mapper)
    {
        _transportDayRepository = transportDayRepository;
        _mapper = mapper;
    }

    public async Task<PaginatedResult<TransportDayDto>> Handle(
        GetAllTransportDays request, CancellationToken cancellationToken)
    {
        // No implicit date floor: a query with no filters returns every logged day.
        // Defaulting to "this month onwards" made the month picker show nothing for any
        // past period, and hid the day behind a taxi expense whenever the two were on
        // opposite sides of that boundary.
        var spec = new ReadOnlySpecification<Domain.Entities.TransportDay>();

        if (request.CrewId.HasValue)
            spec.Query.Where(x => x.CrewId == request.CrewId);

        if (request.DateFrom.HasValue)
            spec.Query.Where(x => x.Date >= request.DateFrom.Value);

        if (request.DateTo.HasValue)
            spec.Query.Where(x => x.Date <= request.DateTo.Value);

        spec.Query
            .Where(x => !x.IsDeleted)
            .OrderByDescending(x => x.Date)
            .Include(x => x.TaxiExpenses)
            .ThenInclude(x => x.PaidBy)
            .WithPagination(request.PaginationInfo);

        var items = await _transportDayRepository.ListAsync(spec, cancellationToken);
        var totalCount = await _transportDayRepository.CountAsync(spec, cancellationToken);

        var mapped = _mapper.Map(items);
        return new PaginatedResult<TransportDayDto>(mapped, totalCount);
    }
}