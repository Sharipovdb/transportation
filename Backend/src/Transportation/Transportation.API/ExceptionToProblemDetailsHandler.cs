using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Transportation.Mediator.Helper.Exceptions;

namespace Transportation.API;

public sealed class ExceptionToProblemDetailsHandler : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetailsService;

    public ExceptionToProblemDetailsHandler(IProblemDetailsService problemDetailsService)
    {
        _problemDetailsService = problemDetailsService;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var problemDetails = ConvertToProblemDetails(exception);

        // В HttpContext нет информации о текущем endpoint,
        // поэтому его необходимо брать из IExceptionHandlerFeature.
        var exceptionHandlerFeature = httpContext.Features.Get<IExceptionHandlerFeature>();
        var endpointMetadata = exceptionHandlerFeature?.Endpoint?.Metadata;

        httpContext.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;

        return await _problemDetailsService.TryWriteAsync(
            new ProblemDetailsContext
            {
                HttpContext = httpContext,
                Exception = exception,
                ProblemDetails = problemDetails,
                AdditionalMetadata = endpointMetadata,
            }
        );
    }

    private static ProblemDetails ConvertToProblemDetails(Exception exception)
    {
        return exception switch
        {
            BusinessLogicException businessLogicException => new ProblemDetails
            {
                Title = businessLogicException.Message,
                Status = businessLogicException switch
                {
                    UnauthorizedException => StatusCodes.Status401Unauthorized,
                    AccessDeniedException => StatusCodes.Status403Forbidden,
                    ResourceNotFoundException => StatusCodes.Status404NotFound,
                    _ => StatusCodes.Status400BadRequest,
                },

                Extensions =
                {
                    ["errorCode"] = businessLogicException.Error.Code,
                },
            },
            ValidationException validationException => new ProblemDetails
            {
                Title = "Validation error",
                Status = StatusCodes.Status400BadRequest,
                Detail = validationException.Message,
                Extensions =
                {
                    ["errors"] = validationException
                        .Errors.GroupBy(x => x.PropertyName, x => x.ErrorMessage)
                        .ToDictionary(x => x.Key, x => x.ToList()),
                },
            },
            _ => new ProblemDetails
            {
                Title = "Internal server error.",
                Status = StatusCodes.Status500InternalServerError,
            },
        };
    }
}