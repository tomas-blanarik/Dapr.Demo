namespace Dapr.Core.Events.Orders;

public class OrderCreatedIntegrationEvent : OrderBaseIntegrationEvent
{
    public DateTime CreatedDate { get; set; }
    public decimal Amount { get; set; }
}
