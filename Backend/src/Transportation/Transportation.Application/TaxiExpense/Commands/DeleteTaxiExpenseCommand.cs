using Transportation.Application.TaxiExpense.Repositories;
using Transportation.Application.TaxiExpense.Specifications;
using Transportation.Domain.Entities;
using Transportation.Mediator.Helper.Commands;
using Transportation.Mediator.Helper.Exceptions;
using Transportation.Mediator.Helper.Persistence;

namespace Transportation.Application.TaxiExpense.Commands;

public sealed record DeleteTaxiExpenseCommand(long TaxiExpenseId) : ICommand<bool>;

internal sealed class DeleteTaxiExpenseCommandHandler : ICommandHandler<DeleteTaxiExpenseCommand, bool>
{
    private readonly ITaxiExpenseRepository _taxiExpenseRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteTaxiExpenseCommandHandler(ITaxiExpenseRepository taxiExpenseRepository, IUnitOfWork unitOfWork)
    {
        _taxiExpenseRepository = taxiExpenseRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(DeleteTaxiExpenseCommand request, CancellationToken cancellationToken)
    {
        var taxiExpense = await _taxiExpenseRepository
            .FirstOrDefaultAsync(new TaxiExpenseByIdSpec(request.TaxiExpenseId), cancellationToken);

        if (taxiExpense is null)
            throw new ResourceNotFoundException(TaxiExpenseErrors.NotFound);

        if (taxiExpense.TaxiExpenseStatus is not TaxiExpenseStatus.Pending)
            throw new BusinessLogicException(TaxiExpenseErrors.ExpenseNotPending);

        taxiExpense.IsDeleted = true;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}