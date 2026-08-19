using FluentValidation;
using Transportation.Application.MonthlyTransportSheet.Mappers;
using Transportation.Application.MonthlyTransportSheet.Models;
using Transportation.Application.MonthlyTransportSheet.Services;
using Transportation.Mediator.Helper.Queries;

namespace Transportation.Application.MonthlyTransportSheet.Queries;

public sealed record PreviewMonthlyTransportSheetQuery(
    long CrewId,
    int Year,
    int Month
) : IQuery<MonthlyTransportSheetDto>;

// ReSharper disable once UnusedType.Global
public sealed class PreviewMonthlyTransportSheetQueryValidator
    : AbstractValidator<PreviewMonthlyTransportSheetQuery>
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

/// <summary>
/// Shows what generating would produce, without writing anything. It runs the same
/// builder and the same mapper as Generate, so a preview can never disagree with the
/// sheet that follows it.
/// </summary>
internal sealed class PreviewMonthlyTransportSheetQueryHandler
    : IQueryHandler<PreviewMonthlyTransportSheetQuery, MonthlyTransportSheetDto>
{
    private readonly IMonthlyTransportSheetBuilder _builder;
    private readonly MonthlyTransportSheetMapper _mapper;

    public PreviewMonthlyTransportSheetQueryHandler(
        IMonthlyTransportSheetBuilder builder,
        MonthlyTransportSheetMapper mapper)
    {
        _builder = builder;
        _mapper = mapper;
    }

    public async Task<MonthlyTransportSheetDto> Handle(
        PreviewMonthlyTransportSheetQuery request,
        CancellationToken cancellationToken)
    {
        var calculated = await _builder
            .BuildAsync(request.CrewId, request.Year, request.Month, cancellationToken);

        return _mapper.Map(calculated);
    }
}
