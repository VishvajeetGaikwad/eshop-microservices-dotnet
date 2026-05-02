using System.Collections.Concurrent;

namespace EShop.Ordering.API.Idempotency;

/// <summary>
/// Provides idempotency for API operations using an in-memory store.
/// In production, this would use Redis/distributed cache for multi-instance deployments.
/// 
/// Problem Solved: Users double-clicking "Place Order" or network retries sending
/// the same request twice would create duplicate orders without this protection.
/// </summary>
public interface IIdempotencyService
{
    bool TryGetCachedResponse(string key, out IdempotentResponse? response);
    void CacheResponse(string key, IdempotentResponse response);
    bool IsProcessing(string key);
    void MarkProcessing(string key);
    void RemoveProcessing(string key);
}

public class IdempotencyService : IIdempotencyService
{
    // In production: Use IDistributedCache (Redis) with TTL
    private readonly ConcurrentDictionary<string, IdempotentResponse> _cache = new();
    private readonly ConcurrentDictionary<string, byte> _processing = new();

    public bool TryGetCachedResponse(string key, out IdempotentResponse? response)
    {
        return _cache.TryGetValue(key, out response);
    }

    public void CacheResponse(string key, IdempotentResponse response)
    {
        _cache.TryAdd(key, response);

        // Auto-expire after 24 hours (simulated with a background task in production)
        _ = Task.Delay(TimeSpan.FromHours(24)).ContinueWith(_ =>
        {
            _cache.TryRemove(key, out IdempotentResponse? _);
        });
    }

    public bool IsProcessing(string key)
    {
        return _processing.ContainsKey(key);
    }

    public void MarkProcessing(string key)
    {
        _processing.TryAdd(key, 0);
    }

    public void RemoveProcessing(string key)
    {
        _processing.TryRemove(key, out _);
    }
}

public record IdempotentResponse(int StatusCode, object? Body, DateTime CreatedAt);
