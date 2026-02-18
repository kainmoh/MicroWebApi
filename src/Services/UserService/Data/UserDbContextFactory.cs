using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace UserService.Data;

/// <summary>
/// Design-time factory for UserDbContext (used for migrations)
/// </summary>
public class UserDbContextFactory : IDesignTimeDbContextFactory<UserDbContext>
{
    public UserDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<UserDbContext>();
        optionsBuilder.UseMySql(
            "Server=localhost;Port=3306;Database=UserServiceDb;User=root;Password=welcome123;",
            ServerVersion.Parse("8.0.0-mysql"));

        return new UserDbContext(optionsBuilder.Options);
    }
}
