using Transportation.Application.MonthlyTransportSheet.Models;
using Transportation.Application.PayoutLine.Models;

namespace Transportation.Application.MonthlyTransportSheet.Mappers;

public interface IMonthlyTransportPreviewMapper
{
    PreviewMonthlyTransportSheetResponse Map(
        MonthlyTransportCalculationResult calculation);
}

internal sealed class MonthlyTransportPreviewMapper
    : IMonthlyTransportPreviewMapper
{
    public PreviewMonthlyTransportSheetResponse Map(MonthlyTransportCalculationResult calculation)
    {
        return new PreviewMonthlyTransportSheetResponse
        {
            CrewId = calculation.CrewId,
            Year = calculation.Year,
            Month = calculation.Month,
            IsConfirmed = false,
            PayoutLines = calculation.Payouts
                .Select(x => new PayoutLineDto
                    {
                        UserId = x.UserId,
                        Fullname = $"{x.FirstName} {x.LastName}".Trim(),
                        DriverKm = x.DriverKm,
                        ExtraBusinessKm = x.ExtraBusinessKm,
                        TaxiCompensation = x.TaxiCompensation,
                        TaxiExpenses = x.TaxiExpenses
                    }
                ).ToList()
        };
    }
}