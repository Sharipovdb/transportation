namespace Transportation.Application.AbsenceNotice;

using Transportation.Mediator.Helper.Common.Models;

public static class AbsenceNoticeErrors
{
    public static readonly Error NotFound = new(
        "AbsenceNotice.NotFound",
        "Absence notice not found."
    );

    public static readonly Error AlreadyExists = new(
        "AbsenceNotice.AlreadyExists",
        "Absence notice already exists for this user on this date."
    );
}