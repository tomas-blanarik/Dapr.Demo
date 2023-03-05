namespace Dapr.Core.Events.Payments;

public class PaymentDeletedIntegrationEvent : PaymentBaseIntegrationEvent
{
    public DateTime DeletedDate { get; set; }
}
