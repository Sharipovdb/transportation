using Transportation.Domain.Entities;

namespace Transportation.Application.TransportDay.Models;

/// <summary>
/// The fare for one taxi leg, captured in the daily log rather than typed in again on
/// the taxi-expenses screen. A taxi leg costs money and produces no kilometres, so the
/// amount is the only thing worth recording about it — and recording it here is what
/// makes the ride show up as a reimbursable expense.
/// </summary>
public sealed record TransportDayTaxiFare(Leg Leg, decimal Amount, long PaidById);
