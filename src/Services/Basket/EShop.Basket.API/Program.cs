using Carter;
using EShop.Basket.API.Data;
using EShop.BuildingBlocks.Common.Behaviors;
using EShop.BuildingBlocks.Common.Exceptions.Handler;
using EShop.BuildingBlocks.Messaging.Extensions;
using FluentValidation;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

// Add services
var assembly = typeof(Program).Assembly;

builder.Services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(assembly);
    config.AddOpenBehavior(typeof(ValidationBehavior<,>));
    config.AddOpenBehavior(typeof(LoggingBehavior<,>));
});

builder.Services.AddValidatorsFromAssembly(assembly);
builder.Services.AddCarter();

// Cache - use in-memory for local dev, Redis for Docker
var useRedis = builder.Configuration.GetValue<bool>("CacheSettings:UseRedis");
if (useRedis)
{
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = builder.Configuration["CacheSettings:ConnectionString"];
    });
}
else
{
    builder.Services.AddDistributedMemoryCache();
}

builder.Services.AddScoped<IBasketRepository, BasketRepository>();

// Message Broker - use in-memory for local dev, RabbitMQ for Docker
var useRabbitMq = builder.Configuration.GetValue<bool>("MessageBroker:UseRabbitMq");
if (useRabbitMq)
{
    builder.Services.AddMessageBroker(builder.Configuration, assembly);
}
else
{
    builder.Services.AddMassTransit(x => x.UsingInMemory());
}

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
