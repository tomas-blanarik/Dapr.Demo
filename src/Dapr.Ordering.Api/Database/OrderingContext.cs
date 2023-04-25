using Dapr.Core.Storage;
using Dapr.Ordering.Api.Entities.Domain;
using Microsoft.EntityFrameworkCore;

namespace Dapr.Ordering.Api.Database;

public class OrderingContext : DbContext, IEntityStorage<Order>, IEntityStorage<OrderItem>
{
    public OrderingContext(DbContextOptions<OrderingContext> dbOptions)
        : base(dbOptions)
    { }

    public DbSet<Order> Orders { get; set; } = null!;
    public DbSet<OrderItem> OrderItems { get; set; } = null!;
    public DbSet<Order> Set() => Orders;
    DbSet<OrderItem> IEntityStorage<OrderItem>.Set() => OrderItems;
}
