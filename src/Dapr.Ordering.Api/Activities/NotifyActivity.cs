using Dapr.Workflow;

namespace Dapr.Ordering.Api.Activities;

public record Notification(string Message);

public class NotifyActivity(ILogger<NotifyActivity> logger) : WorkflowActivity<Notification, object>
{
    private readonly ILogger<NotifyActivity> _logger = logger;

    public override Task<object> RunAsync(WorkflowActivityContext context, Notification input)
    {
        _logger.LogInformation("{Message}", input.Message);
        return Task.FromResult<object>(null!);
    }
}