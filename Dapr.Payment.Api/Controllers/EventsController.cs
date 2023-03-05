using Dapr.Client;
using Dapr.Core;
using Dapr.Core.Events.Orders;
using Dapr.Core.Events.Payments;
using Dapr.Core.Exceptions;
using Dapr.Core.Repositories;
using Dapr.Core.Repositories.Generic;
using Dapr.Payment.Api.Entities.Enums;
using Microsoft.AspNetCore.Mvc;
using Domain = Dapr.Payment.Api.Entities.Domain;

namespace Dapr.Payment.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class EventsController : ControllerBase
{
    [HttpPost(nameof(OrderCreatedIntegrationEvent))]
    [Topic(DaprConstants.Components.PubSub, nameof(OrderCreatedIntegrationEvent))]
    public async Task HandleAsync(OrderCreatedIntegrationEvent @event,
                                  [FromServices] IGenericWriteRepository<Domain.Payment> repository,
                                  [FromServices] DaprClient dapr,
                                  CancellationToken ct)
    {
        Domain.Payment entity = await repository.CreateAsync(() => new()
        {
            CreatedDate = DateTime.UtcNow,
            OrderId = @event.OrderId,
            UserId = @event.UserId,
            Status = PaymentStatusEnum.Initiated
        }, ct);

        var createdEvent = new PaymentCreatedIntegrationEvent
        {
            CreatedDate = entity.CreatedDate!.Value,
            OrderId = entity.OrderId!.Value,
            PaymentId = entity.EntityId!.Value,
            UserId = entity.UserId!.Value
        };

        await dapr.PublishEventAsync(DaprConstants.Components.PubSub,
                                     nameof(PaymentCreatedIntegrationEvent),
                                     createdEvent,
                                     ct);
    }

    [HttpPost(nameof(OrderDeletedIntegrationEvent))]
    [Topic(DaprConstants.Components.PubSub, nameof(OrderDeletedIntegrationEvent))]
    public async Task HandleAsync(OrderDeletedIntegrationEvent @event,
                                  [FromServices] GenericRepository<Domain.Payment> repository,
                                  [FromServices] DaprClient dapr,
                                  CancellationToken ct)
    {
        Domain.Payment? entity = (await repository.GetAllAsync(x => x.OrderId == @event.OrderId, ct))?.SingleOrDefault();
        if (entity is null)
        {
            throw new UnexpectedErrorException(string.Format("Payment with order ID '{0}' not found", @event.OrderId));
        }

        await repository.DeleteAsync(entity.EntityId!.Value, ct);
        var deletedEvent = new PaymentDeletedIntegrationEvent
        {
            DeletedDate = DateTime.UtcNow,
            PaymentId = entity.EntityId!.Value,
            UserId = entity.UserId!.Value
        };

        await dapr.PublishEventAsync(DaprConstants.Components.PubSub,
                                     nameof(PaymentDeletedIntegrationEvent),
                                     deletedEvent,
                                     ct);
    }
}
