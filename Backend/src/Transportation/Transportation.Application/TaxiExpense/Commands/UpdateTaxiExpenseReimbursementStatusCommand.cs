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

public record UpdateTaxiExpenseReimbursementStatusCommand( 
    long TaxiExpenseId,
    TaxiExpenseStatus TaxiExpenseStatus
) : ICommand<TaxiExpenseDto>;

// ReSharper disable once UnusedType.Global
public sealed class UpdateTaxiExpenseReimbursementStatusCommandValidation 
    : AbstractValidator<UpdateTaxiExpenseReimbursementStatusCommand>
{
    public UpdateTaxiExpenseReimbursementStatusCommandValidation()
    {
        RuleFor(p => p.TaxiExpenseId)
            .GreaterThan(0)
            .WithMessage("TaxiExpenseId must be greater than zero");
        
        RuleFor(p => p.TaxiExpenseStatus)
            .IsInEnum()
            .WithMessage("ReimbursementStatus must be in Enum");
    }
}

internal sealed class UpdateTaxiExpenseReimbursementStatusCommandHandler 
    : ICommandHandler<UpdateTaxiExpenseReimbursementStatusCommand, TaxiExpenseDto>
{
    private readonly ITaxiExpenseRepository _taxiExpenseRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TaxiExpenseMapper _taxiExpenseMapper;
    private readonly TimeProvider _timeProvider;

    public UpdateTaxiExpenseReimbursementStatusCommandHandler(
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

    public async Task<TaxiExpenseDto> Handle(UpdateTaxiExpenseReimbursementStatusCommand request, CancellationToken cancellationToken)
    {
        var spec = new TaxiExpenseByIdSpec(request.TaxiExpenseId);
        var taxiExpense = await _taxiExpenseRepository.FirstOrDefaultAsync(spec, cancellationToken);

        if (taxiExpense is null)
            throw new ResourceNotFoundException(TaxiExpenseErrors.NotFound);
        
        if (taxiExpense.TaxiExpenseStatus is not TaxiExpenseStatus.Pending)
            throw new BusinessLogicException(TaxiExpenseErrors.ExpenseNotPending);

        switch (request.TaxiExpenseStatus)
        {
            case TaxiExpenseStatus.Approved:
            {
                if (taxiExpense.TaxiExpenseStatus is TaxiExpenseStatus.Approved)
                    throw new BusinessLogicException(TaxiExpenseErrors.ExpenseAlreadyApproved);
                
                taxiExpense.TaxiExpenseStatus = TaxiExpenseStatus.Approved;
                break;
            }

            case TaxiExpenseStatus.Rejected:
            {
                if (taxiExpense.TaxiExpenseStatus is TaxiExpenseStatus.Rejected)
                    throw new BusinessLogicException(TaxiExpenseErrors.ExpenseAlreadyRejected);
                
                taxiExpense.TaxiExpenseStatus = TaxiExpenseStatus.Rejected;
                break;
            }
            
            default:
                throw new BusinessLogicException(TaxiExpenseErrors.InvalidEnumValue);
        }

        taxiExpense.UpdatedAt = _timeProvider.GetLocalDateTimeNowKindUtc();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        return _taxiExpenseMapper.Map(taxiExpense);
    }
}