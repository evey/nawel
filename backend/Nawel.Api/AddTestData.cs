using Microsoft.EntityFrameworkCore;
using Nawel.Api.Data;
using Nawel.Api.Models;

namespace Nawel.Api;

public class AddTestData
{
    public static async Task AddSylvainTestGifts(NawelDbContext context)
    {
        // Trouver Sylvain
        var sylvain = await context.Users.FirstOrDefaultAsync(u => u.Login == "sylvain");
        if (sylvain == null)
        {
            Console.WriteLine("Utilisateur Sylvain non trouvé");
            return;
        }

        // Trouver Claire pour la réservation
        var claire = await context.Users.FirstOrDefaultAsync(u => u.Login == "claire");
        if (claire == null)
        {
            Console.WriteLine("Utilisateur Claire non trouvé");
            return;
        }

        // Trouver ou créer la liste de Sylvain
        var sylvainList = await context.Lists.FirstOrDefaultAsync(l => l.UserId == sylvain.Id);
        if (sylvainList == null)
        {
            sylvainList = new GiftList
            {
                Name = "Liste 2024",
                UserId = sylvain.Id
            };
            context.Lists.Add(sylvainList);
            await context.SaveChangesAsync();
        }

        // Vérifier si des cadeaux 2024 existent déjà
        var existing2024Gifts = await context.Gifts
            .Where(g => g.ListId == sylvainList.Id && g.Year == 2024)
            .CountAsync();

        if (existing2024Gifts > 0)
        {
            Console.WriteLine($"Sylvain a déjà {existing2024Gifts} cadeau(x) pour 2024");
            return;
        }

        // Ajouter un cadeau libre
        var giftFree = new Gift
        {
            Name = "Livre de science-fiction",
            Description = "Un bon roman de science-fiction, idéalement de la série Fondation d'Asimov",
            Link = "https://www.amazon.fr/Fondation-Isaac-Asimov/dp/2070360539",
            Image = "https://m.media-amazon.com/images/I/51VVQGjZr5L._SY445_SX342_.jpg",
            Cost = 15.90m,
            Currency = "EUR",
            Available = true,
            TakenBy = null,
            IsGroupGift = false,
            Year = 2024,
            ListId = sylvainList.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Ajouter un cadeau réservé
        var giftReserved = new Gift
        {
            Name = "Casque audio Bluetooth",
            Description = "Un casque audio de bonne qualité avec réduction de bruit",
            Link = "https://www.amazon.fr/Sony-WH-1000XM4-Bluetooth-R%C3%A9duction-Argent/dp/B08C7KG5LP",
            Image = "https://m.media-amazon.com/images/I/71o8Q5XJS5L._AC_SX679_.jpg",
            Cost = 279.00m,
            Currency = "EUR",
            Available = false,
            TakenBy = claire.Id,
            IsGroupGift = false,
            Year = 2024,
            ListId = sylvainList.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.Gifts.Add(giftFree);
        context.Gifts.Add(giftReserved);
        await context.SaveChangesAsync();

        Console.WriteLine("2 cadeaux ajoutés pour Sylvain en 2024 :");
        Console.WriteLine($"  - {giftFree.Name} (libre)");
        Console.WriteLine($"  - {giftReserved.Name} (réservé par {claire.FirstName ?? claire.Login})");
    }

    public static async Task AddClaireTestGifts(NawelDbContext context)
    {
        // Trouver Claire
        var claire = await context.Users.FirstOrDefaultAsync(u => u.Login == "claire");
        if (claire == null)
        {
            Console.WriteLine("Utilisateur Claire non trouvé");
            return;
        }

        // Trouver Sylvain pour la réservation
        var sylvain = await context.Users.FirstOrDefaultAsync(u => u.Login == "sylvain");
        if (sylvain == null)
        {
            Console.WriteLine("Utilisateur Sylvain non trouvé");
            return;
        }

        // Trouver ou créer la liste de Claire
        var claireList = await context.Lists.FirstOrDefaultAsync(l => l.UserId == claire.Id);
        if (claireList == null)
        {
            claireList = new GiftList
            {
                Name = "Liste 2024",
                UserId = claire.Id
            };
            context.Lists.Add(claireList);
            await context.SaveChangesAsync();
        }

        // Vérifier si des cadeaux 2024 existent déjà
        var existing2024Gifts = await context.Gifts
            .Where(g => g.ListId == claireList.Id && g.Year == 2024)
            .CountAsync();

        if (existing2024Gifts > 0)
        {
            Console.WriteLine($"Claire a déjà {existing2024Gifts} cadeau(x) pour 2024");
            return;
        }

        // Ajouter un cadeau libre
        var giftFree1 = new Gift
        {
            Name = "Sac à main en cuir",
            Description = "Un sac à main élégant en cuir véritable, de préférence marron ou noir",
            Link = "https://www.amazon.fr/s?k=sac+main+cuir+femme",
            Image = "https://m.media-amazon.com/images/I/71vFKBE-jAL._AC_SX679_.jpg",
            Cost = 89.90m,
            Currency = "EUR",
            Available = true,
            TakenBy = null,
            IsGroupGift = false,
            Year = 2024,
            ListId = claireList.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Ajouter un autre cadeau libre
        var giftFree2 = new Gift
        {
            Name = "Coffret de thés premium",
            Description = "Un coffret découverte de thés du monde avec différentes saveurs",
            Link = "https://www.amazon.fr/Kusmi-Tea-Coffret-Wellness-50g/dp/B07YZCVVWT",
            Image = "https://m.media-amazon.com/images/I/81QHJ9cOtTL._AC_SX679_.jpg",
            Cost = 35.00m,
            Currency = "EUR",
            Available = true,
            TakenBy = null,
            IsGroupGift = false,
            Year = 2024,
            ListId = claireList.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Ajouter un cadeau réservé par Sylvain
        var giftReserved = new Gift
        {
            Name = "Bougie parfumée Diptyque",
            Description = "Une bougie de luxe avec une senteur élégante",
            Link = "https://www.diptyqueparis.com/fr_fr/p/bougie-baies.html",
            Image = "https://www.diptyqueparis.com/dw/image/v2/BCNQ_PRD/on/demandware.static/-/Sites-dip-master-catalog/default/dw37e8b7f9/images/large/CANBAIE190_1.png",
            Cost = 68.00m,
            Currency = "EUR",
            Available = false,
            TakenBy = sylvain.Id,
            IsGroupGift = false,
            Year = 2024,
            ListId = claireList.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.Gifts.Add(giftFree1);
        context.Gifts.Add(giftFree2);
        context.Gifts.Add(giftReserved);
        await context.SaveChangesAsync();

        Console.WriteLine("3 cadeaux ajoutés pour Claire en 2024 :");
        Console.WriteLine($"  - {giftFree1.Name} (libre)");
        Console.WriteLine($"  - {giftFree2.Name} (libre)");
        Console.WriteLine($"  - {giftReserved.Name} (réservé par {sylvain.FirstName ?? sylvain.Login})");
    }
}
