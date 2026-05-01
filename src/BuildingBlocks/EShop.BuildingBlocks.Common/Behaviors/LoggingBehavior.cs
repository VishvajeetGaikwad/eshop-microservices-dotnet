using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace EShop.BuildingBlocks.Common.Behaviors;

public class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var stopwatch = Stopwatch.StartNew();

        logger.LogInformation("[START] Handle request={RequestName} - Response={ResponseName}",
            requestName, typeof(TResponse).Name);

        var response = await next();

        stopwatch.Stop();
        var elapsed = stopwatch.ElapsedMilliseconds;

        if (elapsed > 3000) // Log warning for slow requests
        {
            logger.LogWarning("[PERFORMANCE] The request {RequestName} took {ElapsedMs} ms",
                requestName, elapsed);
        }

        logger.LogInformation("[END] Handled {RequestName} with {ResponseName} in {ElapsedMs} ms",
            requestName, typeof(TResponse).Name, elapsed);

        return response;
    }
}
