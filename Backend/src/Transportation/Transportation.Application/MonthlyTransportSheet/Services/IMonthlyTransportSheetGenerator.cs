using Transportation.Application.MonthlyTransportSheet.Calculations;
using Transportation.Application.MonthlyTransportSheet.Factories;

namespace Transportation.Application.MonthlyTransportSheet.Services;

public interface IMonthlyTransportSheetGenerator
{
    Task<Domain.Entities.MonthlyTransportSheet> GenerateAsync(
        long crewId,
        int year,
        int month,
        CancellationToken cancellationToken);
}

internal sealed class MonthlyTransportSheetGenerator : IMonthlyTransportSheetGenerator
{
    private readonly IMonthlyTransportCalculator _calculator;
    private readonly IMonthlyTransportSheetFactory _factory;

    public MonthlyTransportSheetGenerator(
        IMonthlyTransportCalculator calculator,
        IMonthlyTransportSheetFactory factory)
    {
        _calculator = calculator;
        _factory = factory;
    }

    public async Task<Domain.Entities.MonthlyTransportSheet> GenerateAsync(
        long crewId, int year, int month,
        CancellationToken cancellationToken)
    {
        var calculation = await _calculator.CalculateAsync(crewId, year, month, cancellationToken);

        return _factory.Create(calculation);
    }
}