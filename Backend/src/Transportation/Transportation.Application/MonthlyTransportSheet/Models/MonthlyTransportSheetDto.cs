namespace Transportation.Application.MonthlyTransportSheet.Models;

/// <summary>
/// One crew's month as the accountant reads it. <see cref="Id"/> is 0 for an unsaved
/// preview — the preview and the saved sheet are the same shape on purpose, so every
/// screen renders one set of numbers computed in exactly one place.
/// </summary>
public sealed class MonthlyTransportSheetDto
{
    public long Id { get; set; }

    public long CrewId { get; set; }

    public int Year { get; set; }

    public int Month { get; set; }

    /// <summary>The crew lead the money is handed to; they distribute it inside the team.</summary>
    public long RecipientId { get; set; }

    public string RecipientFullname { get; set; } = string.Empty;

    public bool IsConfirmed { get; set; }

    public bool IsPaid { get; set; }

    public DateTime? PaidAt { get; set; }

    public List<MonthlyTransportSheetDayDto> Days { get; set; } = [];

    // Distance is reported, money is settled: the two are totalled apart because they
    // are different units of account, and only the taxi total is payable here.
    public double TotalDrivenKm => Days.Sum(x => x.DrivenKm);

    public double TotalExtraBusinessKm => Days.Sum(x => x.ExtraBusinessKm);

    public decimal TotalTaxiAmount => Days.Sum(x => x.TaxiAmount);
}

/// <summary>One working day of the crew: distance driven, or the taxi fare it cost.</summary>
public sealed class MonthlyTransportSheetDayDto
{
    public long TransportDayId { get; set; }

    public DateOnly Date { get; set; }

    public double DrivenKm { get; set; }

    public double ExtraBusinessKm { get; set; }

    public decimal TaxiAmount { get; set; }
}
