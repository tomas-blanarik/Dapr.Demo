namespace Dapr.Core.Events.Orders;

public abstract class OrderBaseIntegrationEvent : IntegrationEvent
{
    public Guid OrderId { get; set; }
}
