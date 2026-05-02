# Resilience Patterns

This document covers the three production resilience patterns implemented in Phase 4, with code examples, sequence diagrams, and interview talking points.

---

## 1. Saga Pattern (Orchestration)

### The Problem

Checkout involves multiple services:
1. Create an order in the Ordering service
2. Clear the basket in the Basket service
3. Confirm success to the user

If step 2 fails after step 1 succeeds, we have an **inconsistent state** — an order exists but the basket wasn't cleared. In a monolith, a database transaction would handle this. In microservices, we need a distributed coordination pattern.

### The Solution — Saga Orchestrator

A **Saga** is a sequence of local transactions where each step has a **compensating action** that undoes it on failure:

```
┌─────────────┐     ┌─────────────┐     ┌─────────────┐
│  Step 1:    │     │  Step 2:    │     │  Step 3:    │
│ Create Order│────▶│ Clear Basket│────▶│  Confirm    │
│             │     │             │     │  Success    │
└──────┬──────┘     └──────┬──────┘     └─────────────┘
       │                   │
       ▼                   ▼
┌─────────────┐     ┌─────────────┐
│ Compensate: │     │ Compensate: │
│ Delete Order│     │   (none)    │
└─────────────┘     └─────────────┘
```

### Implementation

```csharp
// CheckoutSagaOrchestrator.cs
public class CheckoutSagaOrchestrator
{
    public async Task<SagaResult> ExecuteCheckoutSaga(CheckoutRequest request)
    {
        var result = new SagaResult();
        Guid? createdOrderId = null;

        try
        {
            // Step 1: Create Order
            result.Steps.Add(new SagaStep("Creating order..."));
            createdOrderId = await _orderService.CreateOrder(orderDto);
            result.Steps.Add(new SagaStep("✓ Order created successfully"));

            // Step 2: Clear Basket
            result.Steps.Add(new SagaStep("Clearing basket..."));
            await _basketService.DeleteBasket(request.UserName);
            result.Steps.Add(new SagaStep("✓ Basket cleared"));

            // Step 3: Success
            result.Success = true;
            result.Steps.Add(new SagaStep("✓ Checkout complete!"));
        }
        catch (Exception ex)
        {
            // COMPENSATION: Undo completed steps
            result.Steps.Add(new SagaStep($"✗ Failed: {ex.Message}"));

            if (createdOrderId.HasValue)
            {
                result.Steps.Add(new SagaStep("↩ Compensating: Deleting order..."));
                await _orderService.DeleteOrder(createdOrderId.Value);
                result.Steps.Add(new SagaStep("✓ Order rolled back"));
            }

            result.Success = false;
        }

        return result;
    }
}
```

### User Experience

The Checkout page shows real-time saga steps:

```
✓ Creating order...
✓ Order created successfully
✓ Clearing basket...
✓ Basket cleared
✓ Checkout complete!

→ Order Placed Successfully!
```

On failure:
```
✓ Creating order...
✓ Order created successfully
✗ Failed: Basket service unavailable
↩ Compensating: Deleting order...
✓ Order rolled back

→ Checkout failed. Please try again.
```

### Interview Talking Points

> **Q: Why Orchestration over Choreography?**
>
> Orchestration gives us a single coordinator that knows the full saga state. Choreography (event-driven) is more decoupled but harder to debug — you can't look at one place to understand what happened. For a checkout flow with clear sequential steps, orchestration is more maintainable.

> **Q: What happens if the compensation itself fails?**
>
> In production, you'd persist saga state and use a background job to retry failed compensations. The orchestrator could also dead-letter the event for manual review. Our implementation logs the failure and surfaces it to the user.

> **Q: How would this work at scale?**
>
> At scale, you'd use a durable saga framework (MassTransit Sagas, NServiceBus Sagas, or Temporal.io) that persists saga state to a database and handles recovery automatically after process restarts.

---

## 2. Idempotency Keys

### The Problem

Network issues cause retries. If a user clicks "Place Order" and the response times out, the client retries — potentially creating **duplicate orders**. The Polly retry policy makes this even more likely since it automatically retries failed HTTP calls.

```
Client ──POST /orders──▶ Server (creates order #1)
       ◀──timeout─── (response lost)
Client ──POST /orders──▶ Server (creates order #2 ← DUPLICATE!)
```

### The Solution — Idempotency Key Header

The client generates a unique key per logical operation. The server checks if it's already processed that key:

```
Client ──POST /orders + Idempotency-Key: abc123──▶ Server
       ◀──201 Created─── (stores key → order mapping)
Client ──POST /orders + Idempotency-Key: abc123──▶ Server
       ◀──200 OK (cached response)─── (same key = return previous result)
```

### Implementation

**Server-side filter:**

```csharp
// IdempotencyFilter.cs — IEndpointFilter
public class IdempotencyFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        var httpContext = context.HttpContext;
        var idempotencyKey = httpContext.Request.Headers["Idempotency-Key"].FirstOrDefault();

        if (string.IsNullOrEmpty(idempotencyKey))
            return await next(context);  // No key = no dedup

        // Check if already processed
        if (_idempotencyService.TryGetResponse(idempotencyKey, out var cached))
            return Results.Ok(cached);   // Return cached result

        // Execute the handler
        var result = await next(context);

        // Cache the result for future duplicates
        _idempotencyService.StoreResponse(idempotencyKey, result);
        return result;
    }
}
```

**Idempotency store:**

```csharp
// IdempotencyService.cs
public class IdempotencyService
{
    private readonly ConcurrentDictionary<string, object?> _cache = new();

    public bool TryGetResponse(string key, out object? response)
        => _cache.TryGetValue(key, out response);

    public void StoreResponse(string key, object? response)
        => _cache.TryAdd(key, response);
}
```

**Client-side (Saga Orchestrator):**

```csharp
// Generate unique key per checkout attempt
var idempotencyKey = Guid.NewGuid().ToString();

var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/ordering/orders");
request.Headers.Add("Idempotency-Key", idempotencyKey);
request.Content = JsonContent.Create(orderDto);

// Even if Polly retries this 3 times, only one order is created
var response = await _httpClient.SendAsync(request);
```

**Endpoint registration:**

```csharp
app.MapPost("/api/v1/ordering/orders", async (CreateOrderDto dto, ISender sender) =>
{
    var command = dto.Adapt<CreateOrderCommand>();
    var result = await sender.Send(command);
    return Results.Created($"/api/v1/ordering/orders/{result.Id}", result);
})
.AddEndpointFilter<IdempotencyFilter>();  // ← Idempotency applied here
```

### Interview Talking Points

> **Q: Why not use database unique constraints instead?**
>
> Unique constraints catch duplicates but return errors to the client. An idempotency key returns the **same successful response** — the client doesn't need special error handling. It's a better UX and works with automatic retry policies transparently.

> **Q: How would you handle key expiration in production?**
>
> In production, use Redis with TTL (e.g., 24 hours). After expiry, the same key would be treated as a new request. For our demo, ConcurrentDictionary works in-process. At scale, you'd use a distributed cache shared across service instances.

> **Q: What happens with concurrent requests with the same key?**
>
> `ConcurrentDictionary.TryAdd` is atomic — only the first request processes the handler. Subsequent concurrent requests would need a slight delay/retry to pick up the cached result. In production, Redis `SET NX` (set if not exists) provides the same atomic guarantee across instances.

---

## 3. Circuit Breaker + Retry with Exponential Backoff

### The Problem

When a downstream service is down, continuing to send requests:
1. Wastes resources on calls that will fail
2. Can cascade failures (thundering herd on recovery)
3. Increases latency for the end user waiting on timeouts

### The Solution — Polly Resilience Policies

Two complementary policies work together:

**Retry** — Automatically retry transient failures with increasing delays:
```
Attempt 1: Fail → wait 1s
Attempt 2: Fail → wait 2s + random jitter
Attempt 3: Fail → give up, throw exception
```

**Circuit Breaker** — Stop calling a failing service entirely:
```
CLOSED (normal) ──5 failures──▶ OPEN (reject all calls for 30s)
       ▲                              │
       │                              ▼ (after 30s)
       └───success───── HALF-OPEN (allow one test call)
```

### Implementation

```csharp
// PollyPolicies.cs
public static class PollyPolicies
{
    public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .Or<HttpRequestException>()  // Handle WASM connection failures
            .WaitAndRetryAsync(
                retryCount: 2,
                sleepDurationProvider: retryAttempt =>
                    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))  // 2s, 4s
                    + TimeSpan.FromMilliseconds(Random.Shared.Next(0, 1000)),  // Jitter
                onRetry: (outcome, timespan, retryAttempt, context) =>
                {
                    Console.WriteLine(
                        $"[Retry] Attempt {retryAttempt} after {timespan.TotalSeconds:F1}s");
                });
    }

    public static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .Or<HttpRequestException>()
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 5,  // 5 failures → open
                durationOfBreak: TimeSpan.FromSeconds(30),  // Stay open 30s
                onBreak: (outcome, timespan) =>
                    Console.WriteLine($"[Circuit OPEN] Breaking for {timespan.TotalSeconds}s"),
                onReset: () =>
                    Console.WriteLine("[Circuit CLOSED] Recovered"),
                onHalfOpen: () =>
                    Console.WriteLine("[Circuit HALF-OPEN] Testing..."));
    }
}
```

**Registration with HttpClientFactory:**

```csharp
// Program.cs — Each service gets both policies
builder.Services.AddHttpClient<CatalogService>(client =>
    client.BaseAddress = new Uri("http://localhost:5050"))
    .AddPolicyHandler(PollyPolicies.GetRetryPolicy())
    .AddPolicyHandler(PollyPolicies.GetCircuitBreakerPolicy());
```

### How They Work Together

```
Request ──▶ [Retry Policy] ──▶ [Circuit Breaker] ──▶ HTTP Call
                │                       │
                │                       ├── CLOSED: Allow call
                │                       ├── OPEN: Fail immediately (no HTTP call)
                │                       └── HALF-OPEN: Allow one test call
                │
                └── On failure: Wait (exponential + jitter) → Retry
                    After max retries: Propagate exception to caller
```

The **jitter** prevents a "thundering herd" — when a service recovers, clients don't all retry at the same millisecond because each has a random delay added.

### Interview Talking Points

> **Q: Why exponential backoff instead of fixed delay?**
>
> Exponential backoff gives failing services time to recover. If a service is overloaded, hammering it every 1 second makes things worse. Waiting 2s, then 4s, then 8s gives exponentially more breathing room. The jitter ensures multiple clients don't synchronize their retries.

> **Q: What's the relationship between retry and circuit breaker?**
>
> Retry wraps the circuit breaker. When the circuit is OPEN, retries fail immediately (no actual HTTP call is made), saving network resources. When CLOSED, retries go through but each failure counts toward the circuit breaker threshold. They're complementary — retry handles transient blips, circuit breaker handles sustained outages.

> **Q: How would you monitor circuit breaker state in production?**
>
> Emit metrics on state transitions (OpenTelemetry, Prometheus). Alert on OPEN state. Include circuit state in health check responses. Use Polly's `onBreak`/`onReset` callbacks to push to your observability stack. Dashboard showing which circuits are open tells you exactly which dependencies are degraded.

> **Q: Why `.Or<HttpRequestException>()` in addition to `HandleTransientHttpError()`?**
>
> In Blazor WebAssembly (browser), connection failures throw `HttpRequestException` rather than returning HTTP 5xx status codes (the request never reaches the server). Without this, the policies wouldn't trigger for network-level failures in WASM.

---

## Pattern Interaction Diagram

```
User clicks "Place Order"
         │
         ▼
┌─────────────────────────────────┐
│    Saga Orchestrator            │
│                                 │
│  Step 1: Create Order           │
│    │                            │
│    ▼                            │
│  ┌───────────────────────────┐  │
│  │ Generate Idempotency Key  │  │  ← Prevents duplicates on retry
│  └───────────┬───────────────┘  │
│              ▼                  │
│  ┌───────────────────────────┐  │
│  │ HTTP POST with Key header │──┼──▶ [Retry Policy (2 attempts)]
│  └───────────────────────────┘  │         │
│                                 │         ▼
│  Step 2: Clear Basket           │    [Circuit Breaker]
│    │                            │         │
│    ▼                            │         ▼
│  ┌───────────────────────────┐  │    Ordering API
│  │ HTTP DELETE basket         │──┼──▶ [IdempotencyFilter]
│  └───────────────────────────┘  │         │
│                                 │         ▼
│  On failure → Compensate        │    Create Order (once)
│    (delete order, notify user)  │
└─────────────────────────────────┘
```

All three patterns work in concert:
1. **Saga** orchestrates the multi-step flow and handles compensation
2. **Idempotency** ensures retries don't create duplicates
3. **Circuit Breaker** prevents cascading failures and gives services recovery time

---

## Summary Table

| Pattern | Problem Solved | Implementation | Production Enhancement |
|---------|---------------|----------------|----------------------|
| Saga | Distributed transaction consistency | `CheckoutSagaOrchestrator.cs` | Durable saga state (MassTransit/Temporal) |
| Idempotency | Duplicate operations from retries | `IdempotencyFilter.cs` + header key | Redis with TTL across instances |
| Circuit Breaker | Cascading failures from down services | `PollyPolicies.cs` + HttpClientFactory | OpenTelemetry metrics + alerting |
