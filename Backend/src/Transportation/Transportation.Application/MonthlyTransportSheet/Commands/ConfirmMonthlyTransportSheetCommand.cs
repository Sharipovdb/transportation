using FluentValidation;
using Transportation.Application.MonthlyTransportSheet.Repositories;
using Transportation.Application.MonthlyTransportSheet.Specifications;
using Transportation.Application.TaxiExpense;
using Transportation.Domain.Entities;
using Transportation.Mediator.Helper.Commands;
using Transportation.Mediator.Helper.Common.Extensions;
using Transportation.Mediator.Helper.Exceptions;
using Transportation.Mediator.Helper.Persistence;
using Transportation.Shared;
using Transportation.Shared.Extensions;
using Transportation.Shared.Middlewares;

namespace Transportation.Application.MonthlyTransportSheet.Commands;

public sealed record ConfirmMonthlyTransportSheetCommand(
    long MonthlyTransportSheetId
) : ICommand;

// ReSharper disable once UnusedType.Global
public sealed class ConfirmMonthlyTransportSheetCommandValidator
    : AbstractValidator<ConfirmMonthlyTransportSheetCommand>
{
    public ConfirmMonthlyTransportSheetCommandValidator()
    {
        RuleFor(x => x.MonthlyTransportSheetId)
            .GreaterThan(0)
            .WithMessage("MonthlyTransportSheetId most be  greater than 0");
    }
}

internal sealed class ConfirmMonthlyTransportSheetCommandHandler
    : ICommandHandler<ConfirmMonthlyTransportSheetCommand>
{
    private readonly IMonthlyTransportSheetRepository _monthlyTransportSheetRepository;
    private readonly ICurrentUserAccessor _currentUserAccessor;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;

    public ConfirmMonthlyTransportSheetCommandHandler(
        IMonthlyTransportSheetRepository monthlyTransportSheetRepository,
        IUnitOfWork unitOfWork,
        TimeProvider timeProvider,
        ICurrentUserAccessor currentUserAccessor)
    {
        _monthlyTransportSheetRepository = monthlyTransportSheetRepository;
        _unitOfWork = unitOfWork;
        _timeProvider = timeProvider;
        _currentUserAccessor = currentUserAccessor;
    }

    public async Task Handle(ConfirmMonthlyTransportSheetCommand request, CancellationToken cancellationToken)
    {
        await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var sheet = await _monthlyTransportSheetRepository.FirstOrDefaultAsync(
                new MonthlyTransportSheetByIdSpec(request.MonthlyTransportSheetId),
                cancellationToken);

            if (sheet is null)
                throw new ResourceNotFoundException(MonthlyTransportSheetErrors.NotFound);

            if (sheet.IsConfirmed)
                throw new BusinessLogicException(MonthlyTransportSheetErrors.AlreadyConfirmed);

            var currentUserId = _currentUserAccessor.GetRequiredUser().GetUserId();

            var now = _timeProvider.GetLocalDateTimeNowKindUtc();

            foreach (var payoutLine in sheet.PayoutLines)
            {
                foreach (var payoutLineTaxiExpense in payoutLine.TaxiExpenses)
                {
                    if (payoutLineTaxiExpense.TaxiExpense.TaxiExpenseStatus is not TaxiExpenseStatus.Approved)
                        throw new BusinessLogicException(TaxiExpenseErrors.ExpenseMustBeApprovedBeforePayment);
                }
            }

            foreach (var payoutLine in sheet.PayoutLines)
            {
                foreach (var relation in payoutLine.TaxiExpenses)
                {
                    relation.TaxiExpense.TaxiExpenseStatus = TaxiExpenseStatus.Paid;
                    relation.TaxiExpense.UpdatedAt = now;
                }
            }

            sheet.ConfirmedById = currentUserId;
            sheet.IsConfirmed = true;
            sheet.ConfirmedAt = now;
            sheet.UpdatedAt = now;

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}