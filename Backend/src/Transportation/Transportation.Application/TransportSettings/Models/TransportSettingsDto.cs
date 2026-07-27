namespace Transportation.Application.TransportSettings.Models;

public sealed class TransportSettingsDto
{
    public long Id { get; set; }

    public decimal CommuteKmRate { get; set; }

    public decimal ExtraBusinessKmRate { get; set; }

    public DateTime EffectiveFrom { get; set; }
}