using Dapr.Audit.Api.Database;
using Dapr.Audit.Api.Entities.Domain;
using Dapr.Core.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Dapr.Audit.Api.Extensions;

public static class ProgramExtensions
{
    public static void AddCustomDatabaseAndRepositories(this WebApplicationBuilder builder)
    {
        builder.Services.AddDbContext<AuditContext>(o =>
        {
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            o.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
            o.EnableDetailedErrors();
            o.EnableSensitiveDataLogging();
        });

        builder.Services.AddGenericRepository<AuditItem, AuditContext>();
    }
}
