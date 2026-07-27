using FluentValidation;
using Transportation.Application.Crew;
using Transportation.Application.Crew.Repositories;
using Transportation.Application.Crew.Specification;
using Transportation.Application.TransportDay.Models;
using Transportation.Application.TransportDay.Repositories;
using Transportation.Application.TransportDay.Specifications;
using Transportation.Mediator.Helper.Commands;
using Transportation.Mediator.Helper.Common.Extensions;
using Transportation.Mediator.Helper.Exceptions;
using Transportation.Mediator.Helper.Persistence;
using Transportation.Shared;
using Transportation.Shared.Extensions;
using Transportation.Shared.Middlewares;

namespace Transportation.Application.TransportDay.Commands;

public sealed record CreateTransportDayCommand(
    long CrewId,
    DateTime Date,
    Domain.Entities.TransportMode MorningMode,
    Domain.Entities.TransportMode? AfternoonMode,
    double? ExtraCommuteKm,
    double? ExtraBusinessCm,
    string? Notes
) : ICommand<TransportDayDto>;

// ReSharper disable once UnusedType.Global
public sealed class CreateTransportDayCommandValidator : AbstractValidator<CreateTransportDayCommand>
{
    public CreateTransportDayCommandValidator()
    {
        RuleFor(x => x.CrewId)
            .GreaterThan(0);
        RuleFor(x => x.Date)
            .NotEmpty();
        RuleFor(x => x.MorningMode)
            .IsInEnum();

        RuleFor(x => x.ExtraCommuteKm)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.ExtraBusinessCm)
            .GreaterThanOrEqualTo(0);
    }
}

internal sealed class CreateTransportDayCommandHandler :
    ICommandHandler<CreateTransportDayCommand, TransportDayDto>
{
    private readonly ITransportDayRepository _transportDayRepository;
    private readonly ICurrentUserAccessor _currentUserAccessor;
    private readonly ICrewRepository _crewRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TransportDayMapper _mapper;
    private readonly TimeProvider _timeProvider;

    public CreateTransportDayCommandHandler(
        ITransportDayRepository transportDayRepository,
        IUnitOfWork unitOfWork,
        TransportDayMapper mapper,
        TimeProvider timeProvider,
        ICrewRepository crewRepository,
        ICurrentUserAccessor currentUserAccessor)
    {
        _transportDayRepository = transportDayRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _timeProvider = timeProvider;
        _crewRepository = crewRepository;
        _currentUserAccessor = currentUserAccessor;
    }

    public async Task<TransportDayDto> Handle(
        CreateTransportDayCommand request,
        CancellationToken cancellationToken)
    {
        var spec = new TransportDayByCrewAndDateSpec(request.CrewId, request.Date);
        var exists = await _transportDayRepository.AnyAsync(spec, cancellationToken);

        if (exists)
            throw new BusinessLogicException(TransportDayErrors.AlreadyExists);

        var crewSpec = new CrewByIdSpec(request.CrewId);
        var crew = await _crewRepository.FirstOrDefaultAsync(crewSpec, cancellationToken);

        if (crew is null)
            throw new ResourceNotFoundException(CrewErrors.NotFound);

        var currentUserId = _currentUserAccessor.GetRequiredUser().GetUserId();

        var now = _timeProvider.GetLocalDateTimeNowKindUtc();

        var entity = new Domain.Entities.TransportDay
        {
            CrewId = crew.Id,
            Date = request.Date.ToUniversalTime(),
            MorningMode = request.MorningMode,
            Notes = request.Notes,
            DriverId = crew.DriverLeadId,
            BaseRouteKm = crew.Route.DistanceKm,
            LoggedBy = currentUserId,
            LoggedAt = now,
            Confirmed = false,
            CreatedAt = now
        };

        if (request.AfternoonMode.HasValue)
            entity.AfternoonMode = request.AfternoonMode;

        if (request.ExtraCommuteKm.HasValue)
            entity.ExtraCommuteKm = (double)request.ExtraCommuteKm;

        if (request.ExtraBusinessCm.HasValue)
            entity.ExtraBusinessKm = (double)request.ExtraBusinessCm;

        if (request.Notes is not null)
            entity.Notes = request.Notes;

        await _transportDayRepository.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map(entity);
    }
}