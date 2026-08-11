using FluentValidation;
using Transportation.Application.MonthlyTransportSheet.Models;
using Transportation.Application.MonthlyTransportSheet.Repositories;
using Transportation.Application.MonthlyTransportSheet.Services;
using Transportation.Application.MonthlyTransportSheet.Specifications;
using Transportation.Application.PayoutLine;
using Transportation.Mediator.Helper.Commands;
using Transportation.Mediator.Helper.Common.Extensions;
using Transportation.Mediator.Helper.Exceptions;
using Transportation.Mediator.Helper.Persistence;

namespace Transportation.Application.MonthlyTransportSheet.Commands;

public sealed record GenerateMonthlyTransportSheetCommand(
    long CrewId,
    int Year,
    int Month
) : ICommand<MonthlyTransportSheetDto>;

// ReSharper disable once UnusedType.Global
public sealed class GenerateMonthlyTransportSheetCommandValidator
    : AbstractValidator<GenerateMonthlyTransportSheetCommand>
{
    public GenerateMonthlyTransportSheetCommandValidator()
    {
        RuleFor(x => x.CrewId)
            .GreaterThan(0)
            .WithMessage("CrewId must be greater than 0");

        RuleFor(x => x.Year)
            .InclusiveBetween(2000, 2100)
            .WithMessage("Year must be between 2000 and 2100");

        RuleFor(x => x.Month)
            .InclusiveBetween(1, 12)
            .WithMessage("Month must be between 1 and 12");
    }
}

internal sealed class GenerateMonthlyTransportSheetCommandHandler
    : ICommandHandler<GenerateMonthlyTransportSheetCommand, MonthlyTransportSheetDto>
{
    private readonly IMonthlyTransportSheetRepository _monthlyTransportSheetRepository;
    private readonly IMonthlyTransportSheetGenerator _generator;
    private readonly IUnitOfWork _unitOfWork;
    private readonly PayoutLineMapperMinually _payoutLineMapper;
    private readonly TimeProvider _timeProvider;

    public GenerateMonthlyTransportSheetCommandHandler(
        IMonthlyTransportSheetRepository monthlyTransportSheetRepository,
        IMonthlyTransportSheetGenerator generator,
        IUnitOfWork unitOfWork,
        PayoutLineMapperMinually payoutLineMapper,
        TimeProvider timeProvider)
    {
        _monthlyTransportSheetRepository = monthlyTransportSheetRepository;
        _generator = generator;
        _unitOfWork = unitOfWork;
        _payoutLineMapper = payoutLineMapper;
        _timeProvider = timeProvider;
    }

    /// <summary>
    /// Generating is idempotent for a draft sheet: it recomputes the period from scratch
    /// and replaces the payout lines. Transport days keep being logged and confirmed
    /// after a sheet is first produced, and a one-shot "create only" sheet silently froze
    /// the payouts at whatever had been confirmed at that moment.
    /// </summary>
    public async Task<MonthlyTransportSheetDto> Handle(
        GenerateMonthlyTransportSheetCommand request,
        CancellationToken cancellationToken)
    {
        var existing = await _monthlyTransportSheetRepository.FirstOrDefaultAsync(
            new MonthlyTransportSheetByPeriodSpec(request.CrewId, request.Year, request.Month), cancellationToken);

        var calculated = await _generator
            .GenerateAsync(request.CrewId, request.Year, request.Month, cancellationToken);

        var sheetId = existing is null
            ? await CreateAsync(calculated, cancellationToken)
            : await ReplacePayoutLinesAsync(existing.Id, calculated, cancellationToken);

        var saved = await _monthlyTransportSheetRepository.FirstOrDefaultAsync(
            new MonthlyTransportSheetWithPayoutsSpec(sheetId), cancellationToken);

        if (saved is null)
            throw new ResourceNotFoundException(MonthlyTransportSheetErrors.NotFound);

        return new MonthlyTransportSheetDto
        {
            Id = saved.Id,
            CrewId = saved.CrewId,
            Year = saved.Year,
            Month = saved.Month,
            IsConfirmed = saved.IsConfirmed,
            PayoutLines = _payoutLineMapper.Map(saved.PayoutLines)
        };
    }

    private async Task<long> CreateAsync(
        Domain.Entities.MonthlyTransportSheet calculated,
        CancellationToken cancellationToken)
    {
        await _monthlyTransportSheetRepository.AddAsync(calculated, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return calculated.Id;
    }

    private async Task<long> ReplacePayoutLinesAsync(
        long sheetId,
        Domain.Entities.MonthlyTransportSheet calculated,
        CancellationToken cancellationToken)
    {
        var sheet = await _monthlyTransportSheetRepository.FirstOrDefaultAsync(
            new MonthlyTransportSheetWithPayoutsSpec(sheetId), cancellationToken);

        if (sheet is null)
            throw new ResourceNotFoundException(MonthlyTransportSheetErrors.NotFound);

        // A confirmed sheet is signed off as a whole and may not be touched again.
        if (sheet.IsConfirmed)
            throw new BusinessLogicException(MonthlyTransportSheetErrors.AlreadyConfirmed);

        // A member who has already been paid keeps the line they were paid on: that is a
        // record of money handed over, not a figure to recompute. Everyone else's line is
        // dropped and rebuilt, which is what lets newly confirmed days reach the payout.
        var settledUserIds = sheet.PayoutLines
            .Where(x => x.IsPaid)
            .Select(x => x.UserId)
            .ToHashSet();

        // Removing orphans the old lines, which cascades to their taxi-expense links.
        sheet.PayoutLines.RemoveAll(x => !x.IsPaid);

        foreach (var payoutLine in calculated.PayoutLines.Where(x => !settledUserIds.Contains(x.UserId)))
            sheet.PayoutLines.Add(payoutLine);

        sheet.UpdatedAt = _timeProvider.GetLocalDateTimeNowKindUtc();

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return sheet.Id;
    }
}