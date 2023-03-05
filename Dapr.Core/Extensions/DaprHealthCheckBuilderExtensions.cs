using Dapr.Core.HealthChecks;
using Microsoft.Extensions.DependencyInjection;

namespace Dapr.Core.Extensions;

public static class DaprHealthCheckBuilderExtensions
{
    public static IHealthChecksBuilder AddDapr(this IHealthChecksBuilder builder) =>
        builder.AddCheck<DaprHealthCheck>("dapr");
}