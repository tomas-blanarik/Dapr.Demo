using Dapr.Core.Storage;
using Dapr.Ordering.Api.Entities.Domain;
using Microsoft.EntityFrameworkCore;

namespace Dapr.Ordering.Api.Database;

public class OrderingContext : DbContext, IEntityStorage<Order>
{
    public OrderingContext(DbContextOptions<OrderingContext> dbOptions)
        : base(dbOptions)
    { }

    public DbSet<Order> Orders { get; set; }
    public DbSet<Order> Set() => Orders;
}
