using Dapr.Core.Storage;
using Dapr.Ordering.Api.Entities.Domain;
using Microsoft.EntityFrameworkCore;

namespace Dapr.Ordering.Api.Database;

public class OrderingContext(DbContextOptions<OrderingContext> dbOptions) : DbContext(dbOptions), IEntityStorage<Order>, IEntityStorage<OrderItem>
{
    public DbSet<Order> Orders { get; set; } = null!;
    public DbSet<OrderItem> OrderItems { get; set; } = null!;
    public DbSet<Order> Set() => Orders;
    DbSet<OrderItem> IEntityStorage<OrderItem>.Set() => OrderItems;
}
