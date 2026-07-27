using Transportation.Mediator.Helper.Common;
using Transportation.Mediator.Helper.Common.Models;

namespace Transportation.Mediator.Helper.Exceptions;

/// <summary>
/// Пользователь не авторизован.
/// </summary>
public class UnauthorizedException : BusinessLogicException
{
    public UnauthorizedException() : this(GeneralErrors.Unauthorized)
    {
    }

    public UnauthorizedException(Error error) : base(error)
    {
    }

    public UnauthorizedException(Error error, Exception innerException) : base(error, innerException)
    {
    }
}