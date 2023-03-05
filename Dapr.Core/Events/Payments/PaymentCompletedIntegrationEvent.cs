namespace Dapr.Core.Events.Payments;

public class PaymentCompletedIntegrationEvent : PaymentBaseIntegrationEvent
{
    public DateTime CompletedDate { get; set; }
}
