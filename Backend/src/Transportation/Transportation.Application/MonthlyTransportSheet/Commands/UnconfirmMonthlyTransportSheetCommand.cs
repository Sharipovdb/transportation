using FluentValidation;
using Transportation.Application.MonthlyTransportSheet.Repositories;
using Transportation.Application.MonthlyTransportSheet.Specifications;
using Transportation.Mediator.Helper.Commands;
using Transportation.Mediator.Helper.Common.Extensions;
using Transportation.Mediator.Helper.Exceptions;
using Transportation.Mediator.Helper.Persistence;

namespace Transportation.Application.MonthlyTransportSheet.Commands;

public sealed record UnconfirmMonthlyTransportSheetCommand(
    long MonthlyTransportSheetId
) : ICommand;

// ReSharper disable once UnusedType.Global
public sealed class UnconfirmMonthlyTransportSheetCommandValidator
    : AbstractValidator<UnconfirmMonthlyTransportSheetCommand>
{
    public UnconfirmMonthlyTransportSheetCommandValidator()
    {
        RuleFor(x => x.MonthlyTransportSheetId)
            .GreaterThan(0)
            .WithMessage("MonthlyTransportSheetId must be greater than 0");
    }
}

/// <summary>
/// Withdraws a signature and puts the sheet back to draft, so the month can be corrected
/// and recalculated. Confirming only freezes figures — it releases no money and touches
/// no taxi fare — which is exactly why it can be taken back.
///
/// Payment is the point of no return: once the money has gone to the lead there is
/// nothing here that could undo it, so a paid sheet stays closed.
/// </summary>
internal sealed class UnconfirmMonthlyTransportSheetCommandHandler
    : ICommandHandler<UnconfirmMonthlyTransportSheetCommand>
{
    private readonly IMonthlyTransportSheetRepository _monthlyTransportSheetRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;

    public UnconfirmMonthlyTransportSheetCommandHandler(
        IMonthlyTransportSheetRepository monthlyTransportSheetRepository,
        IUnitOfWork unitOfWork,
        TimeProvider timeProvider)
    {
        _monthlyTransportSheetRepository = monthlyTransportSheetRepository;
        _unitOfWork = unitOfWork;
        _timeProvider = timeProvider;
    }

    public async Task Handle(UnconfirmMonthlyTransportSheetCommand request, CancellationToken cancellationToken)
    {
        var sheet = await _monthlyTransportSheetRepository.FirstOrDefaultAsync(
            new MonthlyTransportSheetByIdSpec(request.MonthlyTransportSheetId),
            cancellationToken);

        if (sheet is null)
            throw new ResourceNotFoundException(MonthlyTransportSheetErrors.NotFound);

        if (!sheet.IsConfirmed)
            throw new BusinessLogicException(MonthlyTransportSheetErrors.AlreadyDraft);

        if (sheet.IsPaid)
            throw new BusinessLogicException(MonthlyTransportSheetErrors.PaidIsFinal);

        sheet.IsConfirmed = false;
        sheet.ConfirmedById = null;
        sheet.ConfirmedAt = null;
        sheet.UpdatedAt = _timeProvider.GetLocalDateTimeNowKindUtc();

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
