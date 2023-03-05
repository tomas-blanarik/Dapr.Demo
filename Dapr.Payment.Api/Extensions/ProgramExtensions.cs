using Dapr.Core.Extensions;
using Dapr.Payment.Api.Database;
using Microsoft.EntityFrameworkCore;
using Domain = Dapr.Payment.Api.Entities.Domain;

namespace Dapr.Payment.Api.Extensions;

public static class ProgramExtensions
{
    public static void AddCustomDatabaseAndRepositories(this WebApplicationBuilder builder)
    {
        builder.Services.AddDbContext<PaymentContext>(o =>
        {
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            o.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
            o.EnableDetailedErrors();
            o.EnableSensitiveDataLogging();
        });

        builder.Services.AddGenericRepository<Domain.Payment, PaymentContext>();
    }
}
