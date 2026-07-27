namespace Transportation.Domain.Entities;

public sealed class MonthlyTransportSheet : BaseEntity
{
    public long CrewId { get; set; }

    public Crew Crew { get; set; }
    public int Year { get; set; }

    public int Month { get; set; }

    public bool IsConfirmed { get; set; }

    public long? ConfirmedById { get; set; }
    public User CreatedBy { get; set; }
    public DateTime? ConfirmedAt { get; set; }

    public List<PayoutLine> PayoutLines { get; set; } = new();
}