using Transportation.Mediator.Helper.Common.Models;

namespace Transportation.Application.PayoutLine;

public static class PayoutLineErrors
{
    public static readonly Error NotFound = new(
        "PayoutLine.NotFound",
        "Payout line not found."
    );
}
