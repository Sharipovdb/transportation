using FluentValidation;
using Transportation.Application.Crew;
using Transportation.Application.Crew.Repositories;
using Transportation.Application.Crew.Specification;
using Transportation.Application.TransportDay.Models;
using Transportation.Application.TransportDay.Repositories;
using Transportation.Application.TransportDay.Services;
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
    DateOnly Date,
    Domain.Entities.TransportMode MorningMode,
    Domain.Entities.TransportMode? AfternoonMode,
    double? ExtraCommuteKm,
    double? ExtraBusinessKm,
    string? Notes,
    IReadOnlyList<TransportDayTaxiFare>? TaxiFares
) : ICommand<TransportDayDto>;


public sealed class CreateTransportDayCommandValidator : AbstractValidator<CreateTransportDayCommand>
{
    public CreateTransportDayCommandValidator()
    {
        RuleFor(x => x.CrewId)
            .GreaterThan(0);
        RuleFor(x => x.Date)
            .NotEqual(default(DateOnly))
            .WithMessage("Date is required.");
        RuleFor(x => x.MorningMode)
            .IsInEnum();

        RuleFor(x => x.ExtraCommuteKm)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.ExtraBusinessKm)
            .GreaterThanOrEqualTo(0);

        RuleForEach(x => x.TaxiFares)
            .ChildRules(fare =>
            {
                fare.RuleFor(x => x.Leg).IsInEnum();
                fare.RuleFor(x => x.Amount).GreaterThan(0);
                fare.RuleFor(x => x.PaidById).GreaterThan(0);
            });
    }
}

internal sealed class CreateTransportDayCommandHandler :
    ICommandHandler<CreateTransportDayCommand, TransportDayDto>
{
    private readonly ITransportDayRepository _transportDayRepository;
    private readonly ICurrentUserAccessor _currentUserAccessor;
    private readonly ITransportDayTaxiFareService _taxiFareService;
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
        ITransportDayTaxiFareService taxiFareService,
        ICurrentUserAccessor currentUserAccessor)
    {
        _transportDayRepository = transportDayRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _timeProvider = timeProvider;
        _crewRepository = crewRepository;
        _taxiFareService = taxiFareService;
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

        // The route length and the crew's lead are copied onto the day rather than read
        // through the crew later: both can change, and a logged day has to keep saying
        // what it was worth on the date it happened. The lead is the driver-lead when
        // the crew has one and the manager-lead otherwise — a manager-led crew that
        // drove used to land here with no driver at all, which silently dropped its
        // kilometres from the monthly sheet.
        var entity = new Domain.Entities.TransportDay
        {
            CrewId = crew.Id,
            Date = request.Date,
            MorningMode = request.MorningMode,
            AfternoonMode = request.AfternoonMode,
            BaseRouteKm = crew.Route.DistanceKm,
            ExtraCommuteKm = request.ExtraCommuteKm ?? 0,
            ExtraBusinessKm = request.ExtraBusinessKm ?? 0,
            DriverId = crew.DriverLeadId ?? crew.CrewLeadId,
            Notes = request.Notes ?? string.Empty,
            LoggedBy = currentUserId,
            LoggedAt = now,
            CreatedAt = now
        };

        await _taxiFareService.SyncAsync(entity, request.TaxiFares ?? [], now, cancellationToken);

        await _transportDayRepository.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map(entity);
    }
}
