using System.Text.Json.Serialization;
using Dapr.Audit.Api.Database;
using Dapr.Audit.Api.Extensions;
using Dapr.Core.Extensions;
using HealthChecks.UI.Client;
using Microsoft.EntityFrameworkCore;

const string AppName = "Audit API";

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.AddCustomSerilog(AppName);
builder.AddCustomConfiguration();
builder.AddCustomHealthChecks()
    .AddMySql(builder.Configuration.GetConnectionString("AuditDbConnection")!, "AuditAPIDB-check", tags: new[] { "audit-db" });

builder.AddErrorHandlingMiddleware();
builder.AddCustomDatabaseAndRepositories();
builder.Services.AddOptions();
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()))
    .AddDapr(); // 1
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

app.UseCloudEvents(); // 2
app.UseErrorHandlingMiddleware();
app.UseZipkin(AppName);

app.MapGet("/", () => Results.LocalRedirect("~/swagger")).ExcludeFromDescription();
app.MapControllers();
app.MapSubscribeHandler(); // 3 /dapr/subscribe
app.MapCustomHealthChecks(responseWriter: UIResponseWriter.WriteHealthCheckUIResponse);

try
{
    app.Logger.LogInformation("Applying database migration ({ApplicationName})...", AppName);
    app.ApplyDatabaseMigration<AuditContext>();

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