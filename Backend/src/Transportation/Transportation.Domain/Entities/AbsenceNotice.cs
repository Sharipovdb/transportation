namespace Transportation.Domain.Entities;

public sealed class AbsenceNotice : BaseEntity
{
    public long UserId { get; set; }
    public User User { get; set; }

    public DateOnly Date { get; set; }
    public AbsenceScope Scope { get; set; }
    public AbsenceType Type { get; set; }
    public string Reason { get; set; } = string.Empty;
    public bool IsNotified { get; set; }
}

public enum AbsenceScope
{
    Full = 1,
    Morning = 2,
    Afternoon = 3
}

public enum AbsenceType
{
    Worker = 1,
    Driver = 2,
}