using Nawel.Api.Models;

namespace Nawel.Api.Data;

public static class DbSeeder
{
    public static void SeedTestData(NawelDbContext context)
    {
        // Check if data already exists
        if (context.Users.Any())
        {
            Console.WriteLine("Database already seeded");
            return;
        }

        Console.WriteLine("Seeding test data...");

        // Create families
        var familyNironi = new Family { Name = "Nironi" };
        var familyElfassi = new Family { Name = "Elfassi" };
        context.Families.AddRange(familyNironi, familyElfassi);
        context.SaveChanges();

        // Create users
        // Password for all test users: "password123" (BCrypt hashed)
        // Admin password: "admin" (BCrypt hashed)
        var users = new[]
        {
            new User
            {
                Login = "admin",
                Password = BCrypt.Net.BCrypt.HashPassword("admin"),
                Email = "admin@nawel.com",
                FirstName = "Admin",
                LastName = "System",
                Avatar = "avatar.png",
                Pseudo = "Admin",
                IsAdmin = true, // Set first user as admin
                FamilyId = familyNironi.Id
            },
            new User
            {
                Login = "sylvain",
                Password = BCrypt.Net.BCrypt.HashPassword("password123"),
                Email = "sylvain@nawel.com",
                FirstName = "Sylvain",
                LastName = "Nironi",
                Avatar = "avatar.png",
                Pseudo = "Sylvain",
                NotifyListEdit = true,
                NotifyGiftTaken = true,
                FamilyId = familyNironi.Id
            },
            new User
            {
                Login = "claire",
                Password = BCrypt.Net.BCrypt.HashPassword("password123"),
                Email = "claire@nawel.com",
                FirstName = "Claire",
                LastName = "Nironi",
                Avatar = "avatar.png",
                Pseudo = "Claire",
                NotifyListEdit = true,
                FamilyId = familyNironi.Id
            },
            new User
            {
                Login = "marie",
                Password = BCrypt.Net.BCrypt.HashPassword("password123"),
                Email = "marie@nawel.com",
                FirstName = "Marie",
                LastName = "Nironi",
                Avatar = "avatar.png",
                Pseudo = "Marie",
                NotifyListEdit = true,
                FamilyId = familyNironi.Id
            }
        };

        context.Users.AddRange(users);
        context.SaveChanges();

        // Create lists for each user
        foreach (var user in users)
        {
            var list = new GiftList
            {
                Name = $"Liste de {user.FirstName}",
                UserId = user.Id
            };
            context.Lists.Add(list);
        }
        context.SaveChanges();

        // Add some test gifts
        var sylvainList = context.Lists.First(l => l.User!.Login == "sylvain");
        var claireList = context.Lists.First(l => l.User!.Login == "claire");
        var marieList = context.Lists.First(l => l.User!.Login == "marie");

        var gifts = new[]
        {
            new Gift
            {
                ListId = sylvainList.Id,
                Name = "Livre de cuisine",
                Description = "Un beau livre de recettes",
                Image = "https://via.placeholder.com/150",
                Link = "https://example.com",
                Cost = 29.99m,
                Currency = "EUR",
                Year = DateTime.Now.Year
            },
            new Gift
            {
                ListId = sylvainList.Id,
                Name = "Casque audio",
                Description = "Casque Bluetooth",
                Image = "https://via.placeholder.com/150",
                Link = "https://example.com",
                Cost = 79.99m,
                Currency = "EUR",
                Year = DateTime.Now.Year
            },
            new Gift
            {
                ListId = claireList.Id,
                Name = "Sac à main",
                Description = "Sac élégant",
                Image = "https://via.placeholder.com/150",
                Link = "https://example.com",
                Cost = 120.00m,
                Currency = "EUR",
                Year = DateTime.Now.Year
            },
            new Gift
            {
                ListId = marieList.Id,
                Name = "Parfum",
                Description = "Eau de toilette",
                Image = "https://via.placeholder.com/150",
                Link = "https://example.com",
                Cost = 65.00m,
                Currency = "EUR",
                Year = DateTime.Now.Year
            }
        };

        context.Gifts.AddRange(gifts);
        context.SaveChanges();

        Console.WriteLine("Test data seeded successfully!");
        Console.WriteLine("Test users:");
        Console.WriteLine("  - admin / admin");
        Console.WriteLine("  - sylvain / password123");
        Console.WriteLine("  - claire / password123");
        Console.WriteLine("  - marie / password123");
    }
}
