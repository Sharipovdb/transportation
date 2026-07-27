using Transportation.Application.TransportDay.Models;
using Transportation.Application.TransportDay.Repositories;
using Transportation.Application.TransportDay.Specifications;
using Transportation.Domain.Entities;
using Transportation.Mediator.Helper.Commands;
using Transportation.Mediator.Helper.Common.Extensions;
using Transportation.Mediator.Helper.Exceptions;
using Transportation.Mediator.Helper.Persistence;

namespace Transportation.Application.TransportDay.Commands;

public sealed record UpdateTransportDayCommand(
    long TransportDayId,
    TransportMode? MorningMode,
    TransportMode? AfternoonMode,
    double? CommuteKm,
    double? ExtraBusinessKm,
    string? Notes
) : ICommand<TransportDayDto>;

internal sealed class UpdateTransportDayCommandHandler
    : ICommandHandler<UpdateTransportDayCommand, TransportDayDto>
{
    private readonly ITransportDayRepository _transportDayRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TransportDayMapper _mapper;
    private readonly TimeProvider _timeProvider;

    public UpdateTransportDayCommandHandler(
        ITransportDayRepository transportDayRepository,
        IUnitOfWork unitOfWork,
        TransportDayMapper mapper,
        TimeProvider timeProvider)
    {
        _transportDayRepository = transportDayRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _timeProvider = timeProvider;
    }

    public async Task<TransportDayDto> Handle(
        UpdateTransportDayCommand request,
        CancellationToken cancellationToken)
    {
        var spec = new TransportDayByIdSpec(request.TransportDayId);
        var entity = await _transportDayRepository.FirstOrDefaultAsync(spec, cancellationToken);

        if (entity is null)
            throw new ResourceNotFoundException(TransportDayErrors.NotFound);
        
        if (request.MorningMode.HasValue)
            entity.MorningMode = (TransportMode)request.MorningMode;

        if (request.AfternoonMode.HasValue)
            entity.AfternoonMode = (TransportMode)request.AfternoonMode;

        if (request.CommuteKm.HasValue)
            entity.ExtraCommuteKm = (double)request.CommuteKm;

        if (request.ExtraBusinessKm.HasValue)
            entity.ExtraBusinessKm = (double)request.ExtraBusinessKm;

        if (request.Notes is not null)
            entity.Notes = request.Notes;

        entity.UpdatedAt = _timeProvider.GetLocalDateTimeNowKindUtc();

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map(entity);
    }
}