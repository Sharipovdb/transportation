namespace Transportation.Domain.Entities;

public sealed class TransportSettings : BaseEntity
{
    public decimal CommuteKmRate { get; set; }

    public decimal ExtraBusinessKmRate { get; set; }

    public DateTime EffectiveFrom { get; set; }
}