namespace Transportation.Domain.Entities;

public class TransportDay : BaseEntity
{
    public long CrewId { get; set; }
    public Crew Crew { get; set; }

    public DateTime Date { get; set; }

    public TransportMode MorningMode { get; set; }
    public TransportMode? AfternoonMode { get; set; }

    public double BaseRouteKm { get; set; }

    public double ExtraCommuteKm { get; set; } = 0;

    public double ExtraBusinessKm { get; set; } = 0;

    public long? DriverId { get; set; }
    public User? Driver { get; set; }

    public string Notes { get; set; } = string.Empty;

    public long LoggedBy { get; set; }
    public User LoggedByUser { get; set; }

    public DateTime LoggedAt { get; set; }
    public bool Confirmed { get; set; }
    public List<TaxiExpense> TaxiExpenses { get; set; } = new();

    public double TotalCommuteKm => BaseRouteKm + ExtraCommuteKm;
}

public enum TransportMode
{
    Driven,
    Taxi,
    None
}