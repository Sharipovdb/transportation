using FluentValidation;
using Transportation.Application.CrewMembership.Repositories;
using Transportation.Application.TaxiExpense.Models;
using Transportation.Application.TaxiExpense.Repositories;
using Transportation.Application.TransportDay;
using Transportation.Application.TransportDay.Repositories;
using Transportation.Domain.Entities;
using Transportation.Mediator.Helper.Commands;
using Transportation.Mediator.Helper.Common.Extensions;
using Transportation.Mediator.Helper.Exceptions;
using Transportation.Mediator.Helper.Persistence;

namespace Transportation.Application.TaxiExpense.Commands;

public record CreateTaxiExpenseCommand(
    long TransportDayId,
    Leg Leg,
    decimal Amount,
    long PaidById,
    TaxiExpenseStatus TaxiExpenseStatus
) : ICommand<TaxiExpenseDto>;

// ReSharper disable once UnusedType.Global
public sealed class CreateTaxiExpenseCommandValidator : AbstractValidator<CreateTaxiExpenseCommand>
{
    public CreateTaxiExpenseCommandValidator()
    {
        RuleFor(x => x.TransportDayId)
            .GreaterThan(0)
            .WithMessage("The id must be greater than zero");

        RuleFor(x => x.Leg).IsInEnum();

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("The amount must be greater than zero");

        RuleFor(y => y.PaidById)
            .GreaterThan(0)
            .WithMessage("The id must be greater than zero");

        RuleFor(x => x.TaxiExpenseStatus).IsInEnum();
    }
}

internal sealed class CreateTaxiExpenseCommandHandler : ICommandHandler<CreateTaxiExpenseCommand, TaxiExpenseDto>
{
    private readonly ITaxiExpenseRepository _taxiExpenseRepository;
    private readonly ICrewMembershipRepository _crewMembershipRepository;
    private readonly ITransportDayRepository _transportDayRepository;
    private readonly TaxiExpenseMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;

    public CreateTaxiExpenseCommandHandler(
        ITaxiExpenseRepository taxiExpenseRepository,
        ICrewMembershipRepository crewMembershipRepository,
        ITransportDayRepository transportDayRepository,
        TaxiExpenseMapper expenseMapper,
        IUnitOfWork unitOfWork,
        TimeProvider timeProvider)
    {
        _taxiExpenseRepository = taxiExpenseRepository;
        _crewMembershipRepository = crewMembershipRepository;
        _transportDayRepository = transportDayRepository;
        _mapper = expenseMapper;
        _unitOfWork = unitOfWork;
        _timeProvider = timeProvider;
    }

    public async Task<TaxiExpenseDto> Handle(CreateTaxiExpenseCommand request, CancellationToken cancellationToken)
    {
        var transportDay = await _transportDayRepository.GetByIdAsync(request.TransportDayId, cancellationToken);

        if (transportDay is null || transportDay.IsDeleted)
            throw new BusinessLogicException(TransportDayErrors.NotFound);

        if (transportDay.Confirmed)
            throw new BusinessLogicException(TaxiExpenseErrors.AlreadyExists);

        var selectedLeg = request.Leg switch
        {
            Leg.Morning => transportDay.MorningMode,
            Leg.Afternoon => transportDay.AfternoonMode,
            _ => throw new BusinessLogicException(TaxiExpenseErrors.InvalidEnumValue)
        };

        var taxiExpense = new Domain.Entities.TaxiExpense
        {
            TransportDayId = request.TransportDayId,
            Leg = request.Leg,
            Amount = request.Amount,
            PaidById = request.PaidById,
            TaxiExpenseStatus = request.TaxiExpenseStatus,

            CreatedAt = _timeProvider.GetLocalDateTimeNowKindUtc()
        };

        await _taxiExpenseRepository.AddAsync(taxiExpense, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map(taxiExpense);
    }
}