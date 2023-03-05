namespace Dapr.Core.Events.Users;

public class UserCreatedIntegrationEvent : IntegrationEvent
{
    public DateTime CreatedDate { get; set; }
}
