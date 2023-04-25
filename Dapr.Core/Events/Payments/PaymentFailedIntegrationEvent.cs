namespace Dapr.Core.Events.Payments;

public class PaymentFailedIntegrationEvent : PaymentBaseIntegrationEvent
{
    public DateTime FailedDate { get; set; }
    public string Reason { get; set; } = null!;
}
