using FluentValidation;
using Transportation.Application.MonthlyTransportSheet.Mappers;
using Transportation.Application.MonthlyTransportSheet.Models;
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
    private readonly IMonthlyTransportSheetBuilder _builder;
    private readonly ICurrentUserAccessor _currentUserAccessor;
    private readonly IUnitOfWork _unitOfWork;
    private readonly MonthlyTransportSheetMapper _mapper;
    private readonly TimeProvider _timeProvider;

    public GenerateMonthlyTransportSheetCommandHandler(
        IMonthlyTransportSheetRepository monthlyTransportSheetRepository,
        IMonthlyTransportSheetBuilder builder,
        ICurrentUserAccessor currentUserAccessor,
        IUnitOfWork unitOfWork,
        MonthlyTransportSheetMapper mapper,
        TimeProvider timeProvider)
    {
        _monthlyTransportSheetRepository = monthlyTransportSheetRepository;
        _builder = builder;
        _currentUserAccessor = currentUserAccessor;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _timeProvider = timeProvider;
    }

    /// <summary>
    /// Generating is idempotent for a draft sheet: it recomputes the period from scratch
    /// and replaces the day rows. Transport days keep being logged and confirmed after a
    /// sheet is first produced, and a one-shot "create only" sheet silently froze the
    /// month at whatever had been confirmed at that moment.
    /// </summary>
    public async Task<MonthlyTransportSheetDto> Handle(
        GenerateMonthlyTransportSheetCommand request,
        CancellationToken cancellationToken)
    {
        var calculated = await _builder
            .BuildAsync(request.CrewId, request.Year, request.Month, cancellationToken);

        if (calculated.Days.Count == 0)
            throw new BusinessLogicException(MonthlyTransportSheetErrors.NothingToReport);

        var now = _timeProvider.GetLocalDateTimeNowKindUtc();

        var existing = await _monthlyTransportSheetRepository.FirstOrDefaultAsync(
            new MonthlyTransportSheetByPeriodSpec(request.CrewId, request.Year, request.Month),
            cancellationToken);

        var sheet = existing is null
            ? await CreateAsync(calculated, now, cancellationToken)
            : Refresh(existing, calculated, now);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map(sheet);
    }

    private async Task<Domain.Entities.MonthlyTransportSheet> CreateAsync(
        Domain.Entities.MonthlyTransportSheet calculated,
        DateTime now,
        CancellationToken cancellationToken)
    {
        calculated.CreatedAt = now;
        calculated.CreatedById = _currentUserAccessor.GetRequiredUser().GetUserId();

        foreach (var day in calculated.Days)
            day.CreatedAt = now;

        await _monthlyTransportSheetRepository.AddAsync(calculated, cancellationToken);

        return calculated;
    }

    private static Domain.Entities.MonthlyTransportSheet Refresh(
        Domain.Entities.MonthlyTransportSheet existing,
        Domain.Entities.MonthlyTransportSheet calculated,
        DateTime now)
    {
        // A confirmed sheet is signed off as a whole and may not be touched again; the
        // correction path is to delete it and generate a fresh one.
        if (existing.IsConfirmed)
            throw new BusinessLogicException(MonthlyTransportSheetErrors.AlreadyConfirmed);

        existing.RecipientId = calculated.RecipientId;
        existing.Recipient = calculated.Recipient;

        // Clearing orphans the old rows, which EF deletes; the fresh ones take their
        // place, so newly confirmed days reach the sheet and withdrawn ones leave it.
        existing.Days.Clear();

        foreach (var day in calculated.Days)
        {
            day.CreatedAt = now;
            existing.Days.Add(day);
        }

        existing.UpdatedAt = now;

        return existing;
    }
}
