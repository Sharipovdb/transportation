using Transportation.Mediator.Helper.Common.Models;

namespace Transportation.Application.User;

public static class UserError
{
    public static readonly Error NotFound = new(
        "USer.NotFound",
        "The specific user does not exist."
        );

    public static readonly Error AlreadyExist = new(
        "User.AlreadyExist",
        "The specific user already exists."
    );
}