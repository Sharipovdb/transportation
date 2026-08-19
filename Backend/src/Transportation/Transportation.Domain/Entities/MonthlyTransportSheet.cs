namespace Transportation.Domain.Entities;

/// <summary>
/// One crew's travel for one month, settled as a whole.
///
/// The company hands the crew's lead a single amount and the lead distributes it inside
/// the team, so the sheet has exactly one recipient rather than a line per member. Two
/// units of account are kept strictly apart: kilometres driven in the crew's own car are
/// <em>reported</em> — the accountant prices them with their own indices — while the taxi
/// fares the crew fronted are <em>settled</em>, and are the only money this sheet owes.
/// </summary>
public sealed class MonthlyTransportSheet : BaseEntity
{
    public long CrewId { get; set; }

    public Crew Crew { get; set; } = null!;

    public int Year { get; set; }

    public int Month { get; set; }

    /// <summary>
    /// The crew's lead as they stood when the sheet was generated: the driver-lead when
    /// the crew has one, otherwise the manager-lead. Copied onto the sheet so a later
    /// change of lead cannot rewrite who was paid.
    /// </summary>
    public long RecipientId { get; set; }

    public User Recipient { get; set; } = null!;

    public bool IsConfirmed { get; set; }

    public long? ConfirmedById { get; set; }

    public DateTime? ConfirmedAt { get; set; }

    public long CreatedById { get; set; }

    public User CreatedBy { get; set; } = null!;

    // Settlement. A sheet is paid at most once: PaidAt/PaidById record who released the
    // money so a second payout can be refused with an audit trail behind it.
    public bool IsPaid { get; set; }

    public DateTime? PaidAt { get; set; }

    public long? PaidById { get; set; }

    public User? PaidBy { get; set; }

    /// <summary>One entry per confirmed transport day in the period.</summary>
    public List<MonthlyTransportSheetDay> Days { get; set; } = new();

    public double TotalDrivenKm => Days.Sum(x => x.DrivenKm);

    public double TotalExtraBusinessKm => Days.Sum(x => x.ExtraBusinessKm);

    public decimal TotalTaxiAmount => Days.Sum(x => x.TaxiAmount);
}
