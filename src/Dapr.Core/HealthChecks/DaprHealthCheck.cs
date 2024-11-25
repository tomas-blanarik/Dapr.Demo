using Dapr.Client;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Dapr.Core.HealthChecks;

public class DaprHealthCheck(DaprClient daprClient) : IHealthCheck
{
    private readonly DaprClient _daprClient = daprClient;

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var healthy = await _daprClient.CheckHealthAsync(cancellationToken);

        if (healthy)
        {
            return HealthCheckResult.Healthy("Dapr sidecar is healthy.");
        }

        return new HealthCheckResult(
            context.Registration.FailureStatus,
            "Dapr sidecar is unhealthy.");
    }
}
