using Transportation.Application.MonthlyTransportSheet.Models;
using Transportation.Application.MonthlyTransportSheet.Repositories;
using Transportation.Application.MonthlyTransportSheet.Specifications;
using Transportation.Mediator.Helper.Exceptions;
using Transportation.Mediator.Helper.Queries;

namespace Transportation.Application.MonthlyTransportSheet.Queries;

public sealed record GetMonthlyTransportSheetById(long Id)
    : IQuery<MonthlyTransportSheetDto>;

internal sealed class GetMonthlyTransportSheetByIdHandler
    : IQueryHandler<GetMonthlyTransportSheetById, MonthlyTransportSheetDto>
{
    private readonly IMonthlyTransportSheetRepository _monthlyTransportSheetRepository;
    private readonly Mappers.MonthlyTransportSheetMapper _mapper;

    public GetMonthlyTransportSheetByIdHandler(
        IMonthlyTransportSheetRepository monthlyTransportSheetRepository,
        Mappers.MonthlyTransportSheetMapper mapper)
    {
        _monthlyTransportSheetRepository = monthlyTransportSheetRepository;
        _mapper = mapper;
    }

    public async Task<MonthlyTransportSheetDto> Handle(
        GetMonthlyTransportSheetById request,
        CancellationToken cancellationToken)
    {
        var spec = new MonthlyTransportSheetByIdSpec(request.Id);
        var entity = await _monthlyTransportSheetRepository.FirstOrDefaultAsync(spec, cancellationToken);

        if (entity is null)
            throw new ResourceNotFoundException(MonthlyTransportSheetErrors.NotFound);

        return _mapper.Map(entity);
    }
}