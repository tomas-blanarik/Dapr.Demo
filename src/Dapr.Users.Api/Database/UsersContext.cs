using Dapr.Core.Storage;
using Dapr.Users.Api.Entities.Domain;
using Microsoft.EntityFrameworkCore;

namespace Dapr.Users.Api.Database;

public class UsersContext(DbContextOptions<UsersContext> dbOptions) : DbContext(dbOptions), IEntityStorage<User>
{
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<User> Set() => Users;
}
