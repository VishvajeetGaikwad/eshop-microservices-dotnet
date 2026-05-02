using EShop.Web.Models;

namespace EShop.Web.Services;

/// <summary>
/// Implements the Saga Pattern for the checkout workflow.
/// 
/// Problem Solved: In a distributed system, checkout involves multiple services
/// (Ordering + Basket). If order creation succeeds but basket clearing fails,
/// or vice versa, we end up in an inconsistent state.
/// 
/// Solution: Orchestration-based Saga that executes steps in sequence and
/// performs compensating actions if any step fails.
/// 
/// Saga Steps:
/// 1. Create Order (via Ordering API)
/// 2. Clear Basket (via Basket API)  
/// 
/// Compensation:
/// - If Step 2 fails → Delete the order created in Step 1 (rollback)
/// 
/// Interview talking point: "We implemented the Saga pattern to handle distributed
/// transactions across microservices. Since we can't use traditional ACID transactions
/// spanning multiple databases, we use compensating actions to maintain eventual consistency."
/// </summary>
public interface ICheckoutSagaOrchestrator
{
    Task<CheckoutSagaResult> ExecuteAsync(CheckoutSagaContext context);
}

public class CheckoutSagaOrchestrator(
    IOrderService orderService,
    IBasketService basketService,
    ILogger<CheckoutSagaOrchestrator> logger) : ICheckoutSagaOrchestrator
{
    public async Task<CheckoutSagaResult> ExecuteAsync(CheckoutSagaContext context)
    {
        var result = new CheckoutSagaResult();
        Guid? createdOrderId = null;

        try
        {
            // ═══════════════════════════════════════════════
            // STEP 1: Create Order
            // ═══════════════════════════════════════════════
            logger.LogInformation("Saga Step 1: Creating order for customer {CustomerId}",
                context.Order.CustomerId);

            result.Steps.Add(new SagaStep("CreateOrder", SagaStepStatus.InProgress));

            createdOrderId = await orderService.CreateOrder(context.Order, context.IdempotencyKey);

            if (!createdOrderId.HasValue)
            {
                result.Steps.Last().Status = SagaStepStatus.Failed;
                result.Steps.Last().Error = "Ordering service returned failure";
                result.IsSuccess = false;
                result.ErrorMessage = "Failed to create order. Please try again.";
                logger.LogError("Saga Step 1 FAILED: Could not create order");
                return result;
            }

            result.Steps.Last().Status = SagaStepStatus.Completed;
            result.OrderId = createdOrderId.Value;
            logger.LogInformation("Saga Step 1 COMPLETED: Order {OrderId} created", createdOrderId);

            // ═══════════════════════════════════════════════
            // STEP 2: Clear Basket
            // ═══════════════════════════════════════════════
            logger.LogInformation("Saga Step 2: Clearing basket for user {UserName}",
                context.UserName);

            result.Steps.Add(new SagaStep("ClearBasket", SagaStepStatus.InProgress));

            var checkoutSuccess = await basketService.Checkout(context.CheckoutRequest);

            if (!checkoutSuccess)
            {
                result.Steps.Last().Status = SagaStepStatus.Failed;
                result.Steps.Last().Error = "Basket service returned failure";
                logger.LogWarning("Saga Step 2 FAILED: Could not clear basket. Initiating compensation...");

                // ═══════════════════════════════════════════════
                // COMPENSATION: Delete the order created in Step 1
                // ═══════════════════════════════════════════════
                result.Steps.Add(new SagaStep("CompensateDeleteOrder", SagaStepStatus.InProgress));

                var compensated = await orderService.DeleteOrder(createdOrderId.Value);

                if (compensated)
                {
                    result.Steps.Last().Status = SagaStepStatus.Completed;
                    logger.LogInformation("Compensation COMPLETED: Order {OrderId} deleted", createdOrderId);
                }
                else
                {
                    result.Steps.Last().Status = SagaStepStatus.Failed;
                    result.Steps.Last().Error = "Compensation failed — manual intervention required";
                    logger.LogCritical(
                        "COMPENSATION FAILED: Order {OrderId} exists but basket not cleared. " +
                        "Manual intervention required!", createdOrderId);
                }

                result.IsSuccess = false;
                result.ErrorMessage = "Checkout failed during basket clearing. Order has been rolled back.";
                return result;
            }

            result.Steps.Last().Status = SagaStepStatus.Completed;
            logger.LogInformation("Saga Step 2 COMPLETED: Basket cleared for {UserName}", context.UserName);

            // ═══════════════════════════════════════════════
            // SAGA COMPLETED SUCCESSFULLY
            // ═══════════════════════════════════════════════
            result.IsSuccess = true;
            logger.LogInformation("Checkout Saga COMPLETED successfully. OrderId: {OrderId}", createdOrderId);
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Checkout Saga encountered an unexpected error");

            // If we created an order, try to compensate
            if (createdOrderId.HasValue)
            {
                logger.LogWarning("Attempting compensation: Deleting order {OrderId}", createdOrderId);
                result.Steps.Add(new SagaStep("CompensateDeleteOrder", SagaStepStatus.InProgress));

                try
                {
                    await orderService.DeleteOrder(createdOrderId.Value);
                    result.Steps.Last().Status = SagaStepStatus.Completed;
                }
                catch
                {
                    result.Steps.Last().Status = SagaStepStatus.Failed;
                    result.Steps.Last().Error = "Compensation also failed";
                }
            }

            result.IsSuccess = false;
            result.ErrorMessage = $"An unexpected error occurred: {ex.Message}";
            return result;
        }
    }
}

// ══════════════════════════════════════════════════
// Saga Context & Result Models
// ══════════════════════════════════════════════════

public class CheckoutSagaContext
{
    public required string UserName { get; set; }
    public required OrderModel Order { get; set; }
    public required BasketCheckoutRequest CheckoutRequest { get; set; }
    public string IdempotencyKey { get; set; } = Guid.NewGuid().ToString();
}

public class CheckoutSagaResult
{
    public bool IsSuccess { get; set; }
    public Guid? OrderId { get; set; }
    public string? ErrorMessage { get; set; }
    public List<SagaStep> Steps { get; set; } = [];
}

public class SagaStep
{
    public string Name { get; set; }
    public SagaStepStatus Status { get; set; }
    public string? Error { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public SagaStep(string name, SagaStepStatus status)
    {
        Name = name;
        Status = status;
    }
}

public enum SagaStepStatus
{
    InProgress,
    Completed,
    Failed
}
