using FluentValidation;
using Transportation.Application.MonthlyTransportSheet.Calculations;
using Transportation.Application.MonthlyTransportSheet.Mappers;
using Transportation.Application.MonthlyTransportSheet.Models;
using Transportation.Mediator.Helper.Queries;

namespace Transportation.Application.MonthlyTransportSheet.Queries;

public sealed record PreviewMonthlyTransportSheetQuery(
    long CrewId,
    int Year,
    int Month
) : IQuery<PreviewMonthlyTransportSheetResponse>;

// ReSharper disable once UnusedType.Global
public sealed class PreviewMonthlyTransportSheetQueryValidator 
    : AbstractValidator<PreviewMonthlyTransportSheetResponse>
{
    public PreviewMonthlyTransportSheetQueryValidator()
    {
        RuleFor(x => x.CrewId)
            .GreaterThan(0);

        RuleFor(x => x.Year)
            .InclusiveBetween(2000, 2100);

        RuleFor(x => x.Month)
            .InclusiveBetween(1, 12);
    }
}

internal sealed class PreviewMonthlyTransportSheetQueryHandler
    : IQueryHandler<PreviewMonthlyTransportSheetQuery, PreviewMonthlyTransportSheetResponse>
{
    private readonly IMonthlyTransportCalculator _calculator;
    private readonly IMonthlyTransportPreviewMapper _mapper;

    public PreviewMonthlyTransportSheetQueryHandler(
        IMonthlyTransportCalculator calculator,
        IMonthlyTransportPreviewMapper mapper)
    {
        _calculator = calculator;
        _mapper = mapper;
    }


    public async Task<PreviewMonthlyTransportSheetResponse> Handle(
        PreviewMonthlyTransportSheetQuery request,
        CancellationToken cancellationToken)
    {
        var calculation = await _calculator
            .CalculateAsync(
                request.CrewId,
                request.Year,
                request.Month,
                cancellationToken);

        return _mapper.Map(calculation);
    }
}