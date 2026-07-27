using Ardalis.Specification;
using Transportation.Application.TransportSettings.Models;
using Transportation.Application.TransportSettings.Repositories;
using Transportation.Mediator.Helper.Common.Extensions;
using Transportation.Mediator.Helper.Common.Models;
using Transportation.Mediator.Helper.Exceptions;
using Transportation.Mediator.Helper.Persistence;
using Transportation.Mediator.Helper.Queries;

namespace Transportation.Application.TransportSettings.Queries;

public sealed record GetAllTransportSettingsQuery(PaginationInfo PaginationInfo) 
    : IQuery<PaginatedResult<TransportSettingsDto>>;
    
internal sealed class GetAllTransportSettingsQueryHandler
    : IQueryHandler<GetAllTransportSettingsQuery, PaginatedResult<TransportSettingsDto>>
{
    private readonly ITransportSettingsRepository _transportSettingsRepository;
    private readonly TransportSettingsMapper _mapper;

    public GetAllTransportSettingsQueryHandler(
        ITransportSettingsRepository transportSettingsRepository,
        TransportSettingsMapper mapper)
    {
        _transportSettingsRepository = transportSettingsRepository;
        _mapper = mapper;
    }

    public async Task<PaginatedResult<TransportSettingsDto>> Handle(
        GetAllTransportSettingsQuery request,
        CancellationToken cancellationToken)
    {
        var spec = new ReadOnlySpecification<Domain.Entities.TransportSettings>();

        spec.Query
            .Where(x => !x.IsDeleted)
            .OrderByDescending(x => x.EffectiveFrom)
            .WithPagination(request.PaginationInfo);

        var transportSettings = await _transportSettingsRepository.ListAsync(spec, cancellationToken);

        var totalCount = await _transportSettingsRepository.CountAsync(spec, cancellationToken);

        if (totalCount == 0)
            throw new ResourceNotFoundException(TransportSettingsErrors.NotFound);

        var mapped = _mapper.Map(transportSettings);

        return new PaginatedResult<TransportSettingsDto>(mapped, totalCount);
    }
}