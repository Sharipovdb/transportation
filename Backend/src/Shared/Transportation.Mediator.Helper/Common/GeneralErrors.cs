using Transportation.Mediator.Helper.Common.Models;

namespace Transportation.Mediator.Helper.Common;

public static class GeneralErrors
{
    public static readonly Error Unauthorized = new(
        "General.Unauthorized",
        "Не удалось получить текущего пользователя."
    );

    public static readonly Error AccessDenied = new(
        "General.AccessDenied",
        "Доступ запрещен."
    );

    public static Error CreationError(string message)
    {
        return new Error(
            "Create.Error",
            $"При добавлении возникла ошибка. {message}"
        );
    }

    public static Error UpdateError(string message)
    {
        return new Error(
            "Update.Error",
            $"При обновлении возникла ошибка. {message}"
        );
    }

    public static Error DeleteError(string message)
    {
        return new Error(
            "Update.Error",
            $"При удалении возникла ошибка. {message}"
        );
    }
}