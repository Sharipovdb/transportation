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

/// <summary>
/// Corrects what a ride cost and who paid for it, while the fare is still Pending. The
/// leg it belongs to is deliberately not editable — the transport day is what says which
/// legs were taken by taxi, and moving a fare to another leg there would leave one leg
/// claiming nothing and another claiming twice.
/// </summary>
public sealed record UpdateTaxiExpenseCommand(
    long Id,
    long? PaidById,
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
        var entity = await _taxiExpenseRepository
            .FirstOrDefaultAsync(new TaxiExpenseByIdSpec(request.Id), cancellationToken);

        if (entity is null)
            throw new ResourceNotFoundException(TaxiExpenseErrors.NotFound);

        // An expense that has been approved, rejected or paid is an accounting fact.
        if (entity.TaxiExpenseStatus is not TaxiExpenseStatus.Pending)
            throw new BusinessLogicException(TaxiExpenseErrors.ExpenseNotPending);

        if (request.PaidById.HasValue)
        {
            await EnsurePayerIsInCrew(entity.TransportDayId, request.PaidById.Value, cancellationToken);

            entity.PaidById = request.PaidById.Value;
        }

        if (request.Amount.HasValue)
            entity.Amount = request.Amount.Value;

        entity.UpdatedAt = _timeProvider.GetLocalDateTimeNowKindUtc();

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _taxiExpenseMapper.Map(entity);
    }

    // Only someone who travelled with the crew can have fronted its fare. The check used
    // to run even when no payer was supplied, comparing against null, so correcting just
    // the amount always failed as "paid by a non-member".
    private async Task EnsurePayerIsInCrew(long transportDayId, long payerId, CancellationToken cancellationToken)
    {
        var transportDay = await _transportDayRepository
            .FirstOrDefaultAsync(new TransportDayByIdSpec(transportDayId), cancellationToken);

        if (transportDay is null)
            throw new ResourceNotFoundException(TransportDayErrors.NotFound);

        var memberships = await _crewMembershipRepository
            .ListAsync(new CrewMembershipByCrewIdSpec(transportDay.CrewId), cancellationToken);

        if (memberships.All(x => x.UserId != payerId))
            throw new BusinessLogicException(TaxiExpenseErrors.ExpensePaidByNonExistentCrewMember);
    }
}
