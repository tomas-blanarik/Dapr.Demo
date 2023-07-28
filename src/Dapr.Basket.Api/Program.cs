
using System.Text.Json.Serialization;
using Dapr.Basket.Api.Repositories;
using Dapr.Core.Extensions;
using HealthChecks.UI.Client;

const string AppName = "Basket API";

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.AddCustomHealthChecks();
builder.AddCustomSerilog(AppName);
builder.AddErrorHandlingMiddleware();
builder.Services.AddScoped<IBasketRepository, DaprBasketRepository>();
builder.Services.AddOptions();
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()))
    .AddDapr(); //1
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

app.UseCloudEvents(); //2
app.UseErrorHandlingMiddleware();
app.UseZipkin(AppName);

app.MapGet("/", () => Results.LocalRedirect("~/swagger")).ExcludeFromDescription();
app.MapControllers();
app.MapSubscribeHandler(); //3
app.MapCustomHealthChecks(responseWriter: UIResponseWriter.WriteHealthCheckUIResponse);

await app.RunAsync();