using Dapr.Client;
using Dapr.Core;
using Dapr.Core.Events.Orders;
using Dapr.Core.Events.Payments;
using Dapr.Core.Events.Users;
using Dapr.Core.Exceptions;
using Dapr.Core.Extensions;
using Dapr.Core.Repositories;
using Dapr.Core.Repositories.Generic;
using Dapr.Ordering.Api.Entities.Domain;
using Dapr.Ordering.Api.Entities.DTO;
using Dapr.Ordering.Api.Entities.Enums;
using Dapr.Ordering.Api.Entities.Events;
using Dapr.Ordering.Api.Workflows;
using Dapr.Core.Events.Users;
using Dapr.Workflow;
using Microsoft.AspNetCore.Mvc;

namespace Dapr.Ordering.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class EventsController : ControllerBase
{
    [HttpPost(nameof(PaymentCreatedIntegrationEvent))]
    [Topic(DaprConstants.Components.PubSub, nameof(PaymentCreatedIntegrationEvent))]
    public async Task HandleAsync(PaymentCreatedIntegrationEvent @event,
                                  [FromServices] IGenericWriteRepository<Order> repository,
                                  CancellationToken ct)
    {
        await repository.UpdateAsync(@event.OrderId, toUpdate =>
        {
            toUpdate.PaymentId = @event.PaymentId;
        }, ct);
    }

    [HttpPost(nameof(PaymentCompletedIntegrationEvent))]
    [Topic(DaprConstants.Components.PubSub, nameof(PaymentCompletedIntegrationEvent))]
    public async Task HandleAsync(PaymentCompletedIntegrationEvent @event,
                                  [FromServices] GenericRepository<Order> repository,
                                  [FromServices] DaprClient dapr,
                                  CancellationToken ct)
    {
        Order? entity = ((await repository.GetAllAsync(x => x.PaymentId == @event.PaymentId, ct))?.SingleOrDefault())
            ?? throw new UnexpectedErrorException(string.Format("Order with payment ID '{0}' not found", @event.PaymentId));

        entity = await repository.UpdateAsync(entity.EntityId!.Value, toUpdate =>
        {
            if (toUpdate.Status == OrderStatus.Completed || toUpdate.Status == OrderStatus.Failed)
            {
                throw new ConflictException(string.Format("Order is already {0}", toUpdate.Status.ToString()));
            }

            toUpdate.Status = OrderStatus.Completed;
            toUpdate.CompletedDate = DateTime.UtcNow;
        }, ct);

        var completedEvent = new OrderCompletedIntegrationEvent
        {
            CompletedDate = entity.CompletedDate!.Value,
            OrderId = entity.EntityId!.Value,
            UserId = entity.UserId!.Value,
        };

        await dapr.PublishEventAsync(DaprConstants.Components.PubSub,
                                     nameof(OrderCompletedIntegrationEvent),
                                     completedEvent,
                                     ct);
    }

    [HttpPost(nameof(PaymentFailedIntegrationEvent))]
    [Topic(DaprConstants.Components.PubSub, nameof(PaymentFailedIntegrationEvent))]
    public async Task HandleAsync(PaymentFailedIntegrationEvent @event,
                                  [FromServices] GenericRepository<Order> repository,
                                  [FromServices] DaprClient dapr,
                                  CancellationToken ct)
    {
        Order? entity = ((await repository.GetAllAsync(x => x.PaymentId == @event.PaymentId, ct))?.SingleOrDefault())
            ?? throw new UnexpectedErrorException(string.Format("Order with payment ID '{0}' not found", @event.PaymentId));

        await repository.UpdateAsync(entity.EntityId!.Value, toUpdate =>
        {
            if (toUpdate.Status == OrderStatus.Completed || toUpdate.Status == OrderStatus.Failed)
            {
                throw new ConflictException(string.Format("Order is already {0}", toUpdate.Status.ToString()));
            }

            toUpdate.Status = OrderStatus.Failed;
            toUpdate.CompletedDate = null;
            toUpdate.Error = @event.Reason;
        }, ct);

        var failedEvent = new OrderFailedIntegrationEvent
        {
            FailedDate = DateTime.UtcNow,
            OrderId = entity.EntityId!.Value,
            UserId = entity.UserId!.Value,
            Reason = @event.Reason
        };

        await dapr.PublishEventAsync(DaprConstants.Components.PubSub,
                                     nameof(OrderFailedIntegrationEvent),
                                     failedEvent,
                                     ct);
    }

    [HttpPost(nameof(PaymentDeletedIntegrationEvent))]
    [Topic(DaprConstants.Components.PubSub, nameof(PaymentDeletedIntegrationEvent))]
    public async Task HandleAsync(PaymentDeletedIntegrationEvent @event,
                                  [FromServices] GenericRepository<Order> repository,
                                  CancellationToken ct)
    {
        Order? entity = ((await repository.GetAllAsync(x => x.PaymentId == @event.PaymentId, ct))?.SingleOrDefault())
            ?? throw new UnexpectedErrorException(string.Format("Order with payment ID '{0}' not found", @event.PaymentId));

        await repository.UpdateAsync(entity.EntityId!.Value, toUpdate =>
        {
            if (toUpdate.Status == OrderStatus.Completed || toUpdate.Status == OrderStatus.Failed)
            {
                throw new ConflictException(string.Format("Order is already {0}", toUpdate.Status.ToString()));
            }

            toUpdate.Status = OrderStatus.Failed;
            toUpdate.PaymentId = null;
            toUpdate.Error = "Payment was deleted";
        }, ct);
    }

    [HttpPost(nameof(UserDeletedIntegrationEvent))]
    [Topic(DaprConstants.Components.PubSub, nameof(UserDeletedIntegrationEvent))]
    public async Task HandleAsync(UserDeletedIntegrationEvent @event,
                                  [FromServices] GenericRepository<Order> repository,
                                  CancellationToken ct)
    {
        var orders = await repository.GetAllAsync(x => x.UserId == @event.UserId, ct);
        if (orders.Any())
        {
            foreach (var order in orders)
            {
                await repository.DeleteAsync(order.EntityId!.Value, ct);
            }
        }
    }

    [HttpPost(nameof(UserCheckoutAcceptedIntegrationEvent))]
    [Topic(DaprConstants.Components.PubSub, nameof(UserCheckoutAcceptedIntegrationEvent))]
    public async Task HandleAsync(UserCheckoutAcceptedIntegrationEvent @event,
                                  [FromServices] IGenericWriteRepository<Order> repository,
                                  [FromServices] DaprClient dapr,
                                  [FromServices] WorkflowEngineClient engine,
                                  [FromServices] ILogger<OrdersController> logger,
                                  CancellationToken ct)
    {
        UserDTO? user = null;
        try
        {
            user = await dapr.InvokeMethodAsync<UserDTO>(HttpMethod.Get,
                                                         DaprConstants.Services.UsersService,
                                                         $"/api/users/{@event.UserId}",
                                                         ct);

            if (user is null)
            {
                throw new InvalidOperationException("User is null");
            }

            logger.LogInformation("Successfully retrieved user: {UserId}", user.UserId);
        }
        catch (InvocationException ex)
        {
            if (ex.TryParseErrorResponse(out var response))
            {
                logger.LogDaprInvocationError(ex, response);
                return;
            }

            logger.LogDaprInvocationError(ex);
            throw new UnexpectedErrorException("Cannot read error response from dapr result");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "{ExceptionMessage}", ex.Message);
            throw new UnexpectedErrorException(ex.Message);
        }

        if (@event.UseWorkflow)
        {
            var instanceId = Guid.NewGuid().ToString();
            await engine.ScheduleNewWorkflowAsync(nameof(OrderProcessingWorkflow), instanceId, @event);
        }
        else
        {
            Order entity = await repository.CreateAsync(@event.ToDomain, ct);
            OrderDTO dto = entity.ToDTO();

            // publish event
            var createdEvent = new OrderCreatedIntegrationEvent
            {
                Amount = dto.Amount,
                CreatedDate = dto.OrderDate!.Value,
                OrderId = dto.OrderId,
                UserId = user!.UserId
            };

            await dapr.PublishEventAsync(DaprConstants.Components.PubSub,
                                         nameof(OrderCreatedIntegrationEvent),
                                         createdEvent,
                                         ct);
        }
    }

    [HttpPost(nameof(UserDeletedIntegrationEvent))]
    [Topic(DaprConstants.Components.PubSub, nameof(UserDeletedIntegrationEvent))]
    public async Task HandleAsync(UserDeletedIntegrationEvent @event,
                                  [FromServices] GenericRepository<Order> repository,
                                  [FromServices] IGenericWriteRepository<Entities.Domain.OrderItem> itemRepository,
                                  CancellationToken ct)
    {
        IEnumerable<Order> userOrders = await repository.GetAllAsync(x => x.UserId == @event.UserId, ct);
        if (userOrders.Any())
        {
            foreach (var userOrder in userOrders)
            {
                Order order = await repository.GetWithChildrenAsync(userOrder.EntityId!.Value, "Items");
                if (order.Items.Any())
                {
                    foreach (var item in order.Items) await itemRepository.DeleteAsync(item.EntityId!.Value, ct);
                }

                await repository.DeleteAsync(userOrder.EntityId!.Value, ct);
            }
        }
    }
}
