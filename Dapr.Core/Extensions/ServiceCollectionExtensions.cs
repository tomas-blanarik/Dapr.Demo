using Dapr.Core.Entities;
using Dapr.Core.Repositories;
using Dapr.Core.Repositories.Generic;
using Dapr.Core.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Dapr.Core.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGenericRepository<T, TContext>(this IServiceCollection services)
        where T : class, IDomainEntity
        where TContext : DbContext, IEntityStorage<T>
    {
        services.AddTransient<IGenericReadRepository<T>, GenericRepository<T>>();
        services.AddTransient<IGenericWriteRepository<T>, GenericRepository<T>>();
        services.AddTransient<GenericRepository<T>>();
        services.AddScoped<IEntityStorage<T>, TContext>();
        return services;
    }
}
