using Transportation.Mediator.Helper.Common.Models;

namespace Transportation.Mediator.Helper.Exceptions;

/// <summary>
/// Исключение бизнес логики.
/// </summary>
public class BusinessLogicException : Exception
{
    public Error Error { get; }

    public BusinessLogicException(Error error) : base(error.Description)
    {
        Error = error;
    }

    public BusinessLogicException(Error error, Exception innerException) : base(error.Description, innerException)
    {
        Error = error;
    }
}