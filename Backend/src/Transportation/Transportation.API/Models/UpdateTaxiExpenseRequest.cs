namespace Transportation.API.Models;

/// <summary>
/// What may be corrected on a fare that has not been ruled on yet. The leg and the day
/// are not among them: which legs were taken by taxi is what the transport day says, and
/// the fare follows from it.
/// </summary>
public class UpdateTaxiExpenseRequest
{
    public long? PaidById { get; set; }

    public decimal? Amount { get; set; }
}
