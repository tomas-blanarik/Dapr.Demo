using Dapr.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Dapr.Core.Storage;

public interface IEntityStorage<T> where T : class, IDomainEntity
{
    DbSet<T> Set();
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
