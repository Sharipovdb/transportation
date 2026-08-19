using FluentValidation;
using Transportation.Domain.Entities;
using Transportation.Application.MonthlyTransportSheet.Repositories;
using Transportation.Application.MonthlyTransportSheet.Specifications;
using Transportation.Mediator.Helper.Commands;
using Transportation.Mediator.Helper.Common.Extensions;
using Transportation.Mediator.Helper.Exceptions;
using Transportation.Mediator.Helper.Persistence;
using Transportation.Shared;
using Transportation.Shared.Extensions;
using Transportation.Shared.Middlewares;

namespace Transportation.Application.MonthlyTransportSheet.Commands;

public sealed record MarkMonthlyTransportSheetPaidCommand(long Id) : ICommand;

// ReSharper disable once UnusedType.Global
public sealed class MarkMonthlyTransportSheetPaidCommandValidator
    : AbstractValidator<MarkMonthlyTransportSheetPaidCommand>
{
    public MarkMonthlyTransportSheetPaidCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Id must be greater than 0");
    }
}

/// <summary>
/// Releases a crew's money for one month to its lead, who distributes it inside the
/// team. The sheet is the unit of settlement, and <c>IsPaid</c> makes a second payment
/// for the same crew and month impossible.
///
/// Paying is also what settles the individual fares behind the sheet: they are Approved
/// — owed but not handed over — right up to this point, and only here do they become
/// Paid. Nothing may report a fare as paid before the money has actually left.
/// </summary>
internal sealed class MarkMonthlyTransportSheetPaidCommandHandler
    : ICommandHandler<MarkMonthlyTransportSheetPaidCommand>
{
    private readonly IMonthlyTransportSheetRepository _monthlyTransportSheetRepository;
    private readonly ICurrentUserAccessor _currentUserAccessor;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;

    public MarkMonthlyTransportSheetPaidCommandHandler(
        IMonthlyTransportSheetRepository monthlyTransportSheetRepository,
        ICurrentUserAccessor currentUserAccessor,
        IUnitOfWork unitOfWork,
        TimeProvider timeProvider)
    {
        _monthlyTransportSheetRepository = monthlyTransportSheetRepository;
        _currentUserAccessor = currentUserAccessor;
        _unitOfWork = unitOfWork;
        _timeProvider = timeProvider;
    }

    public async Task Handle(MarkMonthlyTransportSheetPaidCommand request, CancellationToken cancellationToken)
    {
        var sheet = await _monthlyTransportSheetRepository.FirstOrDefaultAsync(
            new MonthlyTransportSheetByIdSpec(request.Id),
            cancellationToken);

        if (sheet is null)
            throw new ResourceNotFoundException(MonthlyTransportSheetErrors.NotFound);

        // Confirming is what signs the figures off; paying an unconfirmed sheet would
        // release money nobody has agreed to yet.
        if (!sheet.IsConfirmed)
            throw new BusinessLogicException(MonthlyTransportSheetErrors.NotConfirmed);

        if (sheet.IsPaid)
            throw new BusinessLogicException(MonthlyTransportSheetErrors.AlreadyPaid);

        // A month of driving alone owes nothing here: kilometres are reported and priced
        // outside this system, so marking it paid would be a meaningless audit entry.
        if (sheet.TotalTaxiAmount <= 0)
            throw new BusinessLogicException(MonthlyTransportSheetErrors.NothingToPay);

        var now = _timeProvider.GetLocalDateTimeNowKindUtc();

        // The sheet and the fares it settles are two aggregates, and a half-applied
        // payment would leave money both owed and released at once.
        await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            foreach (var taxiExpense in ApprovedFaresOf(sheet))
            {
                taxiExpense.TaxiExpenseStatus = TaxiExpenseStatus.Paid;
                taxiExpense.UpdatedAt = now;
            }

            sheet.IsPaid = true;
            sheet.PaidAt = now;
            sheet.PaidById = _currentUserAccessor.GetRequiredUser().GetUserId();
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

    private static IEnumerable<Domain.Entities.TaxiExpense> ApprovedFaresOf(
        Domain.Entities.MonthlyTransportSheet sheet)
    {
        return sheet.Days
            .SelectMany(day => day.TransportDay.TaxiExpenses)
            .Where(x => !x.IsDeleted && x.TaxiExpenseStatus is TaxiExpenseStatus.Approved);
    }
}
