var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddHealthChecksUI()
    .AddInMemoryStorage();

builder.Logging.AddJsonConsole();

var app = builder.Build();

app.UseHealthChecksUI(config =>
{
    config.ResourcesPath = "/ui/resources";
});

app.MapGet("/", () => Results.LocalRedirect("~/healthchecks-ui"));
app.MapHealthChecksUI();

app.Run();