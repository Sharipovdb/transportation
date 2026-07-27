using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Transportation.Mediator.Helper.Behaviors;

public sealed class LoggingBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        _logger.LogInformation("Handling request: {RequestName}", requestName);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var response = await next(cancellationToken);
            return response;
        }
        finally
        {
            stopwatch.Stop();

            _logger.LogInformation(
                "Completed request: {RequestName} in {ElapsedMs} ms",
                requestName,
                stopwatch.ElapsedMilliseconds
            );
        }
    }
}