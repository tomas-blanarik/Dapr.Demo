using Dapr.Core.Extensions;
using Dapr.Users.Api.Database;
using Dapr.Users.Api.Entities.Domain;
using Microsoft.EntityFrameworkCore;

namespace Dapr.Users.Api.Extensions;

public static class ProgramExtensions
{
    public static void AddCustomDatabaseAndRepositories(this WebApplicationBuilder builder)
    {
        builder.Services.AddDbContext<UsersContext>(o =>
        {
            var connectionString = builder.Configuration.GetConnectionString("UsersDbConnection");
            o.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
            o.EnableDetailedErrors();
            o.EnableSensitiveDataLogging();
        });

        builder.Services.AddGenericRepository<User, UsersContext>();
    }
}
