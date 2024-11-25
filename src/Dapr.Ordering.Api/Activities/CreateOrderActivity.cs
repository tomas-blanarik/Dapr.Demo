using Dapr.Core.Repositories.Generic;
using Dapr.Ordering.Api.Entities.Domain;
using Dapr.Ordering.Api.Entities.DTO;
using Dapr.Ordering.Api.Entities.Events;
using Dapr.Workflow;

namespace Dapr.Ordering.Api.Activities
{
    public record OrderRequest(UserCheckoutAcceptedIntegrationEvent Event);
    public record OrderResponse(OrderDTO DTO);

    public class CreateOrderActivity(GenericRepository<Order> repository, ILogger<CreateOrderActivity> logger) : WorkflowActivity<OrderRequest, OrderResponse?>
    {
        private readonly GenericRepository<Order> _repository = repository;
        private readonly ILogger<CreateOrderActivity> _logger = logger;

        public override async Task<OrderResponse?> RunAsync(WorkflowActivityContext context, OrderRequest input)
        {
            Order entity = await _repository.CreateAsync(input.Event.ToDomain);
            _logger.LogInformation("Order successfully created with ID: {OrderId}", entity.EntityId);
            return new OrderResponse(entity.ToDTO());
        }
    }
}