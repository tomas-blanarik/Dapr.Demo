using Dapr.Audit.Api.Entities.Domain;
using Dapr.Core.Storage;
using Microsoft.EntityFrameworkCore;

namespace Dapr.Audit.Api.Database;

public class AuditContext(DbContextOptions<AuditContext> dbOptions) : DbContext(dbOptions), IEntityStorage<AuditItem>
{
    public DbSet<AuditItem> AuditItems { get; set; } = null!;
    public DbSet<AuditItem> Set() => AuditItems;
}
