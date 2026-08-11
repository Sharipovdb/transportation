using Transportation.Application.TransportDay.Models;

namespace Transportation.API.Models;

public sealed class UpdateTransportDayRequest
{
    public Domain.Entities.TransportMode? MorningMode { get; set; }
    public Domain.Entities.TransportMode? AfternoonMode { get; set; }
    public double? ExtraCommuteKm { get; set; }
    public double? ExtraBusinessKm { get; set; }
    public string? Notes { get; set; }

    /// <summary>
    /// The fare of every leg travelled by taxi. Sent in full on each update: the day's
    /// taxi expenses are replaced by this list, so an omitted leg drops its expense.
    /// </summary>
    public IReadOnlyList<TransportDayTaxiFare>? TaxiFares { get; set; }
}
