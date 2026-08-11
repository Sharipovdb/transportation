using Transportation.Mediator.Helper.Common.Models;

namespace Transportation.Application.PayoutLine;

public static class PayoutLineErrors
{
    public static readonly Error NotFound = new(
        "PayoutLine.NotFound",
        "Payout line not found."
    );

    public static readonly Error AlreadyPaid = new(
        "PayoutLine.AlreadyPaid",
        "This member has already been paid for this month."
    );

    public static readonly Error NothingToPay = new(
        "PayoutLine.NothingToPay",
        "This member has nothing to be paid for this month."
    );
}
