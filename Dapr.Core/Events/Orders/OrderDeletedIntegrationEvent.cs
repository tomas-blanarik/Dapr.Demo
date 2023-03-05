namespace Dapr.Core.Events.Orders;

public class OrderDeletedIntegrationEvent : OrderBaseIntegrationEvent
{
    public DateTime DeletedDate { get; set; }
}
