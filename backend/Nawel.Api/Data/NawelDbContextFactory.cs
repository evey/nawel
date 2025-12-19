using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Nawel.Api.Data;

/// <summary>
/// Factory for creating NawelDbContext at design-time (for migrations).
/// This ensures migrations are generated for SQLite in development.
/// Production uses MySQL through the deployment script.
/// </summary>
public class NawelDbContextFactory : IDesignTimeDbContextFactory<NawelDbContext>
{
    public NawelDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<NawelDbContext>();

        // Default to SQLite for development
        var connectionString = "Data Source=nawel.db";
        optionsBuilder.UseSqlite(connectionString);

        return new NawelDbContext(optionsBuilder.Options);
    }
}
