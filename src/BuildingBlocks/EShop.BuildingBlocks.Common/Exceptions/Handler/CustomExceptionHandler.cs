using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace EShop.BuildingBlocks.Common.Exceptions.Handler;

public class CustomExceptionHandler(ILogger<CustomExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Exception occurred: {Message}", exception.Message);

        var (statusCode, title, detail) = exception switch
        {
            BadRequestException badRequest => (StatusCodes.Status400BadRequest, badRequest.Message, badRequest.Details),
            NotFoundException notFound => (StatusCodes.Status404NotFound, notFound.Message, null),
            InternalServerException internalServer => (StatusCodes.Status500InternalServerError, internalServer.Message, internalServer.Details),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred.", exception.Message)
        };

        var problemDetails = new ProblemDetails
        {
            Title = title,
            Detail = detail,
            Status = statusCode,
            Instance = context.Request.Path
        };

        problemDetails.Extensions.Add("traceId", context.TraceIdentifier);

        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
