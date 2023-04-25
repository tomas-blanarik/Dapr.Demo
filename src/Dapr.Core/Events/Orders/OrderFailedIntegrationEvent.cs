namespace Dapr.Core.Events.Orders;

public class OrderFailedIntegrationEvent : OrderBaseIntegrationEvent
{
    public DateTime FailedDate { get; set; }
    public string Reason { get; set; } = null!;
}
