using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Nawel.Api.Data;

/// <summary>
/// Factory for creating NawelDbContext at design-time (for migrations).
/// This ensures migrations are always generated for MySQL, not SQLite.
/// </summary>
public class NawelDbContextFactory : IDesignTimeDbContextFactory<NawelDbContext>
{
    public NawelDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<NawelDbContext>();

        // Use a dummy MySQL connection string for migration generation
        // The actual connection string will be used at runtime
        var connectionString = "Server=localhost;Port=3306;Database=nawel;User=root;Password=dummy;";
        var serverVersion = new MySqlServerVersion(new Version(8, 0, 0));

        optionsBuilder.UseMySql(connectionString, serverVersion);

        return new NawelDbContext(optionsBuilder.Options);
    }
}
