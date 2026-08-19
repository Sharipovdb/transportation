using Transportation.Application.TaxiExpense.Models;

namespace Transportation.Application.TransportDay.Models;

public sealed class TransportDayDto
{
    public long Id { get; set; }

    public long CrewId { get; set; }

    public DateOnly Date { get; set; }

    public string MorningMode { get; set; } = string.Empty;

    public string? AfternoonMode { get; set; }

    public long? DriverId { get; set; }

    /// <summary>Length of one commute leg — the crew's route plus any detour.</summary>
    public double CommuteKmPerLeg { get; set; }

    /// <summary>Kilometres actually driven in a member's car; taxi legs add none.</summary>
    public double DrivenCommuteKm { get; set; }

    public double ExtraBusinessKm { get; set; }

    public string? Notes { get; set; }

    public long LoggedBy { get; set; }

    public DateTime LoggedAt { get; set; }

    /// <summary>
    /// The day's taxi rides with their review state — a fare only becomes money the
    /// company owes once it is Approved.
    /// </summary>
    public List<TaxiExpenseDto> TaxiExpenses { get; set; } = new();
}