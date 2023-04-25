namespace Dapr.Core.Events.Orders;

public class OrderCompletedIntegrationEvent : OrderBaseIntegrationEvent
{
    public DateTime CompletedDate { get; set; }
}
