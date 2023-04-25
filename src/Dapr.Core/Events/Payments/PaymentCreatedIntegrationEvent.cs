namespace Dapr.Core.Events.Payments;

public class PaymentCreatedIntegrationEvent : PaymentBaseIntegrationEvent
{
    public Guid OrderId { get; set; }
    public DateTime CreatedDate { get; set; }
}
