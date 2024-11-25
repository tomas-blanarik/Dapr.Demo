using System.Linq.Expressions;
using Dapr.Core.Entities;
using Dapr.Core.Exceptions;
using Dapr.Core.Paging;
using Dapr.Core.Storage;
using Microsoft.EntityFrameworkCore;

namespace Dapr.Core.Repositories.Generic;

public class GenericRepository<T>(IEntityStorage<T> storage) : IGenericReadRepository<T>, IGenericWriteRepository<T> 
    where T : class, IDomainEntity
{
    private readonly IEntityStorage<T> _storage = storage;

    public async Task<T> CreateAsync(Func<T> createFunction, CancellationToken ct = default)
    {
        T entity = createFunction();
        await _storage.Set().AddAsync(entity, ct);
        await _storage.SaveChangesAsync(ct);
        return entity;
    }

    public async Task DeleteAllAsync(CancellationToken ct = default)
    {
        _storage.Set().RemoveRange(_storage.Set());
        await _storage.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        T? entity = await _storage.Set().SingleOrDefaultAsync(x => x.EntityId == id, ct) ?? throw new DomainEntityNotFoundException(typeof(T), id);
        _storage.Set().Remove(entity);
        await _storage.SaveChangesAsync(ct);
    }

    public async Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>>? searchExpression = null, CancellationToken ct = default)
    {
        IQueryable<T> query = _storage.Set().AsNoTracking().AsQueryable();
        if (searchExpression is not null)
        {
            query = query.Where(searchExpression);
        }

        return await query.ToListAsync(ct);
    }

    public async Task<PagedResult<T>> GetAllPagedAsync(PagingRequest request, Expression<Func<T, bool>>? searchExpression = null, CancellationToken ct = default)
    {
        IQueryable<T> query = _storage.Set().AsNoTracking().AsQueryable();
        if (searchExpression is not null)
        {
            query = query.Where(searchExpression);
        }

        var items = await query
            .Skip((request.PageIndex - 1) * request.PageSize)
            .Take(request.PageSize)
            .OrderBy(x => x.EntityId)
            .ToListAsync(ct);

        return new PagedResult<T> { Items = items, Paging = request };
    }

    public async Task<PagedResult<T>> GetAllPagedAsync<TKey>(PagingRequest request, Expression<Func<T, bool>>? searchExpression = null, Expression<Func<T, TKey>>? orderByExpression = null, CancellationToken ct = default)
    {
        IQueryable<T> query = _storage.Set().AsNoTracking().AsQueryable();
        if (searchExpression is not null)
        {
            query = query.Where(searchExpression);
        }

        query = query
            .Skip((request.PageIndex - 1) * request.PageSize)
            .Take(request.PageSize);

        if (orderByExpression is not null)
        {
            query = query.OrderByDescending(orderByExpression);
        }

        var items = await query.ToListAsync(ct);
        return new PagedResult<T> { Items = items, Paging = request };
    }

    public async Task<T> GetAsync(Guid id, CancellationToken ct = default)
    {
        T? entity = await _storage.Set().AsNoTracking().SingleOrDefaultAsync(x => x.EntityId == id, ct);
        return entity is null ? throw new DomainEntityNotFoundException(typeof(T), id) : entity;
    }

    public async Task<T> GetWithChildrenAsync(Guid id, params string[] includes)
    {
        var query = _storage.Set().AsNoTracking();
        foreach (var include in includes)
        {
            query = query.Include(include);
        }

        T? entity = await query.SingleOrDefaultAsync(x => x.EntityId == id);
        return entity is null ? throw new DomainEntityNotFoundException(typeof(T), id) : entity;
    }

    public async Task<T> UpdateAsync(Guid id, Action<T> updateEntityFunction, CancellationToken ct = default)
    {
        T entity = await GetAsync(id, ct);
        updateEntityFunction(entity);
        _storage.Set().Update(entity);
        await _storage.SaveChangesAsync(ct);
        return entity;
    }
}
