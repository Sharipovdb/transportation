namespace Transportation.Domain.Entities;

public class Route : BaseEntity
{
    public string Name { get; set; }
    public double DistanceKm { get; set; }
}