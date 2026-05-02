using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using EShop.Web;
using EShop.Web.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configure HttpClient to point to our API Gateway or direct services
var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5050";

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiBaseUrl) });

// Register typed HttpClient for each service
builder.Services.AddHttpClient<ICatalogService, CatalogService>(client =>
    client.BaseAddress = new Uri(builder.Configuration["ApiSettings:CatalogUrl"] ?? "http://localhost:5050"));

builder.Services.AddHttpClient<IBasketService, BasketService>(client =>
    client.BaseAddress = new Uri(builder.Configuration["ApiSettings:BasketUrl"] ?? "http://localhost:6060"));

builder.Services.AddHttpClient<IOrderService, OrderService>(client =>
    client.BaseAddress = new Uri(builder.Configuration["ApiSettings:OrderingUrl"] ?? "http://localhost:5070"));

await builder.Build().RunAsync();
