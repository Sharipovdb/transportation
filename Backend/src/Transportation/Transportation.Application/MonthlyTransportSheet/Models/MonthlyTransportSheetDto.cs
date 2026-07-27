using Transportation.Application.PayoutLine.Models;

namespace Transportation.Application.MonthlyTransportSheet.Models;

public sealed class MonthlyTransportSheetDto
{
    public long Id { get; set; }

    public long CrewId { get; set; }

    public int Year { get; set; }

    public int Month { get; set; }

    public bool IsConfirmed { get; set; }
    
    public List<PayoutLineDto> PayoutLines { get; set; } = [];
    
    public decimal TotalAmount => PayoutLines.Sum(line => line.TotalAmount);
}