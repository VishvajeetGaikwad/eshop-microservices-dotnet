using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using EShop.Web;
using EShop.Web.Services;
using EShop.Web.Resilience;
using Microsoft.Extensions.Http;
using Polly;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configure HttpClient to point to our API Gateway or direct services
var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5050";

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiBaseUrl) });

// ══════════════════════════════════════════════════════════════
// Register typed HttpClients with Polly resilience policies
// Pattern: Circuit Breaker + Retry with Exponential Backoff
// ══════════════════════════════════════════════════════════════
builder.Services.AddHttpClient<ICatalogService, CatalogService>(client =>
    client.BaseAddress = new Uri(builder.Configuration["ApiSettings:CatalogUrl"] ?? "http://localhost:5050"))
    .AddPolicyHandler(PollyPolicies.GetRetryPolicy())
    .AddPolicyHandler(PollyPolicies.GetCircuitBreakerPolicy());

builder.Services.AddHttpClient<IBasketService, BasketService>(client =>
    client.BaseAddress = new Uri(builder.Configuration["ApiSettings:BasketUrl"] ?? "http://localhost:6060"))
    .AddPolicyHandler(PollyPolicies.GetRetryPolicy())
    .AddPolicyHandler(PollyPolicies.GetCircuitBreakerPolicy());

builder.Services.AddHttpClient<IOrderService, OrderService>(client =>
    client.BaseAddress = new Uri(builder.Configuration["ApiSettings:OrderingUrl"] ?? "http://localhost:5070"))
    .AddPolicyHandler(PollyPolicies.GetRetryPolicy())
    .AddPolicyHandler(PollyPolicies.GetCircuitBreakerPolicy());

// ══════════════════════════════════════════════════════════════
// Register Checkout Saga Orchestrator
// Pattern: Saga with compensating transactions
// ══════════════════════════════════════════════════════════════
builder.Services.AddScoped<ICheckoutSagaOrchestrator, CheckoutSagaOrchestrator>();

await builder.Build().RunAsync();
