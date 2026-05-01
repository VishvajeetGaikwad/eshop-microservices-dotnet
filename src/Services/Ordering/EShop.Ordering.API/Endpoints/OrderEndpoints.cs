using Carter;
using EShop.Ordering.Application.DTOs;
using EShop.Ordering.Application.Orders.Commands.CreateOrder;
using EShop.Ordering.Application.Orders.Commands.DeleteOrder;
using EShop.Ordering.Application.Orders.Commands.UpdateOrder;
using EShop.Ordering.Application.Orders.Queries.GetOrders;
using EShop.Ordering.Application.Orders.Queries.GetOrdersByCustomer;
using MediatR;

namespace EShop.Ordering.API.Endpoints;

public class OrderEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/ordering/orders");

        group.MapGet("/", async (ISender sender) =>
        {
            var result = await sender.Send(new GetOrdersQuery());
            return Results.Ok(result);
        })
        .WithName("GetOrders")
        .Produces<GetOrdersResult>()
        .WithSummary("Get all orders");

        group.MapGet("/customer/{customerId:guid}", async (Guid customerId, ISender sender) =>
        {
            var result = await sender.Send(new GetOrdersByCustomerQuery(customerId));
            return Results.Ok(result);
        })
        .WithName("GetOrdersByCustomer")
        .Produces<GetOrdersByCustomerResult>()
        .WithSummary("Get orders by customer");

        group.MapPost("/", async (OrderDto orderDto, ISender sender) =>
        {
            var result = await sender.Send(new CreateOrderCommand(orderDto));
            return Results.Created($"/api/v1/ordering/orders/{result.Id}", result);
        })
        .WithName("CreateOrder")
        .Produces<CreateOrderResult>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithSummary("Create a new order");

        group.MapPut("/", async (OrderDto orderDto, ISender sender) =>
        {
            var result = await sender.Send(new UpdateOrderCommand(orderDto));
            return Results.Ok(result);
        })
        .WithName("UpdateOrder")
        .Produces<UpdateOrderResult>()
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithSummary("Update an existing order");

        group.MapDelete("/{orderId:guid}", async (Guid orderId, ISender sender) =>
        {
            var result = await sender.Send(new DeleteOrderCommand(orderId));
            return Results.Ok(result);
        })
        .WithName("DeleteOrder")
        .Produces<DeleteOrderResult>()
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithSummary("Delete an order");
    }
}
