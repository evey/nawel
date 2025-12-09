# Architecture Backend - Nawel API

## Vue d'Ensemble

L'API Nawel est construite avec **ASP.NET Core 9.0** et suit une architecture en couches inspirée de la Clean Architecture. Elle gère les listes de cadeaux familiales avec authentification sécurisée, notifications par email, et extraction automatique d'informations produits.

### Stack Technique

| Composant | Technologie | Version |
|-----------|-------------|---------|
| Framework | ASP.NET Core | 9.0 |
| ORM | Entity Framework Core | 9.0 |
| Base de données (Prod) | MySQL | 8.0 |
| Base de données (Dev) | SQLite | - |
| Authentification | JWT Bearer | - |
| Hashing passwords | BCrypt.Net-Next | 4.0.3 |
| Email | MailKit | 4.14.1 |
| Rate Limiting | AspNetCoreRateLimit | 5.0.0 |
| Web Scraping | HtmlAgilityPack | 1.12.4 |
| Documentation API | Swashbuckle (Swagger) | 10.0.1 |

## Architecture en Couches

```
┌─────────────────────────────────────────────────┐
│              HTTP Request                        │
└─────────────────┬───────────────────────────────┘
                  │
┌─────────────────▼───────────────────────────────┐
│           Middleware Pipeline                    │
│  - Exception Handler                             │
│  - Rate Limiting                                 │
│  - CORS                                          │
│  - Authentication / Authorization                │
└─────────────────┬───────────────────────────────┘
                  │
┌─────────────────▼───────────────────────────────┐
│            Controllers Layer                     │
│  - AuthController                                │
│  - AdminController                               │
│  - UsersController                               │
│  - ListsController                               │
│  - GiftsController                               │
│  - ProductsController                            │
└─────────────────┬───────────────────────────────┘
                  │
┌─────────────────▼───────────────────────────────┐
│            Services Layer                        │
│  - AuthService (IAuthService)                    │
│  - JwtService (IJwtService)                      │
│  - EmailService (IEmailService)                  │
│  - ProductInfoExtractor (IProductInfoExtractor)  │
│  - NotificationDebouncers                        │
└─────────────────┬───────────────────────────────┘
                  │
┌─────────────────▼───────────────────────────────┐
│              Data Layer                          │
│  - NawelDbContext (EF Core)                      │
│  - Models (User, Gift, Family, etc.)            │
│  - Migrations                                    │
└─────────────────┬───────────────────────────────┘
                  │
┌─────────────────▼───────────────────────────────┐
│            Database                              │
│  - MySQL 8.0 (Production)                        │
│  - SQLite (Development)                          │
└──────────────────────────────────────────────────┘
```

## Structure des Répertoires

```
backend/Nawel.Api/
├── Controllers/           # Contrôleurs API (6 contrôleurs)
├── Services/             # Services métier
│   ├── Auth/            # Authentification et JWT
│   ├── Email/           # Envoi d'emails et debouncing
│   └── ProductInfo/     # Extraction métadonnées produits
├── Data/                # Contexte EF Core et seeding
├── Models/              # Entités de domaine
├── DTOs/                # Data Transfer Objects
├── Middleware/          # Middleware personnalisés
├── Authorization/       # Policies et handlers d'autorisation
├── Configuration/       # Classes de configuration
├── Constants/           # Constantes de l'application
├── Exceptions/          # Exceptions personnalisées
├── Extensions/          # Extension methods
├── Migrations/          # Migrations EF Core
├── uploads/             # Fichiers uploadés (avatars)
├── Program.cs           # Point d'entrée et configuration
└── appsettings.json     # Configuration de l'application
```

## Couche Controllers

Les contrôleurs suivent le pattern **API REST** avec les conventions suivantes :

### Responsabilités
- Validation des requêtes HTTP
- Gestion de l'authentification/autorisation
- Délégation de la logique métier aux services
- Transformation des résultats en réponses HTTP

### Conventions
- Route pattern : `api/[controller]/[action]`
- Attributs : `[ApiController]`, `[Route]`, `[Authorize]`
- Codes HTTP standards (200, 201, 400, 401, 403, 404, 500)
- Retours typés avec `ActionResult<T>`

### Exemple : GiftsController

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GiftsController : ControllerBase
{
    private readonly NawelDbContext _context;
    private readonly IEmailService _emailService;

    [HttpGet("my-list")]
    public async Task<ActionResult<IEnumerable<GiftDto>>> GetMyGifts([FromQuery] int? year = null)
    {
        // Récupération de l'utilisateur depuis les claims JWT
        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        // Logique métier
        var gifts = await _context.Gifts
            .Include(g => g.TakenByUser)
            .Where(g => g.ListId == userListId && g.Year == currentYear)
            .ToListAsync();

        return Ok(gifts.Select(g => g.ToDto()));
    }
}
```

## Couche Services

Les services encapsulent la logique métier et les dépendances externes.

### AuthService

**Responsabilités** :
- Authentification avec support MD5 legacy et BCrypt
- Génération et validation de tokens de réinitialisation
- Migration automatique MD5 → BCrypt
- Gestion des mots de passe

**Méthodes clés** :
```csharp
Task<User?> AuthenticateAsync(string login, string password)  // Peut lancer LegacyPasswordException
Task<string> GenerateResetTokenAsync(string email)
Task<bool> ValidateResetTokenAsync(string token)
Task<bool> ResetPasswordAsync(string token, string newPassword)
Task<bool> UpdatePasswordAsync(int userId, string newPassword)
```

**Détection MD5** :
```csharp
// Détecte si un mot de passe est en MD5 (32 hex chars, pas BCrypt)
if (user.Password.Length == 32 && !user.Password.StartsWith("$2"))
{
    // Lancer exception pour forcer migration
    throw new LegacyPasswordException(...);
}
```

### JwtService

**Responsabilités** :
- Génération de tokens JWT avec claims personnalisés
- Validation et parsing de tokens JWT
- Configuration de l'expiration et des algorithmes

**Configuration JWT** :
```csharp
// Claims standards + personnalisés
new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
new Claim(ClaimTypes.Name, user.Login)
new Claim("FirstName", user.FirstName ?? "")
new Claim("LastName", user.LastName ?? "")
new Claim("Avatar", user.Avatar ?? "")
```

**Sécurité** :
- Algorithme : HS256
- Secret : 256+ bits (validé au startup)
- Expiration : Configurable (par défaut : 7 jours)
- Validation : Issuer, Audience, Lifetime, Signature

### EmailService

**Responsabilités** :
- Envoi d'emails via SMTP (MailKit)
- Templates HTML pour différents types d'emails
- Debouncing des notifications (2 minutes)

**Types d'emails** :
1. **Réservation de cadeau** : Notification au propriétaire
2. **Participation cadeau groupé** : Notification au propriétaire
3. **Édition de liste** : Notification agrégée aux réserveurs
4. **Migration MD5** : Email de réinitialisation sécurisé

**Debouncing Pattern** :
```csharp
// Évite le spam d'emails en regroupant les notifications
public interface INotificationDebouncer
{
    void ScheduleListEditNotification(int ownerId, string ownerName, string action, string giftName);
}

// Implémentation avec Timer de 2 minutes
// Les notifications sont agrégées puis envoyées en un seul email
```

### ProductInfoExtractor

**Responsabilités** :
- Extraction de métadonnées depuis URLs externes
- Parsing HTML avec HtmlAgilityPack
- Intégration avec OpenGraph API
- Tracking des requêtes pour monitoring

**Workflow** :
1. Appel à l'API OpenGraph (opengraph.io)
2. Parsing de la réponse JSON
3. Extraction : titre, prix, image, description
4. Enregistrement dans `OpenGraphRequests` pour stats admin

## Couche Data

### NawelDbContext

Le contexte Entity Framework Core gère toutes les entités et relations.

**Entités principales** :
- `User` : Utilisateurs (avec roles admin, enfants)
- `Family` : Groupes familiaux
- `GiftList` : Listes de cadeaux (1 par user)
- `Gift` : Cadeaux individuels
- `GiftParticipation` : Participations aux cadeaux groupés
- `OpenGraphRequest` : Tracking API externe

**Configuration** :
```csharp
// Dual database provider support
if (useSqlite)
{
    options.UseSqlite(connectionString);
}
else
{
    options.UseMySql(connectionString, serverVersion);
}
```

**Seeding** :
```csharp
public static class DbSeeder
{
    public static void SeedTestData(NawelDbContext context)
    {
        // Crée l'utilisateur admin et des données de test
        // Seulement en développement avec SQLite
    }
}
```

### Migrations

Les migrations EF Core sont gérées avec les commandes :
```bash
# Créer une migration
dotnet ef migrations add NomMigration

# Appliquer les migrations
dotnet ef database update

# Rollback
dotnet ef database update PreviousMigration
```

## Middleware Pipeline

L'ordre des middleware est crucial pour le bon fonctionnement de l'application.

### Ordre d'Exécution (Program.cs)

```csharp
// 1. Exception Handler (doit être en premier)
app.UseExceptionHandler();

// 2. Swagger UI (documentation)
app.UseSwagger();
app.UseSwaggerUI();

// 3. Rate Limiting (avant auth)
app.UseIpRateLimiting();

// 4. CORS
app.UseCors("AllowFrontend");

// 5. Static Files
app.UseStaticFiles();

// 6. Authentication
app.UseAuthentication();

// 7. Authorization
app.UseAuthorization();

// 8. Routing vers controllers
app.MapControllers();
```

### GlobalExceptionHandler

Middleware personnalisé pour la gestion centralisée des exceptions.

**Fonctionnalités** :
- Capture toutes les exceptions non gérées
- Log avec ILogger
- Retourne des réponses JSON standardisées
- Masque les détails techniques en production

```csharp
public class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Unhandled exception occurred");

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await httpContext.Response.WriteAsJsonAsync(new
        {
            message = "An internal server error occurred"
        });

        return true;
    }
}
```

## Authentification et Autorisation

### JWT Bearer Authentication

**Configuration** (Program.cs) :
```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });
```

### Authorization Policies

**Policy "AdminOnly"** :
```csharp
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.Requirements.Add(new AdminRequirement()));
});

// Custom handler
public class AdminAuthorizationHandler : AuthorizationHandler<AdminRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        AdminRequirement requirement)
    {
        var isAdminClaim = context.User.FindFirst("IsAdmin");
        if (isAdminClaim != null && bool.Parse(isAdminClaim.Value))
        {
            context.Succeed(requirement);
        }
        return Task.CompletedTask;
    }
}
```

**Usage dans les contrôleurs** :
```csharp
[Authorize(Policy = "AdminOnly")]
public class AdminController : ControllerBase
{
    // Tous les endpoints nécessitent le rôle admin
}
```

## Configuration

### Structure appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "server=localhost;database=nawel_db;user=root;password=***",
    "SqliteConnection": "Data Source=nawel.db"
  },
  "UseSqlite": true,
  "Jwt": {
    "Secret": "***",
    "Issuer": "NawelApi",
    "Audience": "NawelApp",
    "ExpiryDays": 7
  },
  "Email": {
    "SmtpServer": "smtp.example.com",
    "SmtpPort": 587,
    "UseSsl": true,
    "Username": "***",
    "Password": "***",
    "FromEmail": "noreply@nawel.com",
    "FromName": "Nawel App"
  },
  "FileStorage": {
    "AvatarsPath": "uploads/avatars",
    "MaxFileSizeMB": 5,
    "AllowedExtensions": [".jpg", ".jpeg", ".png", ".gif", ".webp"]
  },
  "OpenGraph": {
    "ApiKey": "***",
    "BaseUrl": "https://opengraph.io/api/1.1/site/"
  },
  "IpRateLimiting": {
    "EnableEndpointRateLimiting": true,
    "StackBlockedRequests": false,
    "RealIpHeader": "X-Real-IP",
    "ClientIdHeader": "X-ClientId",
    "HttpStatusCode": 429
  }
}
```

### Variables d'Environnement

Les secrets sensibles sont overridés par des variables d'environnement :

```bash
JWT_SECRET=***
JWT_ISSUER=NawelApi
JWT_AUDIENCE=NawelApp
EMAIL_SMTP_PASSWORD=***
OPENGRAPH_API_KEY=***
```

### Validation au Startup

```csharp
public class JwtSettings
{
    public void Validate()
    {
        if (string.IsNullOrEmpty(Secret) || Secret.Length < 32)
            throw new InvalidOperationException("JWT Secret must be at least 32 characters");

        if (string.IsNullOrEmpty(Issuer))
            throw new InvalidOperationException("JWT Issuer is required");

        // ... autres validations
    }
}
```

## Patterns Utilisés

### Repository Pattern (Implicite)

Entity Framework Core agit comme un Repository Pattern :
- `DbSet<T>` = Repository pour chaque entité
- Queries LINQ = Specification pattern
- `SaveChangesAsync()` = Unit of Work

### Service Layer Pattern

- Séparation claire entre contrôleurs et logique métier
- Interfaces pour l'injection de dépendances
- Testabilité accrue

### DTO Pattern

Séparation entre entités de domaine et objets de transfert :

```csharp
// Entité (Models/)
public class User
{
    public int Id { get; set; }
    public string Password { get; set; }  // Ne doit jamais être exposé
    // ...
}

// DTO (DTOs/)
public class UserDto
{
    public int Id { get; set; }
    // Pas de password !
    public string Login { get; set; }
    // ...
}

// Extension method pour conversion
public static UserDto ToDto(this User user)
{
    return new UserDto { /* mapping */ };
}
```

### Dependency Injection

Toutes les dépendances sont injectées via le constructeur :

```csharp
// Enregistrement (Program.cs)
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddSingleton<INotificationDebouncer, NotificationDebouncer>();

// Utilisation (Controller)
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }
}
```

**Lifetimes** :
- **Scoped** : Services métier, DbContext (1 instance par requête)
- **Singleton** : Configuration, debouncers (1 instance globale)
- **Transient** : Non utilisé dans ce projet

## Rate Limiting

### Configuration par Endpoint

```json
"IpRateLimitPolicies": {
  "IpRules": [
    {
      "Ip": "*",
      "Rules": [
        {
          "Endpoint": "POST:/api/auth/login",
          "Period": "1m",
          "Limit": 5
        },
        {
          "Endpoint": "*",
          "Period": "15m",
          "Limit": 100
        }
      ]
    }
  ]
}
```

### Objectifs

- **Protection contre le brute force** : 5 tentatives de login / minute
- **Protection DDoS** : 100 requêtes / 15 minutes maximum
- **Par IP** : Chaque client a sa propre limite

## Sécurité

### Bonnes Pratiques Implémentées

1. **Passwords** :
   - BCrypt avec 10+ rounds (coût computationnel élevé)
   - Migration forcée depuis MD5
   - Validation de complexité (à implémenter)

2. **JWT** :
   - Secret de 256+ bits minimum
   - Validation stricte (issuer, audience, lifetime)
   - Pas de stockage côté serveur (stateless)

3. **CORS** :
   - Origins explicitement définis
   - Pas de wildcard en production

4. **Rate Limiting** :
   - Protection endpoints critiques (login)
   - Limite globale par IP

5. **File Upload** :
   - Validation extension et taille
   - Noms de fichiers avec GUID (pas de path traversal)
   - Limite 5MB par défaut

6. **SQL Injection** :
   - Protection automatique via EF Core (parameterized queries)

7. **XSS** :
   - Protection automatique via JSON serialization
   - Pas de HTML direct dans les réponses

### Points d'Attention

- **Secrets** : Tous les secrets doivent être en variables d'environnement
- **HTTPS** : Obligatoire en production (non forcé en dev)
- **CSRF** : Non nécessaire (API stateless, pas de cookies)

## Performance

### Optimisations Implémentées

1. **Queries EF Core** :
   - `Include()` explicite pour éviter N+1
   - `AsNoTracking()` pour lectures seules (à implémenter)
   - Indexes sur colonnes fréquemment recherchées

2. **Caching** :
   - MemoryCache pour rate limiting
   - Pas de caching applicatif (à implémenter si besoin)

3. **Async/Await** :
   - Toutes les opérations I/O sont asynchrones
   - Libération des threads pendant les attentes

4. **Debouncing** :
   - Emails agrégés (évite spam)
   - Timer de 2 minutes avant envoi

### Métriques à Surveiller

- Temps de réponse API (< 200ms idéal)
- Utilisation mémoire (GC)
- Connexions DB actives
- Rate de requêtes OpenGraph (coût externe)

## Tests

### Structure (Nawel.Api.Tests/)

```
Nawel.Api.Tests/
├── Integration/        # Tests d'intégration (à implémenter)
├── Unit/              # Tests unitaires (à implémenter)
└── Helpers/           # Utilitaires de test
```

### Stratégie de Test

1. **Tests Unitaires** :
   - Services (AuthService, JwtService, etc.)
   - Extension methods
   - Validation logic

2. **Tests d'Intégration** :
   - Endpoints controllers
   - Base de données (in-memory)
   - Authentification end-to-end

3. **Tests à Implémenter** :
   - Couverture cible : 70%+
   - Focus sur logique métier critique

## Déploiement

### Docker

Le projet inclut un `Dockerfile` multi-stage optimisé :

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
# ... build stage ...

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
# ... runtime stage ...
```

**Avantages** :
- Image finale légère (runtime only)
- Build reproductible
- Isolation complète

### Variables d'Environnement Requises

```bash
# Base de données
ConnectionStrings__DefaultConnection=***

# JWT
JWT_SECRET=***  # Minimum 32 caractères

# Email (optionnel en dev)
EMAIL_SMTP_PASSWORD=***

# OpenGraph API
OPENGRAPH_API_KEY=***
```

### Commandes Docker

```bash
# Build
docker build -t nawel-api .

# Run
docker run -p 5000:8080 \
  -e JWT_SECRET="your-secret-here" \
  -e ConnectionStrings__DefaultConnection="server=mysql;database=nawel" \
  nawel-api

# Avec Docker Compose
docker-compose up -d
```

## Logging

### Configuration

```csharp
// Utilisation de ILogger<T> partout
private readonly ILogger<GiftsController> _logger;

// Niveaux de log
_logger.LogInformation("User {UserId} logged in", userId);
_logger.LogWarning("Failed login attempt for {Login}", login);
_logger.LogError(exception, "Error processing gift {GiftId}", giftId);
```

### Structured Logging

Les logs utilisent le format structuré :
- Paramètres nommés : `{UserId}`, `{Login}`
- Facilite le parsing et l'indexation
- Compatible Serilog, NLog, etc.

## Extensibilité

### Ajouter un Nouveau Contrôleur

1. Créer le contrôleur dans `Controllers/`
2. Ajouter les annotations XML pour Swagger
3. Injecter les services nécessaires
4. Suivre les conventions REST

### Ajouter un Nouveau Service

1. Créer l'interface `IMyService`
2. Implémenter la classe `MyService`
3. Enregistrer dans `Program.cs` : `builder.Services.AddScoped<IMyService, MyService>()`
4. Injecter dans les contrôleurs

### Ajouter une Migration

```bash
dotnet ef migrations add AddNewFeature
dotnet ef database update
```

## Références

- [ASP.NET Core Documentation](https://learn.microsoft.com/en-us/aspnet/core/)
- [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/)
- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [REST API Best Practices](https://learn.microsoft.com/en-us/azure/architecture/best-practices/api-design)
