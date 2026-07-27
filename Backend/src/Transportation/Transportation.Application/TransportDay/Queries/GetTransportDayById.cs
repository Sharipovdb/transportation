using Transportation.Application.TransportDay.Models;
using Transportation.Application.TransportDay.Repositories;
using Transportation.Application.TransportDay.Specifications;
using Transportation.Mediator.Helper.Exceptions;
using Transportation.Mediator.Helper.Queries;

namespace Transportation.Application.TransportDay.Queries;

public sealed record GetTransportDayById(long Id) : IQuery<TransportDayDto>;

internal sealed class GetTransportDayByIdHandler : IQueryHandler<GetTransportDayById, TransportDayDto>
{
    private readonly ITransportDayRepository _repository;
    private readonly TransportDayMapper _mapper;

    public GetTransportDayByIdHandler(
        ITransportDayRepository repository,
        TransportDayMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<TransportDayDto> Handle(
        GetTransportDayById request,
        CancellationToken cancellationToken)
    {
        var spec = new TransportDayByIdSpec(request.Id);

        var entity = await _repository.FirstOrDefaultAsync(spec, cancellationToken);

        if (entity is null)
            throw new BusinessLogicException(TransportDayErrors.NotFound);

        return _mapper.Map(entity);
    }
}