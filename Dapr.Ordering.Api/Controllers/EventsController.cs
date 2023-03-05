using Dapr.Client;
using Dapr.Core;
using Dapr.Core.Events.Orders;
using Dapr.Core.Events.Payments;
using Dapr.Core.Exceptions;
using Dapr.Core.Repositories;
using Dapr.Core.Repositories.Generic;
using Dapr.Ordering.Api.Entities.Domain;
using Dapr.Ordering.Api.Entities.Enums;
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
        Order? entity = (await repository.GetAllAsync(x => x.PaymentId == @event.PaymentId, ct))?.SingleOrDefault();
        if (entity is null)
        {
            throw new UnexpectedErrorException(string.Format("Order with payment ID '{0}' not found", @event.PaymentId));
        }

        entity = await repository.UpdateAsync(entity.EntityId!.Value, toUpdate =>
        {
            if (toUpdate.Status == OrderStatusEnum.Completed || toUpdate.Status == OrderStatusEnum.Failed)
            {
                throw new ConflictException(string.Format("Order is already {0}", toUpdate.Status.ToString()));
            }

            toUpdate.Status = OrderStatusEnum.Completed;
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
        Order? entity = (await repository.GetAllAsync(x => x.PaymentId == @event.PaymentId, ct))?.SingleOrDefault();
        if (entity is null)
        {
            throw new UnexpectedErrorException(string.Format("Order with payment ID '{0}' not found", @event.PaymentId));
        }

        await repository.UpdateAsync(entity.EntityId!.Value, toUpdate =>
        {
            if (toUpdate.Status == OrderStatusEnum.Completed || toUpdate.Status == OrderStatusEnum.Failed)
            {
                throw new ConflictException(string.Format("Order is already {0}", toUpdate.Status.ToString()));
            }

            toUpdate.Status = OrderStatusEnum.Failed;
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
        Order? entity = (await repository.GetAllAsync(x => x.PaymentId == @event.PaymentId, ct))?.SingleOrDefault();
        if (entity is null)
        {
            throw new UnexpectedErrorException(string.Format("Order with payment ID '{0}' not found", @event.PaymentId));
        }

        await repository.UpdateAsync(entity.EntityId!.Value, toUpdate =>
        {
            if (toUpdate.Status == OrderStatusEnum.Completed || toUpdate.Status == OrderStatusEnum.Failed)
            {
                throw new ConflictException(string.Format("Order is already {0}", toUpdate.Status.ToString()));
            }

            toUpdate.Status = OrderStatusEnum.Failed;
            toUpdate.PaymentId = null;
            toUpdate.Error = "Payment was deleted";
        }, ct);
    }
}
