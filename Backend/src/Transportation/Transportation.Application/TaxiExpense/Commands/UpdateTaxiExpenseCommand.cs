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

public sealed record UpdateTaxiExpenseCommand(
    long Id,
    long? PaidById,
    Leg? Leg,
    decimal? Amount
) : ICommand<TaxiExpenseDto>;

internal sealed class UpdateTaxiExpenseCommandHandler : ICommandHandler<UpdateTaxiExpenseCommand, TaxiExpenseDto>
{
    private readonly ITaxiExpenseRepository _taxiExpenseRepository;
    private readonly ICrewMembershipRepository _crewMembershipRepository;
    private readonly ITransportDayRepository _transportDayRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TaxiExpenseMapper _taxiExpenseMapper;
    private readonly TimeProvider _timeProvider;

    public UpdateTaxiExpenseCommandHandler(
        ITaxiExpenseRepository taxiExpenseRepository,
        ICrewMembershipRepository crewMembershipRepository,
        ITransportDayRepository transportDayRepository,
        IUnitOfWork unitOfWork,
        TaxiExpenseMapper taxiExpenseMapper,
        TimeProvider timeProvider)
    {
        _taxiExpenseRepository = taxiExpenseRepository;
        _crewMembershipRepository = crewMembershipRepository;
        _transportDayRepository = transportDayRepository;
        _unitOfWork = unitOfWork;
        _taxiExpenseMapper = taxiExpenseMapper;
        _timeProvider = timeProvider;
    }

    public async Task<TaxiExpenseDto> Handle(UpdateTaxiExpenseCommand request, CancellationToken cancellationToken)
    {
        var spec = new TaxiExpenseByIdSpec(request.Id);
        var entity = await _taxiExpenseRepository.FirstOrDefaultAsync(spec, cancellationToken);

        if (entity is null)
            throw new ResourceNotFoundException(TaxiExpenseErrors.NotFound);

        if (entity.TaxiExpenseStatus is not TaxiExpenseStatus.Pending)
            throw new BusinessLogicException(TaxiExpenseErrors.ExpenseNotPending);

        var transportDay = await _transportDayRepository
            .FirstOrDefaultAsync(new TransportDayByIdSpec(entity.TransportDayId), cancellationToken);

        if (transportDay is null)
            throw new ResourceNotFoundException(TransportDayErrors.NotFound);

        var crewMemberships = await _crewMembershipRepository
            .ListAsync(new CrewMembershipByCrewIdSpec(transportDay.CrewId), cancellationToken);

        var isPayerInCrew = crewMemberships.Any(x => x.UserId == request.PaidById);

        if (!isPayerInCrew)
            throw new BusinessLogicException(TaxiExpenseErrors.ExpensePaidByNonExistentCrewMember);

        if (request.PaidById.HasValue) 
            entity.PaidById = request.PaidById.Value;

        if (request.Leg.HasValue)
            entity.Leg = request.Leg.Value;

        if (request.Amount.HasValue)
            entity.Amount = request.Amount.Value;

        entity.UpdatedAt = _timeProvider.GetLocalDateTimeNowKindUtc();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _taxiExpenseMapper.Map(entity);
    }
}