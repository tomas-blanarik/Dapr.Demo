using Dapr.Core.Extensions;
using Dapr.Ordering.Api.Database;
using Dapr.Ordering.Api.Entities.Domain;
using Microsoft.EntityFrameworkCore;

namespace Dapr.Ordering.Api.Extensions;

public static class ProgramExtensions
{
    public static void AddCustomDatabaseAndRepositories(this WebApplicationBuilder builder)
    {
        builder.Services.AddDbContext<OrderingContext>(o =>
        {
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            o.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
            o.EnableDetailedErrors();
            o.EnableSensitiveDataLogging();
        });

        builder.Services.AddGenericRepository<Order, OrderingContext>();
    }
}
