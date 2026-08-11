namespace Transportation.Domain.Entities;

public class TransportDay : BaseEntity
{
    public long CrewId { get; set; }
    public Crew Crew { get; set; } = null!;

    /// <summary>
    /// The working day itself, as a calendar date. Deliberately not a
    /// <see cref="DateTime"/>: a crew's Monday is a Monday regardless of time zone, and
    /// storing it as an instant used to shift it a day when written from a UTC+5 server.
    /// </summary>
    public DateOnly Date { get; set; }

    public TransportMode MorningMode { get; set; }
    public TransportMode? AfternoonMode { get; set; }

    public double BaseRouteKm { get; set; }

    public double ExtraCommuteKm { get; set; } = 0;

    public double ExtraBusinessKm { get; set; } = 0;

    public long? DriverId { get; set; }
    public User? Driver { get; set; }

    public string Notes { get; set; } = string.Empty;

    public long LoggedBy { get; set; }
    public User LoggedByUser { get; set; } = null!;

    public DateTime LoggedAt { get; set; }
    public bool Confirmed { get; set; }
    public List<TaxiExpense> TaxiExpenses { get; set; } = new();

    /// <summary>
    /// Length of a single commute leg: the crew's route as it stood on this day plus
    /// any detour driven on top of it.
    /// </summary>
    public double CommuteKmPerLeg => BaseRouteKm + ExtraCommuteKm;

    /// <summary>
    /// How many of the day's two legs the crew covered in a member's own car.
    /// </summary>
    public int DrivenLegCount =>
        (MorningMode is TransportMode.Driven ? 1 : 0) +
        (AfternoonMode is TransportMode.Driven ? 1 : 0);

    /// <summary>
    /// Kilometres actually driven in a member's car on this day. Taxi and no-travel legs
    /// contribute nothing: a taxi leg is settled in money (its fare), never in distance.
    /// </summary>
    public double DrivenCommuteKm => DrivenLegCount * CommuteKmPerLeg;
}

public enum TransportMode
{
    Driven,
    Taxi,
    None
}