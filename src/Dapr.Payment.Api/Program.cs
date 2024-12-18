using System.Text.Json.Serialization;
using Dapr.Core.Extensions;
using Dapr.Payment.Api.Database;
using Dapr.Payment.Api.Extensions;
using HealthChecks.UI.Client;

const string AppName = "Payment API";

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.AddCustomSerilog(AppName);
builder.AddCustomConfiguration();
builder.AddCustomHealthChecks()
    .AddDbContextCheck<PaymentContext>("PaymentAPIDB-check", tags: ["payment-db"]);

builder.AddCustomDatabaseAndRepositories();
builder.AddErrorHandlingMiddleware();
builder.Services.AddOptions();
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()))
    .AddDapr();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o => o.EnableAnnotations());
builder.Services.AddRouting(o => o.LowercaseUrls = true);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCloudEvents();
app.UseErrorHandlingMiddleware();
app.UseZipkin(AppName);

app.MapGet("/", () => Results.LocalRedirect("~/swagger")).ExcludeFromDescription();
app.MapControllers();
app.MapSubscribeHandler();
app.MapCustomHealthChecks(responseWriter: UIResponseWriter.WriteHealthCheckUIResponse);

try
{
    app.Logger.LogInformation("Applying database migration ({ApplicationName})...", AppName);
    app.ApplyDatabaseMigration<PaymentContext>();

    app.Run();
}
catch (Exception e)
{
    app.Logger.LogCritical(e, "Host terminated unexpectedly ({ApplicationName})", AppName);
}
finally
{
    Serilog.Log.CloseAndFlush();
}