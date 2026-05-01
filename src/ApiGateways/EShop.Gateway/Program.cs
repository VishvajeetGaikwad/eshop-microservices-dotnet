var builder = WebApplication.CreateBuilder(args);

// Add YARP reverse proxy
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddHealthChecks();

var app = builder.Build();

app.MapReverseProxy();

app.MapHealthChecks("/health");

app.MapGet("/", () => Results.Ok(new
{
    Service = "EShop API Gateway",
    Status = "Running",
    Timestamp = DateTime.UtcNow
}));

app.Run();
