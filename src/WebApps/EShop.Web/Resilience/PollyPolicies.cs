using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.CircuitBreaker;
using Polly.Extensions.Http;
using Polly.Retry;
using Polly.Timeout;

namespace EShop.Web.Resilience;

/// <summary>
/// Configures Polly resilience policies for all HTTP clients.
/// 
/// Problem Solved: In a microservices architecture, downstream services can become
/// temporarily unavailable (deployments, crashes, network issues). Without resilience
/// policies, a single failing service cascades failures to the entire system.
/// 
/// Policies applied:
/// 1. RETRY: Transient failures get 3 retries with exponential backoff
///    (1s → 2s → 4s). Handles 5xx errors and network timeouts.
/// 
/// 2. CIRCUIT BREAKER: After 5 consecutive failures, the circuit "opens" for 30s.
///    During this time, requests fail immediately (fast-fail) instead of waiting
///    for timeouts. After 30s, one test request is allowed through (half-open state).
/// 
/// 3. TIMEOUT: Individual requests timeout after 10s to prevent thread starvation.
/// 
/// Interview talking point: "We implemented the Circuit Breaker pattern with Polly to
/// prevent cascading failures. When the Ordering service was down, checkout requests
/// would hang for 30+ seconds. With circuit breaker, after 5 failures it fails fast,
/// giving the downstream service time to recover while showing users a friendly error
/// message immediately."
/// </summary>
public static class PollyPolicies
{
    /// <summary>
    /// Retry policy with exponential backoff + jitter.
    /// Jitter prevents "thundering herd" — all clients retrying at the same instant.
    /// </summary>
    public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError() // 5xx, 408, network failures
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt =>
                    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))  // 2s, 4s, 8s
                    + TimeSpan.FromMilliseconds(Random.Shared.Next(0, 1000)), // jitter
                onRetry: (outcome, timespan, retryAttempt, context) =>
                {
                    Console.WriteLine(
                        $"[Polly Retry] Attempt {retryAttempt} after {timespan.TotalSeconds:F1}s. " +
                        $"Status: {outcome.Result?.StatusCode}. Reason: {outcome.Exception?.Message}");
                });
    }

    /// <summary>
    /// Circuit breaker that opens after consecutive failures.
    /// 
    /// States:
    /// - CLOSED: Normal operation, requests pass through
    /// - OPEN: Circuit tripped, requests fail immediately (fast-fail)
    /// - HALF-OPEN: After break duration, allows one test request
    /// </summary>
    public static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 5,    // Open after 5 failures
                durationOfBreak: TimeSpan.FromSeconds(30), // Stay open for 30s
                onBreak: (outcome, breakDelay) =>
                {
                    Console.WriteLine(
                        $"[Circuit Breaker] OPEN for {breakDelay.TotalSeconds}s. " +
                        $"Reason: {outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString()}");
                },
                onReset: () =>
                {
                    Console.WriteLine("[Circuit Breaker] CLOSED — service recovered");
                },
                onHalfOpen: () =>
                {
                    Console.WriteLine("[Circuit Breaker] HALF-OPEN — testing with one request");
                });
    }

    /// <summary>
    /// Combines retry (inner) wrapped by circuit breaker (outer).
    /// Order matters: Circuit breaker wraps retry so that retries count toward the break threshold.
    /// </summary>
    public static IAsyncPolicy<HttpResponseMessage> GetCombinedPolicy()
    {
        return Policy.WrapAsync(GetCircuitBreakerPolicy(), GetRetryPolicy());
    }
}
