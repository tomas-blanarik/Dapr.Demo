using Dapr.Audit.Api.Entities.Domain;
using Dapr.Core.Storage;
using Microsoft.EntityFrameworkCore;

namespace Dapr.Audit.Api.Database;

public class AuditContext : DbContext, IEntityStorage<AuditItem>
{
    public AuditContext(DbContextOptions<AuditContext> dbOptions)
        : base(dbOptions)
    { }

    public DbSet<AuditItem> AuditItems { get; set; } = null!;
    public DbSet<AuditItem> Set() => AuditItems;
}
