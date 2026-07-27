using Transportation.Mediator.Helper.Common;
using Transportation.Mediator.Helper.Common.Models;

namespace Transportation.Mediator.Helper.Exceptions;

/// <summary>
/// У пользователя нет доступа.
/// </summary>
public class AccessDeniedException : BusinessLogicException
{
    public AccessDeniedException() : this(GeneralErrors.AccessDenied)
    {
    }

    public AccessDeniedException(Error error) : base(error)
    {
    }

    public AccessDeniedException(Error error, Exception innerException) : base(error, innerException)
    {
    }
}
