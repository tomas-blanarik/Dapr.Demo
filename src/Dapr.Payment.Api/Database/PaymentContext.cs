using Dapr.Core.Storage;
using Microsoft.EntityFrameworkCore;
using Domain = Dapr.Payment.Api.Entities.Domain;

namespace Dapr.Payment.Api.Database;

public class PaymentContext : DbContext, IEntityStorage<Domain.Payment>
{
    public PaymentContext(DbContextOptions<PaymentContext> dbOptions)
        : base(dbOptions)
    { }

    public DbSet<Domain.Payment> Payments { get; set; } = null!;
    public DbSet<Domain.Payment> Set() => Payments;
}
