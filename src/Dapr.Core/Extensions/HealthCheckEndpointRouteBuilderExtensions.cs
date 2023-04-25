using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Dapr.Core.Extensions;

public static class HealthCheckEndpointRouteBuilderExtensions
{
    public static void MapCustomHealthChecks(
        this WebApplication app,
        string healthPattern = "/hc",
        string livenessPattern = "/liveness",
        Func<HttpContext, HealthReport, Task>? responseWriter = default)
    {
        app.MapHealthChecks(healthPattern, new HealthCheckOptions()
        {
            Predicate = _ => true,
            ResponseWriter = responseWriter ?? ((_, __) => Task.CompletedTask),
        });
        app.MapHealthChecks(livenessPattern, new HealthCheckOptions
        {
            Predicate = r => r.Name.Contains("self")
        });
    }
}
