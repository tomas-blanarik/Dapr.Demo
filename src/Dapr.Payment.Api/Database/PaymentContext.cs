using Dapr.Core.Storage;
using Microsoft.EntityFrameworkCore;
using Domain = Dapr.Payment.Api.Entities.Domain;

namespace Dapr.Payment.Api.Database;

public class PaymentContext(DbContextOptions<PaymentContext> dbOptions) : DbContext(dbOptions), IEntityStorage<Domain.Payment>
{
    public DbSet<Domain.Payment> Payments { get; set; } = null!;
    public DbSet<Domain.Payment> Set() => Payments;
}
