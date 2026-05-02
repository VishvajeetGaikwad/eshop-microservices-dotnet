namespace EShop.Ordering.API.Idempotency;

/// <summary>
/// Endpoint filter that enforces idempotency on POST operations.
/// 
/// How it works:
/// 1. Client sends "Idempotency-Key" header (typically a GUID generated client-side)
/// 2. First request: processes normally, caches response with the key
/// 3. Duplicate request (same key): returns cached response immediately (HTTP 200)
/// 4. Concurrent duplicate: returns 409 Conflict while first request is still processing
/// 
/// Interview talking point: "We solved the double-submit problem by implementing 
/// idempotency keys at the API level. The client generates a unique key per user action,
/// and the server guarantees at-most-once execution regardless of retries."
/// </summary>
public class IdempotencyFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        var httpContext = context.HttpContext;
        var idempotencyService = httpContext.RequestServices.GetRequiredService<IIdempotencyService>();

        // Check for Idempotency-Key header
        if (!httpContext.Request.Headers.TryGetValue("Idempotency-Key", out var idempotencyKey) ||
            string.IsNullOrWhiteSpace(idempotencyKey))
        {
            // No idempotency key — proceed without idempotency protection
            return await next(context);
        }

        var key = idempotencyKey.ToString();

        // Check if we already have a cached response for this key
        if (idempotencyService.TryGetCachedResponse(key, out var cachedResponse))
        {
            // Return cached response — this is a duplicate request
            httpContext.Response.Headers["X-Idempotency-Status"] = "cached";
            httpContext.Response.Headers["X-Idempotency-Key"] = key;
            return Results.Json(cachedResponse!.Body, statusCode: cachedResponse.StatusCode);
        }

        // Check if another request with the same key is currently being processed
        if (idempotencyService.IsProcessing(key))
        {
            // Concurrent duplicate — reject with 409 Conflict
            return Results.Conflict(new
            {
                Error = "A request with this idempotency key is currently being processed.",
                IdempotencyKey = key
            });
        }

        // Mark as processing to prevent concurrent duplicates
        idempotencyService.MarkProcessing(key);

        try
        {
            // Execute the actual endpoint
            var result = await next(context);

            // Cache the response
            var response = new IdempotentResponse(
                StatusCode: 201,
                Body: result,
                CreatedAt: DateTime.UtcNow);

            idempotencyService.CacheResponse(key, response);

            // Add idempotency headers to response
            httpContext.Response.Headers["X-Idempotency-Status"] = "created";
            httpContext.Response.Headers["X-Idempotency-Key"] = key;

            return result;
        }
        finally
        {
            idempotencyService.RemoveProcessing(key);
        }
    }
}
