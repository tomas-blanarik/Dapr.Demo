using Dapr.Core.Repositories;
using Dapr.Ordering.Api.Entities.Domain;
using Dapr.Workflow;

namespace Dapr.Ordering.Api.Activities;

public record UpdateOrderRequest(Guid OrderId, Guid PaymentId);

public class UpdateOrderWithPaymentActivity(IGenericWriteRepository<Order> repository, ILogger<UpdateOrderWithPaymentActivity> logger) : WorkflowActivity<UpdateOrderRequest, object>
{
    private readonly IGenericWriteRepository<Order> _repository = repository;
    private readonly ILogger<UpdateOrderWithPaymentActivity> _logger = logger;

    public override async Task<object> RunAsync(WorkflowActivityContext context, UpdateOrderRequest input)
    {
        await _repository.UpdateAsync(input.OrderId, order => order.PaymentId = input.PaymentId);
        _logger.LogInformation("Order with ID: {OrderId} successfully updated with PaymentId: {PaymentId}", input.OrderId, input.PaymentId);

        return null!;
    }
}