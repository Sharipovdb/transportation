using Transportation.Mediator.Helper.Common.Models;

namespace Transportation.Application.MonthlyTransportSheet;

public static class MonthlyTransportSheetErrors
{
    public static readonly Error NotFound = new(
        "MonthlyTransportSheet.NotFound",
        "Monthly transport sheet not found."
    );

    public static readonly Error AlreadyConfirmed = new(
        "MonthlyTransportSheet.AlreadyConfirmed",
        "Monthly transport sheet already confirmed."
    );

    public static readonly Error AlreadyDraft = new(
        "MonthlyTransportSheet.AlreadyDraft",
        "This sheet is still a draft — there is no confirmation to withdraw."
    );

    public static readonly Error PaidIsFinal = new(
        "MonthlyTransportSheet.PaidIsFinal",
        "This sheet has been paid — its money is already released and it can no longer be reopened."
    );

    public static readonly Error NotConfirmed = new(
        "MonthlyTransportSheet.NotConfirmed",
        "Confirm the sheet before releasing its money."
    );

    public static readonly Error AlreadyPaid = new(
        "MonthlyTransportSheet.AlreadyPaid",
        "This crew has already been paid for this month."
    );

    public static readonly Error NothingToPay = new(
        "MonthlyTransportSheet.NothingToPay",
        "This sheet owes no money — only distance, which is priced outside this system."
    );

    public static readonly Error NothingToReport = new(
        "MonthlyTransportSheet.NothingToReport",
        "This crew has no travel logged in this period."
    );

    public static readonly Error CrewHasNoLead = new(
        "MonthlyTransportSheet.CrewHasNoLead",
        "The crew has no lead to pay — assign a driver-lead or a manager-lead first."
    );

    public static readonly Error OutOfDate = new(
        "MonthlyTransportSheet.OutOfDate",
        "The transport days behind this sheet have changed — recalculate it before confirming."
    );
}
