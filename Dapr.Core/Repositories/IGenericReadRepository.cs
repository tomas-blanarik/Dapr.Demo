using System.Linq.Expressions;
using Dapr.Core.Entities;
using Dapr.Core.Paging;

namespace Dapr.Core.Repositories;

public interface IGenericReadRepository<T> where T : IDomainEntity
{
    Task<T> GetAsync(Guid id, CancellationToken ct = default);
    Task<T> GetWithChildrenAsync(Guid id, params string[] includes);
    Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>>? searchExpression = null, CancellationToken ct = default);
    Task<PagedResult<T>> GetAllPagedAsync(PagingRequest request, Expression<Func<T, bool>>? searchExpression = null, CancellationToken ct = default);
    Task<PagedResult<T>> GetAllPagedAsync<TKey>(PagingRequest request, Expression<Func<T, bool>>? searchExpression = null, Expression<Func<T, TKey>>? orderByExpression = null, CancellationToken ct = default);
}
