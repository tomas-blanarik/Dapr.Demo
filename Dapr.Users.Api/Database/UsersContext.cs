using Dapr.Core.Storage;
using Dapr.Users.Api.Entities.Domain;
using Microsoft.EntityFrameworkCore;

namespace Dapr.Users.Api.Database;

public class UsersContext : DbContext, IEntityStorage<User>
{
    public UsersContext(DbContextOptions<UsersContext> dbOptions)
        : base(dbOptions)
    { }

    public DbSet<User> Users { get; set; }
    public DbSet<User> Set() => Users;
}
