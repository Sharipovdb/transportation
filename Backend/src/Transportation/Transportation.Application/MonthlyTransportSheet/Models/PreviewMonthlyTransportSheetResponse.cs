using Transportation.Application.PayoutLine.Models;

namespace Transportation.Application.MonthlyTransportSheet.Models;

public sealed class PreviewMonthlyTransportSheetResponse
{
    public long CrewId { get; set; }

    public int Year { get; set; }

    public int Month { get; set; }

    public bool IsConfirmed { get; set; }
    
    public List<PayoutLineDto> PayoutLines { get; set; } = [];

    public double TotalDriverKm => PayoutLines.Sum(line => line.DriverKm);

    public double TotalExtraBusinessKm => PayoutLines.Sum(line => line.ExtraBusinessKm);

    public decimal TotalTaxiAmount => PayoutLines.Sum(line => line.TaxiCompensation);

    public decimal TotalAmount => PayoutLines.Sum(line => line.TotalAmount);
}