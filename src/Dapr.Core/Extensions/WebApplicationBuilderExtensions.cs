using Dapr.Client;
using Dapr.Core.Middlewares;
using Dapr.Extensions.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Polly;
using Serilog;
using Serilog.Exceptions;
using zipkin4net;
using zipkin4net.Middleware;
using zipkin4net.Tracers.Zipkin;
using zipkin4net.Transport.Http;

namespace Dapr.Core.Extensions;

public static class WebApplicationBuilderExtensions
{
    public static void AddCustomConfiguration(this WebApplicationBuilder builder)
    {
        try
        {
            var daprClient = new DaprClientBuilder().Build();
            builder.Configuration.AddDaprSecretStore(
                DaprConstants.Components.SecretStore,
                daprClient,
                ["--", ":", "__"],
                TimeSpan.FromSeconds(30));

            Log.Logger.Information("Successfully registered Dapr Secret Store to IConfiguration");
        }
        catch (Exception e)
        {
            Log.Logger.Error(e, "Cannot register dapr secret store to the configuration: {Message}", e.Message);
            throw;
        }
    }

    public static void AddCustomSerilog(this WebApplicationBuilder builder, string appName)
    {
        var seqServerUrl = builder.Configuration["SeqServerUrl"];
        var loggerConfiguration = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .WriteTo.Console()
            .Enrich.WithExceptionDetails()
            .Enrich.FromLogContext()
            .Enrich.WithProperty("ApplicationName", appName);

        if (seqServerUrl is not null)
        {
            loggerConfiguration.WriteTo.Seq(seqServerUrl);
        }

        Log.Logger = loggerConfiguration.CreateLogger();
        builder.Host.UseSerilog();
    }

    public static IHealthChecksBuilder AddCustomHealthChecks(this WebApplicationBuilder builder)
    {
        return builder.Services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy())
            .AddDapr();
    }

    public static void AddErrorHandlingMiddleware(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<ErrorHandlingMiddleware>();
    }

    public static void UseErrorHandlingMiddleware(this WebApplication app)
    {
        app.UseMiddleware<ErrorHandlingMiddleware>();
    }

    public static void UseZipkin(this WebApplication app, string appName)
    {
        var lifetime = app.Lifetime;
        var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
        lifetime.ApplicationStarted.Register(() =>
        {
            TraceManager.SamplingRate = 1.0f;
            var logger = new TracingLogger(loggerFactory, "zipkin");
            var httpSender = new HttpZipkinSender(app.Configuration["ZipkinUrl"], "application/json");
            var tracer = new ZipkinTracer(httpSender, new JSONSpanSerializer());

            TraceManager.RegisterTracer(tracer);
            TraceManager.Start(logger);
        });

        lifetime.ApplicationStopped.Register(() => TraceManager.Stop());
        app.UseTracing(appName);
    }

    public static void ApplyDatabaseMigration<TContext>(this WebApplication app) where TContext : DbContext
    {
        // Apply database migration automatically. Note that this approach is not
        // recommended for production scenarios. Consider generating SQL scripts from
        // migrations instead.
        using var scope = app.Services.CreateScope();

        var retryPolicy = CreateRetryPolicy(app.Configuration, Log.Logger);
        var context = scope.ServiceProvider.GetRequiredService<TContext>();

        retryPolicy.Execute(context.Database.Migrate);
    }

    private static Policy CreateRetryPolicy(IConfiguration configuration, Serilog.ILogger logger)
    {
        // Only use a retry policy if configured to do so.
        // When running in an orchestrator/K8s, it will take care of restarting failed services.
        if (bool.TryParse(configuration["RetryMigrations"], out bool _))
        {
            return Policy.Handle<Exception>().
                WaitAndRetryForever(
                    sleepDurationProvider: _ => TimeSpan.FromSeconds(5),
                    onRetry: (exception, retry, _) =>
                    {
                        logger.Warning(
                            exception,
                            "Exception {ExceptionType} with message {Message} detected during database migration (retry attempt {retry})",
                            exception.GetType().Name,
                            exception.Message,
                            retry);
                    }
                );
        }

        return Policy.NoOp();
    }
}
