using Dapr.Basket.Api.Entities.DTO;
using Dapr.Core.Events;

namespace Dapr.Basket.Api.Entities.Events;

public class UserCheckoutAcceptedIntegrationEvent : IntegrationEvent
{
    public BasketDTO Basket { get; set; } = null!;
    public bool UseWorkflow { get; set; }
}