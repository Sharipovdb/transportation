using FluentValidation;
using Transportation.Application.MonthlyTransportSheet.Models;
using Transportation.Application.MonthlyTransportSheet.Repositories;
using Transportation.Application.MonthlyTransportSheet.Services;
using Transportation.Application.MonthlyTransportSheet.Specifications;
using Transportation.Application.PayoutLine;
using Transportation.Mediator.Helper.Commands;
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

    public GenerateMonthlyTransportSheetCommandHandler(
        IMonthlyTransportSheetRepository monthlyTransportSheetRepository,
        IMonthlyTransportSheetGenerator generator,
        IUnitOfWork unitOfWork,
        PayoutLineMapperMinually payoutLineMapper)
    {
        _monthlyTransportSheetRepository = monthlyTransportSheetRepository;
        _generator = generator;
        _unitOfWork = unitOfWork;
        _payoutLineMapper = payoutLineMapper;
    }

    public async Task<MonthlyTransportSheetDto> Handle(
        GenerateMonthlyTransportSheetCommand request,
        CancellationToken cancellationToken)
    {
        var exists = await _monthlyTransportSheetRepository
            .AnyAsync(new MonthlyTransportSheetByPeriodSpec(
                request.CrewId, request.Year, request.Month
            ), cancellationToken);

        if (exists)
            throw new BusinessLogicException(MonthlyTransportSheetErrors.AlreadyExists);

        var monthlyTransportSheet = await _generator
            .GenerateAsync(request.CrewId, request.Year, request.Month, cancellationToken);

        await _monthlyTransportSheetRepository.AddAsync(monthlyTransportSheet, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var createdMonthlyTransportSheet = await _monthlyTransportSheetRepository.FirstOrDefaultAsync(
            new MonthlyTransportSheetWithPayoutsSpec(monthlyTransportSheet.Id),
            cancellationToken);

        if (createdMonthlyTransportSheet is null)
            throw new ResourceNotFoundException(MonthlyTransportSheetErrors.NotFound);

        return new MonthlyTransportSheetDto
        {
            Id = createdMonthlyTransportSheet.Id,
            CrewId = createdMonthlyTransportSheet.CrewId,
            Year = createdMonthlyTransportSheet.Year,
            Month = createdMonthlyTransportSheet.Month,
            IsConfirmed = createdMonthlyTransportSheet.IsConfirmed,
            PayoutLines = _payoutLineMapper.Map(createdMonthlyTransportSheet.PayoutLines)
        };
    }
}