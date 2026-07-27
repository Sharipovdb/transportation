namespace Transportation.Domain.Entities;

public class Route : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public double DistanceKm { get; set; }
}