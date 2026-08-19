using FluentValidation;
using Transportation.Application.MonthlyTransportSheet.Repositories;
using Transportation.Application.MonthlyTransportSheet.Services;
using Transportation.Application.MonthlyTransportSheet.Specifications;
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
            .WithMessage("MonthlyTransportSheetId must be greater than 0");
    }
}

/// <summary>
/// Signs a crew's month off. Confirming freezes the figures — it does not release any
/// money, so the fares behind the sheet stay Approved until the sheet is actually paid.
/// </summary>
internal sealed class ConfirmMonthlyTransportSheetCommandHandler
    : ICommandHandler<ConfirmMonthlyTransportSheetCommand>
{
    private readonly IMonthlyTransportSheetRepository _monthlyTransportSheetRepository;
    private readonly IMonthlyTransportSheetBuilder _builder;
    private readonly ICurrentUserAccessor _currentUserAccessor;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;

    public ConfirmMonthlyTransportSheetCommandHandler(
        IMonthlyTransportSheetRepository monthlyTransportSheetRepository,
        IMonthlyTransportSheetBuilder builder,
        IUnitOfWork unitOfWork,
        TimeProvider timeProvider,
        ICurrentUserAccessor currentUserAccessor)
    {
        _monthlyTransportSheetRepository = monthlyTransportSheetRepository;
        _builder = builder;
        _unitOfWork = unitOfWork;
        _timeProvider = timeProvider;
        _currentUserAccessor = currentUserAccessor;
    }

    public async Task Handle(ConfirmMonthlyTransportSheetCommand request, CancellationToken cancellationToken)
    {
        var sheet = await _monthlyTransportSheetRepository.FirstOrDefaultAsync(
            new MonthlyTransportSheetByIdSpec(request.MonthlyTransportSheetId),
            cancellationToken);

        if (sheet is null)
            throw new ResourceNotFoundException(MonthlyTransportSheetErrors.NotFound);

        if (sheet.IsConfirmed)
            throw new BusinessLogicException(MonthlyTransportSheetErrors.AlreadyConfirmed);

        // Days can be logged, corrected or unconfirmed after the sheet was produced.
        // Signing off figures that no longer match the log would be signing off the
        // wrong money, so a stale sheet has to be recalculated first.
        var current = await _builder
            .BuildAsync(sheet.CrewId, sheet.Year, sheet.Month, cancellationToken);

        if (!IsUpToDate(sheet, current))
            throw new BusinessLogicException(MonthlyTransportSheetErrors.OutOfDate);

        var now = _timeProvider.GetLocalDateTimeNowKindUtc();

        sheet.IsConfirmed = true;
        sheet.ConfirmedById = _currentUserAccessor.GetRequiredUser().GetUserId();
        sheet.ConfirmedAt = now;
        sheet.UpdatedAt = now;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static bool IsUpToDate(
        Domain.Entities.MonthlyTransportSheet stored,
        Domain.Entities.MonthlyTransportSheet current)
    {
        return stored.RecipientId == current.RecipientId &&
               stored.Days.Count == current.Days.Count &&
               stored.TotalDrivenKm.Equals(current.TotalDrivenKm) &&
               stored.TotalExtraBusinessKm.Equals(current.TotalExtraBusinessKm) &&
               stored.TotalTaxiAmount == current.TotalTaxiAmount;
    }
}
