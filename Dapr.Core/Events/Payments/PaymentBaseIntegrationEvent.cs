namespace Dapr.Core.Events.Payments;

public abstract class PaymentBaseIntegrationEvent : IntegrationEvent
{
    public Guid PaymentId { get; set; }
}
