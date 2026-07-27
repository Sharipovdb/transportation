using FluentValidation;
using Transportation.Application.TaxiExpense.Models;
using Transportation.Application.TaxiExpense.Repositories;
using Transportation.Application.TaxiExpense.Specifications;
using Transportation.Domain.Entities;
using Transportation.Mediator.Helper.Commands;
using Transportation.Mediator.Helper.Common.Extensions;
using Transportation.Mediator.Helper.Exceptions;
using Transportation.Mediator.Helper.Persistence;

namespace Transportation.Application.TaxiExpense.Commands;

public record TaxiExpensePaidCommand(long TaxiExpenseId) :  ICommand<TaxiExpenseDto>;

// ReSharper disable once UnusedType.Global
public sealed class TaxiExpensePaidCommandValidator : AbstractValidator<TaxiExpensePaidCommand>
{
    public TaxiExpensePaidCommandValidator()
    {
        RuleFor(command => command.TaxiExpenseId)
            .GreaterThan(0)
            .WithMessage("TaxiExpenseId must be greater than zero");
    }
}

internal sealed class TaxiExpensePaidCommandHandler : ICommandHandler<TaxiExpensePaidCommand, TaxiExpenseDto>
{
    private readonly ITaxiExpenseRepository _taxiExpenseRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TaxiExpenseMapper _taxiExpenseMapper;
    private readonly TimeProvider _timeProvider;

    public TaxiExpensePaidCommandHandler(
        ITaxiExpenseRepository taxiExpenseRepository,
        IUnitOfWork unitOfWork,
        TaxiExpenseMapper taxiExpenseMapper,
        TimeProvider timeProvider)
    {
        _taxiExpenseRepository = taxiExpenseRepository;
        _unitOfWork = unitOfWork;
        _taxiExpenseMapper = taxiExpenseMapper;
        _timeProvider = timeProvider;
    }

    public async Task<TaxiExpenseDto> Handle(TaxiExpensePaidCommand request, CancellationToken cancellationToken)
    {
        var spec = new TaxiExpenseByIdSpec(request.TaxiExpenseId);
        var taxiExpense = await _taxiExpenseRepository.FirstOrDefaultAsync(spec, cancellationToken);

        if (taxiExpense is null)
            throw new ResourceNotFoundException(TaxiExpenseErrors.NotFound);

        if (taxiExpense.TaxiExpenseStatus is not TaxiExpenseStatus.Approved)
            throw new BusinessLogicException(TaxiExpenseErrors.ExpenseNotApproved);

        taxiExpense.TaxiExpenseStatus = TaxiExpenseStatus.Paid;
        taxiExpense.UpdatedAt = _timeProvider.GetLocalDateTimeNowKindUtc();
        
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        return _taxiExpenseMapper.Map(taxiExpense);
    }
}