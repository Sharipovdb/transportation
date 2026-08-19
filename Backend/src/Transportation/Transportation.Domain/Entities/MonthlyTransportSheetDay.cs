namespace Transportation.Domain.Entities;

/// <summary>
/// What one working day contributed to a crew's month, frozen when the sheet was
/// generated so a later edit of the daily log cannot move money that was signed off.
///
/// One day is one row: the distance the crew covered in its own car (a round trip when
/// both legs were driven) and the taxi money it fronted. Who paid the fare is not
/// repeated here — the lead receives the total and distributes it from the taxi-expense
/// record, which is where that detail lives.
/// </summary>
public sealed class MonthlyTransportSheetDay : BaseEntity
{
    public long MonthlyTransportSheetId { get; set; }

    public MonthlyTransportSheet MonthlyTransportSheet { get; set; } = null!;

    public long TransportDayId { get; set; }

    public TransportDay TransportDay { get; set; } = null!;

    public DateOnly Date { get; set; }

    /// <summary>Kilometres driven in the crew's own car on this day.</summary>
    public double DrivenKm { get; set; }

    /// <summary>Kilometres driven for company purposes beyond the commute.</summary>
    public double ExtraBusinessKm { get; set; }

    /// <summary>Approved taxi fares for this day, whoever in the crew fronted them.</summary>
    public decimal TaxiAmount { get; set; }
}
