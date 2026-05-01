using EShop.Ordering.Application.Abstractions;
using EShop.Ordering.Infrastructure.Data;
using EShop.Ordering.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EShop.Ordering.Infrastructure.Extensions;

public static class InfrastructureExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<OrderDbContext>(options =>
        {
            options.UseInMemoryDatabase("OrderDb");
        });

        services.AddScoped<IOrderRepository, OrderRepository>();

        return services;
    }
}
