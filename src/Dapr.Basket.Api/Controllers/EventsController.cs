using Dapr.Basket.Api.Entities.DTO;
using Dapr.Basket.Api.Repositories;
using Dapr.Core;
using Dapr.Core.Events.Orders;
using Dapr.Core.Events.Users;
using Microsoft.AspNetCore.Mvc;

namespace Dapr.Basket.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class EventsController : ControllerBase
{
    [HttpPost(nameof(OrderCompletedIntegrationEvent))]
    [Topic(DaprConstants.Components.PubSub, nameof(OrderCompletedIntegrationEvent))]
    public async Task HandleAsync(OrderCompletedIntegrationEvent @event,
                                  [FromServices] IBasketRepository basketRepository,
                                  CancellationToken ct)
    {
        var userId = @event.UserId;
        await basketRepository.DeleteAsync(userId.ToString(), ct);
    }

    [HttpPost(nameof(UserDeletedIntegrationEvent))]
    [Topic(DaprConstants.Components.PubSub, nameof(UserDeletedIntegrationEvent))]
    public async Task HandleAsync(UserDeletedIntegrationEvent @event,
                                 [FromServices] IBasketRepository repository,
                                 CancellationToken ct)
    {
        BasketDTO basket = await repository.GetAsync(@event.UserId.ToString(), ct);
        if (basket is not null)
        {
            await repository.DeleteAsync(@event.UserId.ToString(), ct);
        }
    }
}