namespace Dapr.Core.Events.Users;

public class UserDeletedIntegrationEvent : IntegrationEvent
{
    public DateTime DeletedDate { get; set; }
}
