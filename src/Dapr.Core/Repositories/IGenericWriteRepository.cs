using Dapr.Core.Entities;

namespace Dapr.Core.Repositories;

public interface IGenericWriteRepository<T> where T : IDomainEntity
{
    Task<T> CreateAsync(Func<T> createFunction, CancellationToken ct = default);
    Task<T> UpdateAsync(Guid id, Action<T> updateEntityFunction, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task DeleteAllAsync(CancellationToken ct = default);
}
