using Transportation.Application.TransportSettings.Models;
using Transportation.Application.TransportSettings.Repositories;
using Transportation.Application.TransportSettings.Specifications;
using Transportation.Mediator.Helper.Exceptions;
using Transportation.Mediator.Helper.Queries;

namespace Transportation.Application.TransportSettings.Queries;

public sealed record GetTransportSettingsByIdQuery(long Id) : IQuery<TransportSettingsDto>;

internal sealed class GetTransportSettingsByIdQueryHandler
    : IQueryHandler<GetTransportSettingsByIdQuery, TransportSettingsDto>
{
    private readonly ITransportSettingsRepository _repository;
    private readonly TransportSettingsMapper _mapper;

    public GetTransportSettingsByIdQueryHandler(
        ITransportSettingsRepository repository,
        TransportSettingsMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<TransportSettingsDto> Handle(
        GetTransportSettingsByIdQuery request,
        CancellationToken cancellationToken)
    {
        var entity = await _repository.FirstOrDefaultAsync(
            new TransportSettingsByIdSpec(request.Id, true), cancellationToken);

        if (entity is null)
            throw new ResourceNotFoundException(TransportSettingsErrors.NotFound);

        return _mapper.Map(entity);
    }
}