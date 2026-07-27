using Transportation.Application.TaxiExpense.Models;

namespace Transportation.Application.TransportDay.Models;

public sealed class TransportDayDto
{
    public long Id { get; set; }

    public long CrewId { get; set; }

    public DateTime Date { get; set; }

    public string MorningMode { get; set; } = string.Empty;

    public string? AfternoonMode { get; set; }

    public long? DriverId { get; set; }

    public double TotalCommuteKm { get; set; }

    public double ExtraBusinessKm { get; set; }

    public string? Notes { get; set; }

    public long LoggedBy { get; set; }

    public DateTime LoggedAt { get; set; }

    public bool Confirmed { get; set; }

    public List<TaxiExpenseDto> TaxiExpenses { get; set; } = new();
}