using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Nawel.Api.Data;

namespace Nawel.Api.Tests.Integration;

public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Remove the app's DbContext registrations
            var dbContextDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<NawelDbContext>));

            if (dbContextDescriptor != null)
            {
                services.Remove(dbContextDescriptor);
            }

            // Add InMemory database for testing
            services.AddDbContext<NawelDbContext>(options =>
            {
                options.UseInMemoryDatabase("InMemoryTestDb_" + Guid.NewGuid().ToString());
            });

            // Seed test data after the app is built
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<NawelDbContext>();
            db.Database.EnsureCreated();
            SeedTestData(db);
        });
    }

    private static void SeedTestData(NawelDbContext context)
    {
        // Clear existing data
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();

        // Seed test family
        var family = new Nawel.Api.Models.Family
        {
            Id = 1,
            Name = "Test Family"
        };
        context.Families.Add(family);

        // Seed test users
        var adminUser = new Nawel.Api.Models.User
        {
            Id = 1,
            Login = "admin",
            Password = BCrypt.Net.BCrypt.HashPassword("admin123"),
            Email = "admin@test.com",
            FirstName = "Admin",
            LastName = "User",
            IsAdmin = true,
            FamilyId = 1
        };

        var regularUser = new Nawel.Api.Models.User
        {
            Id = 2,
            Login = "user",
            Password = BCrypt.Net.BCrypt.HashPassword("user123"),
            Email = "user@test.com",
            FirstName = "Regular",
            LastName = "User",
            IsAdmin = false,
            FamilyId = 1
        };

        var childUser = new Nawel.Api.Models.User
        {
            Id = 3,
            Login = "child",
            Password = BCrypt.Net.BCrypt.HashPassword("child123"),
            Email = "child@test.com",
            FirstName = "Child",
            LastName = "User",
            IsChildren = true,
            IsAdmin = false,
            FamilyId = 1
        };

        context.Users.AddRange(adminUser, regularUser, childUser);

        // Seed gift lists
        var adminList = new Nawel.Api.Models.GiftList
        {
            Id = 1,
            UserId = 1
        };

        var userList = new Nawel.Api.Models.GiftList
        {
            Id = 2,
            UserId = 2
        };

        var childList = new Nawel.Api.Models.GiftList
        {
            Id = 3,
            UserId = 3
        };

        context.Lists.AddRange(adminList, userList, childList);

        // Seed some gifts
        var gift1 = new Nawel.Api.Models.Gift
        {
            Id = 1,
            Name = "Test Gift 1",
            Description = "Description 1",
            ListId = 2,
            Year = DateTime.UtcNow.Year,
            Available = true,
            Cost = 29.99m
        };

        var gift2 = new Nawel.Api.Models.Gift
        {
            Id = 2,
            Name = "Test Gift 2",
            Description = "Description 2",
            ListId = 2,
            Year = DateTime.UtcNow.Year,
            Available = true,
            IsGroupGift = true
        };

        context.Gifts.AddRange(gift1, gift2);

        context.SaveChanges();
    }
}
