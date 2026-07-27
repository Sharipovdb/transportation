namespace Transportation.Application.MonthlyTransportSheet.Models;

public class MonthlyTransportCalculationResult
{
    public long CrewId { get; set; }

    public int Year { get; set; }

    public int Month { get; set; }
    
    public List<PayoutAccumulator> Payouts { get; set; } = [];

    public decimal TotalAmount => Payouts.Sum(x => x.TotalAmount);
}