using Carter;
using EShop.BuildingBlocks.Common.Exceptions.Handler;
using EShop.Ordering.Application.Orders.Commands.CreateOrder;
using EShop.Ordering.Infrastructure.Extensions;
using FluentValidation;
using MassTransit;
using MediatR;

var builder = WebApplication.CreateBuilder(args);

// Add Application services
var applicationAssembly = typeof(CreateOrderCommand).Assembly;
var apiAssembly = typeof(Program).Assembly;

builder.Services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(applicationAssembly);
});

builder.Services.AddValidatorsFromAssembly(applicationAssembly);
builder.Services.AddCarter();

// Add Infrastructure
builder.Services.AddInfrastructure(builder.Configuration);

// Message Broker - use in-memory for local dev
var useRabbitMq = builder.Configuration.GetValue<bool>("MessageBroker:UseRabbitMq");
if (useRabbitMq)
{
    builder.Services.AddMassTransit(config =>
    {
        config.SetKebabCaseEndpointNameFormatter();
        config.AddConsumers(apiAssembly);

        config.UsingRabbitMq((context, cfg) =>
        {
            cfg.Host(new Uri(builder.Configuration["MessageBroker:Host"]!), host =>
            {
                host.Username(builder.Configuration["MessageBroker:UserName"] ?? "guest");
                host.Password(builder.Configuration["MessageBroker:Password"] ?? "guest");
            });
            cfg.ConfigureEndpoints(context);
        });
    });
}
else
{
    builder.Services.AddMassTransit(config =>
    {
        config.AddConsumers(apiAssembly);
        config.UsingInMemory((context, cfg) =>
        {
            cfg.ConfigureEndpoints(context);
        });
    });
}

// Idempotency — prevents duplicate order creation from retries/double-clicks
builder.Services.AddSingleton<EShop.Ordering.API.Idempotency.IIdempotencyService, 
    EShop.Ordering.API.Idempotency.IdempotencyService>();

builder.Services.AddExceptionHandler<CustomExceptionHandler>();
builder.Services.AddHealthChecks();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure pipeline
app.UseCors();
app.UseExceptionHandler(options => { });

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapCarter();
app.MapHealthChecks("/health");

app.Run();
