using Ardalis.Specification;
using Transportation.Application.PayoutLine.Models;
using Transportation.Application.PayoutLine.Repositories;
using Transportation.Mediator.Helper.Common.Extensions;
using Transportation.Mediator.Helper.Common.Models;
using Transportation.Mediator.Helper.Persistence;
using Transportation.Mediator.Helper.Queries;

namespace Transportation.Application.PayoutLine.Queries;

public sealed record GetAllPayoutLines(
    long? MonthlyTransportSheetId,
    long? UserId,
    int PageIndex,
    int PageSize
) : IQuery<PaginatedResult<PayoutLineDto>>;

internal sealed class GetAllPayoutLineHandler : IQueryHandler<GetAllPayoutLines, PaginatedResult<PayoutLineDto>>
{
    private readonly IPayoutLineRepository _payoutLineRepository;
    private readonly PayoutLineMapper _mapper;

    public GetAllPayoutLineHandler(IPayoutLineRepository payoutLineRepository, PayoutLineMapper mapper)
    {
        _payoutLineRepository = payoutLineRepository;
        _mapper = mapper;
    }

    public async Task<PaginatedResult<PayoutLineDto>> Handle(GetAllPayoutLines request,
        CancellationToken cancellationToken)
    {
        var spec = new ReadOnlySpecification<Domain.Entities.PayoutLine>();
        spec.Query.Where(x => !x.IsDeleted);

        if (request.MonthlyTransportSheetId.HasValue)
            spec.Query.Where(x => x.MonthlyTransportSheetId == request.MonthlyTransportSheetId);

        if (request.UserId.HasValue)
            spec.Query.Where(x => x.UserId == request.UserId);

        spec.Query
            .OrderByDescending(x => x.Id)
            .WithPagination(new PaginationInfo(request.PageIndex, request.PageSize));

        var items = await _payoutLineRepository.ListAsync(spec, cancellationToken);
        var totalCount = await _payoutLineRepository.CountAsync(spec, cancellationToken);

        var mapped = _mapper.Map(items);

        return new PaginatedResult<PayoutLineDto>(mapped, totalCount);
    }
}