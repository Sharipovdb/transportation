using FluentValidation;
using Transportation.Application.CrewMembership.Repositories;
using Transportation.Application.CrewMembership.Specification;
using Transportation.Application.TaxiExpense.Models;
using Transportation.Application.TaxiExpense.Repositories;
using Transportation.Application.TaxiExpense.Specifications;
using Transportation.Application.TransportDay;
using Transportation.Application.TransportDay.Repositories;
using Transportation.Application.TransportDay.Specifications;
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
    long PaidById
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
    }
}

internal sealed class CreateTaxiExpenseCommandHandler : ICommandHandler<CreateTaxiExpenseCommand, TaxiExpenseDto>
{
    private readonly ITaxiExpenseRepository _taxiExpenseRepository;
    private readonly ITransportDayRepository _transportDayRepository;
    private readonly ICrewMembershipRepository _crewMembershipRepository;
    private readonly TaxiExpenseMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;

    public CreateTaxiExpenseCommandHandler(
        ITaxiExpenseRepository taxiExpenseRepository,
        ITransportDayRepository transportDayRepository,
        ICrewMembershipRepository crewMembershipRepository,
        TaxiExpenseMapper expenseMapper,
        IUnitOfWork unitOfWork,
        TimeProvider timeProvider)
    {
        _taxiExpenseRepository = taxiExpenseRepository;
        _transportDayRepository = transportDayRepository;
        _crewMembershipRepository = crewMembershipRepository;
        _mapper = expenseMapper;
        _unitOfWork = unitOfWork;
        _timeProvider = timeProvider;
    }

    public async Task<TaxiExpenseDto> Handle(CreateTaxiExpenseCommand request, CancellationToken cancellationToken)
    {
        var spec = new TransportDayByIdSpec(request.TransportDayId);
        var transportDay = await _transportDayRepository.FirstOrDefaultAsync(spec, cancellationToken);

        if (transportDay is null)
            throw new ResourceNotFoundException(TransportDayErrors.NotFound);

        if (transportDay.Confirmed)
            throw new BusinessLogicException(TransportDayErrors.AlreadyConfirmed);

        var taxiExpenseExists = await _taxiExpenseRepository
            .AnyAsync(new TaxiExpenseByTransportDayIdSpec(request.TransportDayId, request.Leg), cancellationToken);

        if (taxiExpenseExists)
            throw new BusinessLogicException(TaxiExpenseErrors.ExpenseThisLegAlreadyExist);

        var crewMemberships = await _crewMembershipRepository
            .ListAsync(new CrewMembershipByCrewIdSpec(transportDay.CrewId), cancellationToken);

        var isPayerInCrew = crewMemberships.Any(x => x.UserId == request.PaidById);

        if (!isPayerInCrew)
            throw new BusinessLogicException(TaxiExpenseErrors.ExpensePaidByNonExistentCrewMember);

        var taxiExpense = new Domain.Entities.TaxiExpense
        {
            TransportDayId = request.TransportDayId,
            Leg = request.Leg,
            Amount = request.Amount,
            PaidById = request.PaidById,
            TaxiExpenseStatus = TaxiExpenseStatus.Pending,
            CreatedAt = _timeProvider.GetLocalDateTimeNowKindUtc()
        };

        await _taxiExpenseRepository.AddAsync(taxiExpense, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map(taxiExpense);
    }
}