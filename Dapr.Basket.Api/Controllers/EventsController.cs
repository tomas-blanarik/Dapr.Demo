using Dapr.Basket.Api.Repositories;
using Dapr.Core;
using Dapr.Core.Events.Orders;
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
}