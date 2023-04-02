namespace Dapr.Core.Events;

public abstract class IntegrationEvent
{
    protected IntegrationEvent()
    {
        EventId = Guid.NewGuid();
        EventDate = DateTime.UtcNow;
    }

    public Guid EventId { get; set; }
    public Guid UserId { get; set; }
    public DateTime EventDate { get; set; }
}
