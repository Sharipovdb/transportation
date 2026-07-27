using Transportation.Application.TaxiExpense.Models;
using Transportation.Application.TaxiExpense.Repositories;
using Transportation.Domain.Entities;
using Transportation.Mediator.Helper.Commands;
using Transportation.Mediator.Helper.Common.Extensions;
using Transportation.Mediator.Helper.Exceptions;
using Transportation.Mediator.Helper.Persistence;

namespace Transportation.Application.TaxiExpense.Commands;

public sealed record UpdateTaxiExpenseCommand(long Id,
    long TransportDayId,
    long PaidById,
    Leg Leg,
    decimal Amount,
    TaxiExpenseStatus TaxiExpenseStatus
) : ICommand<TaxiExpenseDto>;

internal sealed class UpdateTaxiExpenseCommandHandler : ICommandHandler<UpdateTaxiExpenseCommand, TaxiExpenseDto>
{
    private readonly ITaxiExpenseRepository _taxiExpenseRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TaxiExpenseMapper _taxiExpenseMapper;
    private readonly TimeProvider _timeProvider;

    public UpdateTaxiExpenseCommandHandler(ITaxiExpenseRepository taxiExpenseRepository,
        IUnitOfWork unitOfWork,
        TaxiExpenseMapper taxiExpenseMapper,
        TimeProvider timeProvider)
    {
        _taxiExpenseRepository = taxiExpenseRepository;
        _unitOfWork = unitOfWork;
        _taxiExpenseMapper = taxiExpenseMapper;
        _timeProvider = timeProvider;
    }

    public async Task<TaxiExpenseDto> Handle(UpdateTaxiExpenseCommand request, CancellationToken cancellationToken)
    {
        var entity = await _taxiExpenseRepository.GetByIdAsync(request.Id, cancellationToken);

        if (entity is null)
            throw new BusinessLogicException(TaxiExpenseErrors.NotFound);

        entity.TransportDayId = request.TransportDayId;
        entity.PaidById = request.PaidById;
        entity.Leg = request.Leg;
        entity.Amount = request.Amount;
        entity.TaxiExpenseStatus = request.TaxiExpenseStatus;
        entity.UpdatedAt = _timeProvider.GetLocalDateTimeNowKindUtc();

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _taxiExpenseMapper.Map(entity);
    }
}
