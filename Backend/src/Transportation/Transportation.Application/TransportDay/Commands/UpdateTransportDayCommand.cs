using FluentValidation;
using Transportation.Application.TransportDay.Models;
using Transportation.Application.TransportDay.Repositories;
using Transportation.Application.TransportDay.Services;
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
    double? ExtraCommuteKm,
    double? ExtraBusinessKm,
    string? Notes,
    IReadOnlyList<TransportDayTaxiFare>? TaxiFares
) : ICommand<TransportDayDto>;

// ReSharper disable once UnusedType.Global
public sealed class UpdateTransportDayCommandValidator : AbstractValidator<UpdateTransportDayCommand>
{
    public UpdateTransportDayCommandValidator()
    {
        RuleFor(x => x.TransportDayId).GreaterThan(0);

        RuleFor(x => x.ExtraCommuteKm).GreaterThanOrEqualTo(0);

        RuleFor(x => x.ExtraBusinessKm).GreaterThanOrEqualTo(0);

        RuleForEach(x => x.TaxiFares)
            .ChildRules(fare =>
            {
                fare.RuleFor(x => x.Leg).IsInEnum();
                fare.RuleFor(x => x.Amount).GreaterThan(0);
                fare.RuleFor(x => x.PaidById).GreaterThan(0);
            });
    }
}

internal sealed class UpdateTransportDayCommandHandler
    : ICommandHandler<UpdateTransportDayCommand, TransportDayDto>
{
    private readonly ITransportDayRepository _transportDayRepository;
    private readonly ITransportDayTaxiFareService _taxiFareService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TransportDayMapper _mapper;
    private readonly TimeProvider _timeProvider;

    public UpdateTransportDayCommandHandler(
        ITransportDayRepository transportDayRepository,
        IUnitOfWork unitOfWork,
        TransportDayMapper mapper,
        ITransportDayTaxiFareService taxiFareService,
        TimeProvider timeProvider)
    {
        _transportDayRepository = transportDayRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _taxiFareService = taxiFareService;
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

        // A day stays editable for as long as its money is still open: the fare service
        // refuses to rewrite an expense that has already been ruled on, which is what
        // stops an edit from moving money that was signed off.
        if (request.MorningMode.HasValue)
            entity.MorningMode = request.MorningMode.Value;

        if (request.AfternoonMode.HasValue)
            entity.AfternoonMode = request.AfternoonMode.Value;

        if (request.ExtraCommuteKm.HasValue)
            entity.ExtraCommuteKm = request.ExtraCommuteKm.Value;

        if (request.ExtraBusinessKm.HasValue)
            entity.ExtraBusinessKm = request.ExtraBusinessKm.Value;

        if (request.Notes is not null)
            entity.Notes = request.Notes;

        var now = _timeProvider.GetLocalDateTimeNowKindUtc();

        await _taxiFareService.SyncAsync(entity, request.TaxiFares ?? [], now, cancellationToken);

        entity.UpdatedAt = now;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map(entity);
    }
}
