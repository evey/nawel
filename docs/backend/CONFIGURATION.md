# Guide de Configuration Backend - Nawel API

## Vue d'Ensemble

L'API Nawel utilise un système de configuration flexible basé sur :
- **appsettings.json** : Configuration de base (commitée dans Git)
- **appsettings.{Environment}.json** : Configuration spécifique par environnement
- **Variables d'environnement** : Surcharge prioritaire pour les secrets et configurations sensibles
- **Settings Classes** : Classes C# typées avec validation automatique

**Priorité de configuration** (du plus faible au plus fort) :
```
appsettings.json → appsettings.{Environment}.json → Variables d'environnement
```

---

## Table des Matières

1. [Structure des Fichiers](#structure-des-fichiers)
2. [appsettings.json Complet](#appsettingsjson-complet)
3. [Variables d'Environnement](#variables-denvironnement)
4. [Configuration JWT](#configuration-jwt)
5. [Configuration Email/SMTP](#configuration-emailsmtp)
6. [Configuration Base de Données](#configuration-base-de-données)
7. [Configuration Rate Limiting](#configuration-rate-limiting)
8. [Configuration CORS](#configuration-cors)
9. [Configuration File Storage](#configuration-file-storage)
10. [Configuration OpenGraph API](#configuration-opengraph-api)
11. [Sécurité et Best Practices](#sécurité-et-best-practices)
12. [Configuration par Environnement](#configuration-par-environnement)
13. [Docker et Déploiement](#docker-et-déploiement)
14. [Dépannage](#dépannage)

---

## Structure des Fichiers

```
backend/Nawel.Api/
├── appsettings.json                    # Configuration de base (COMMITÉ)
├── appsettings.Development.json        # Config dev (optionnel)
├── appsettings.Production.json         # Config prod (NON COMMITÉ)
├── appsettings.RateLimiting.json       # Config rate limiting détaillée
├── appsettings.Testing.json            # Config pour tests
├── .env.example                        # Template variables d'env (COMMITÉ)
└── Configuration/                      # Classes Settings typées
    ├── JwtSettings.cs
    ├── EmailSettings.cs
    └── FileStorageSettings.cs
```

**Fichiers à ne JAMAIS commiter** :
- `appsettings.Production.json` (contient secrets)
- `.env` (variables d'environnement locales)
- `appsettings.*.json` avec secrets réels

---

## appsettings.json Complet

### Structure Complète

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=nawel;User=nawel_user;Password=nawel_pass;",
    "SqliteConnection": "Data Source=nawel.db"
  },
  "Jwt": {
    "Secret": "CHANGE_ME_IN_PRODUCTION_USE_JWT_SECRET_ENV_VAR_minimum_32_chars",
    "Issuer": "NawelApi",
    "Audience": "NawelApp",
    "ExpirationMinutes": 60,
    "Comment": "DO NOT use this default secret in production! Set JWT_SECRET environment variable instead."
  },
  "Email": {
    "Enabled": false,
    "SmtpHost": "localhost",
    "SmtpPort": 1025,
    "SmtpUsername": "",
    "SmtpPassword": "",
    "FromEmail": "no-reply@nawel.com",
    "FromName": "Nawel - Listes de Noël",
    "UseSsl": false,
    "NotificationDelayMinutes": 2,
    "ReservationNotificationDelayMinutes": 2
  },
  "OpenGraph": {
    "Enabled": true,
    "ApiKey": "YOUR_OPENGRAPH_API_KEY_HERE",
    "Comment": "Get your free API key at https://www.opengraph.io/ (1000 requests/month free). Set OPENGRAPH_API_KEY environment variable in production."
  },
  "FileStorage": {
    "AvatarsPath": "uploads/avatars",
    "MaxFileSizeMB": 5,
    "AllowedExtensions": [".jpg", ".jpeg", ".png", ".gif", ".webp"]
  },
  "UseSqlite": true,
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "IpRateLimiting": {
    "EnableEndpointRateLimiting": true,
    "StackBlockedRequests": false,
    "RealIpHeader": "X-Real-IP",
    "ClientIdHeader": "X-ClientId",
    "HttpStatusCode": 429,
    "GeneralRules": [
      {
        "Endpoint": "*",
        "Period": "1m",
        "Limit": 60
      },
      {
        "Endpoint": "*",
        "Period": "1h",
        "Limit": 1000
      }
    ],
    "QuotaExceededResponse": {
      "Content": "{{ \"message\": \"Rate limit exceeded. Please try again later.\" }}",
      "ContentType": "application/json"
    }
  },
  "IpRateLimitPolicies": {
    "IpRules": [
      {
        "Ip": "127.0.0.1",
        "Rules": [
          {
            "Endpoint": "*",
            "Period": "1s",
            "Limit": 1000
          }
        ]
      }
    ]
  }
}
```

---

## Variables d'Environnement

### Priorité et Surcharge

Les variables d'environnement **surchargent** les valeurs de `appsettings.json`.

**Exemple** : Si `JWT_SECRET` est définie comme variable d'environnement, elle remplace `Jwt:Secret` du fichier JSON.

### Variables d'Environnement Supportées

| Variable | Description | Obligatoire | Exemple |
|----------|-------------|-------------|---------|
| `JWT_SECRET` | Secret JWT (minimum 32 chars) | ✅ Production | `your_super_secret_jwt_key_at_least_32_characters_long` |
| `JWT_ISSUER` | Émetteur des tokens JWT | ❌ | `NawelApi` |
| `JWT_AUDIENCE` | Audience des tokens JWT | ❌ | `NawelApp` |
| `ConnectionStrings__DefaultConnection` | Connection string MySQL | ✅ Production | `Server=mysql;Database=nawel;User=root;Password=***` |
| `EMAIL_SMTP_SERVER` | Serveur SMTP | ✅ Si email activé | `smtp.gmail.com` |
| `EMAIL_SMTP_PORT` | Port SMTP | ✅ Si email activé | `587` |
| `EMAIL_USERNAME` | Username SMTP | ✅ Si email activé | `your-email@gmail.com` |
| `EMAIL_PASSWORD` | Mot de passe SMTP | ✅ Si email activé | `your_app_password` |
| `OPENGRAPH_API_KEY` | Clé API OpenGraph.io | ❌ | `abc123def456` |
| `ASPNETCORE_ENVIRONMENT` | Environnement ASP.NET Core | ❌ | `Development`, `Production` |

### Syntaxe de Surcharge

ASP.NET Core supporte deux syntaxes pour surcharger des valeurs JSON imbriquées :

**Syntaxe 1** : Double underscore `__`
```bash
export ConnectionStrings__DefaultConnection="Server=mysql;..."
export Jwt__Secret="my_secret_key"
export Email__SmtpHost="smtp.gmail.com"
```

**Syntaxe 2** : Deux points `:` (non supporté dans tous les shells)
```bash
export "ConnectionStrings:DefaultConnection=Server=mysql;..."
```

**Recommandation** : Utilisez `__` (compatible partout).

### Fichier .env (Développement Local)

**Ne JAMAIS commiter** ce fichier. Utilisez `.env.example` comme template.

**Contenu typique** :
```bash
# JWT Configuration
JWT_SECRET=your_local_development_secret_at_least_32_characters_long
JWT_ISSUER=NawelApi
JWT_AUDIENCE=NawelApp

# Database (MySQL)
ConnectionStrings__DefaultConnection=Server=localhost;Port=3306;Database=nawel;User=nawel_user;Password=nawel_pass;

# Email (Gmail example)
EMAIL_SMTP_SERVER=smtp.gmail.com
EMAIL_SMTP_PORT=587
EMAIL_USERNAME=your-email@gmail.com
EMAIL_PASSWORD=your_gmail_app_password
EMAIL_FROM=no-reply@nawel.com
EMAIL_FROM_NAME=Nawel - Listes de Noël
EMAIL_USE_SSL=true

# OpenGraph API
OPENGRAPH_API_KEY=your_opengraph_api_key

# Environment
ASPNETCORE_ENVIRONMENT=Development
```

**Charger le fichier .env** (avec Docker Compose) :
```yaml
services:
  backend:
    env_file:
      - .env
```

---

## Configuration JWT

### Paramètres

| Paramètre | Type | Par Défaut | Description |
|-----------|------|------------|-------------|
| **Secret** | `string` | *REQUIRED* | Secret pour signer les tokens JWT (min 32 chars) |
| **Issuer** | `string` | `NawelApi` | Émetteur du token (vérifié à la validation) |
| **Audience** | `string` | `NawelApp` | Audience du token (vérifié à la validation) |
| **ExpirationMinutes** | `int` | `60` | Durée de vie du token en minutes |

### Validation Automatique

La classe `JwtSettings` valide automatiquement au démarrage :
- ✅ Secret non vide
- ✅ Secret ≥ 32 caractères (sécurité)
- ✅ Issuer non vide
- ✅ Audience non vide

**Code de validation** :
```csharp
public void Validate()
{
    if (string.IsNullOrWhiteSpace(Secret))
        throw new InvalidOperationException("JWT Secret must be configured");

    if (Secret.Length < 32)
        throw new InvalidOperationException("JWT Secret must be at least 32 characters long");

    if (string.IsNullOrWhiteSpace(Issuer))
        throw new InvalidOperationException("JWT Issuer must be configured");

    if (string.IsNullOrWhiteSpace(Audience))
        throw new InvalidOperationException("JWT Audience must be configured");
}
```

### Configuration Recommandée

**Développement** :
```json
{
  "Jwt": {
    "Secret": "dev_secret_key_minimum_32_characters_required_for_security",
    "Issuer": "NawelApi",
    "Audience": "NawelApp",
    "ExpirationMinutes": 120
  }
}
```

**Production** (via variables d'environnement) :
```bash
JWT_SECRET=<généré avec `openssl rand -base64 48`>
JWT_ISSUER=NawelApi
JWT_AUDIENCE=NawelApp
# ExpirationMinutes: 60 (par défaut, bon pour prod)
```

### Génération d'un Secret Sécurisé

**Linux/Mac** :
```bash
openssl rand -base64 48
```

**Windows (PowerShell)** :
```powershell
[Convert]::ToBase64String((1..48 | ForEach-Object { Get-Random -Minimum 0 -Maximum 256 }))
```

**Résultat** (exemple) :
```
K7vJ9mP2wX4nQ5tR8sL1cY6eU3oZ0dF7hA4gB2iM9kN6pV8q
```

### Token JWT Généré

**Structure du token** :
```json
{
  "sub": "2",
  "unique_name": "sylvain",
  "nameid": "2",
  "FirstName": "Sylvain",
  "LastName": "Nironi",
  "Avatar": "avatar.png",
  "jti": "bc3d77e6-a2dc-4741-a2d6-b2f6dde2d712",
  "exp": 1764869137,
  "iss": "NawelApi",
  "aud": "NawelApp"
}
```

**Durée de vie** :
- Par défaut : 60 minutes
- Configurable via `ExpirationMinutes`
- Recommandation : 60-120 min pour web app

---

## Configuration Email/SMTP

### Paramètres

| Paramètre | Type | Par Défaut | Description |
|-----------|------|------------|-------------|
| **Enabled** | `bool` | `false` | Active/désactive l'envoi d'emails |
| **SmtpHost** | `string` | `localhost` | Serveur SMTP |
| **SmtpPort** | `int` | `1025` | Port SMTP (25, 587, 465, 1025) |
| **SmtpUsername** | `string` | `""` | Username SMTP (si auth requise) |
| **SmtpPassword** | `string` | `""` | Mot de passe SMTP |
| **FromEmail** | `string` | `no-reply@nawel.com` | Email expéditeur |
| **FromName** | `string` | `Nawel - Listes de Noël` | Nom expéditeur |
| **UseSsl** | `bool` | `false` | Utiliser SSL/TLS |
| **NotificationDelayMinutes** | `int` | `2` | Délai pour regrouper les notifications de liste modifiée |
| **ReservationNotificationDelayMinutes** | `int` | `2` | Délai pour regrouper les notifications de réservation |

### Configuration par Fournisseur

#### Gmail

**appsettings.json** :
```json
{
  "Email": {
    "Enabled": true,
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": 587,
    "SmtpUsername": "your-email@gmail.com",
    "SmtpPassword": "your_app_password",
    "FromEmail": "your-email@gmail.com",
    "FromName": "Nawel - Listes de Noël",
    "UseSsl": true,
    "NotificationDelayMinutes": 2,
    "ReservationNotificationDelayMinutes": 2
  }
}
```

**Variables d'environnement** (recommandé) :
```bash
EMAIL_SMTP_SERVER=smtp.gmail.com
EMAIL_SMTP_PORT=587
EMAIL_USERNAME=your-email@gmail.com
EMAIL_PASSWORD=your_gmail_app_password
EMAIL_FROM=your-email@gmail.com
EMAIL_FROM_NAME="Nawel - Listes de Noël"
EMAIL_USE_SSL=true
```

**⚠️ Important Gmail** :
- Utilisez un **mot de passe d'application** (App Password), pas votre mot de passe Gmail
- Activez la double authentification sur votre compte Google
- Générez le mot de passe d'application : https://myaccount.google.com/apppasswords

#### Office 365 / Outlook.com

```json
{
  "Email": {
    "Enabled": true,
    "SmtpHost": "smtp-mail.outlook.com",
    "SmtpPort": 587,
    "SmtpUsername": "your-email@outlook.com",
    "SmtpPassword": "your_password",
    "FromEmail": "your-email@outlook.com",
    "FromName": "Nawel - Listes de Noël",
    "UseSsl": true
  }
}
```

#### SendGrid

```json
{
  "Email": {
    "Enabled": true,
    "SmtpHost": "smtp.sendgrid.net",
    "SmtpPort": 587,
    "SmtpUsername": "apikey",
    "SmtpPassword": "your_sendgrid_api_key",
    "FromEmail": "no-reply@yourdomain.com",
    "FromName": "Nawel - Listes de Noël",
    "UseSsl": true
  }
}
```

#### Mailgun

```json
{
  "Email": {
    "Enabled": true,
    "SmtpHost": "smtp.mailgun.org",
    "SmtpPort": 587,
    "SmtpUsername": "postmaster@yourdomain.mailgun.org",
    "SmtpPassword": "your_mailgun_password",
    "FromEmail": "no-reply@yourdomain.com",
    "FromName": "Nawel - Listes de Noël",
    "UseSsl": true
  }
}
```

#### MailHog (Dev/Test Local)

```json
{
  "Email": {
    "Enabled": true,
    "SmtpHost": "localhost",
    "SmtpPort": 1025,
    "SmtpUsername": "",
    "SmtpPassword": "",
    "FromEmail": "no-reply@nawel.com",
    "FromName": "Nawel - Listes de Noël",
    "UseSsl": false
  }
}
```

Lancer MailHog :
```bash
docker run -d -p 1025:1025 -p 8025:8025 mailhog/mailhog
```
Interface web : http://localhost:8025

### Validation Automatique

La classe `EmailSettings` valide automatiquement si `Enabled: true` :
- ✅ SmtpHost non vide
- ✅ SmtpPort entre 1 et 65535
- ✅ FromEmail non vide

Si `Enabled: false`, aucune validation (mode désactivé).

### Délai de Regroupement (Debouncing)

**Fonctionnement** :
- Lorsqu'un cadeau est réservé, un timer de 2 minutes démarre
- Si d'autres cadeaux sont réservés pendant ces 2 minutes, le timer est réinitialisé
- L'email n'est envoyé que 2 minutes après la dernière réservation
- Permet de regrouper plusieurs réservations en un seul email

**Configuration** :
```json
{
  "Email": {
    "ReservationNotificationDelayMinutes": 2
  }
}
```

**Exemple Timeline** :
```
T+0s  : Marie réserve "Livre A" → Timer démarre (2 min)
T+30s : Pierre réserve "Livre B" → Timer reset (2 min)
T+1m  : Jean réserve "Jeu C" → Timer reset (2 min)
T+3m  : Aucune autre réservation → Email envoyé

Email : "3 cadeaux de votre liste ont été réservés : Livre A, Livre B, Jeu C"
```

---

## Configuration Base de Données

### Choix MySQL vs SQLite

**SQLite** :
- ✅ Développement local rapide
- ✅ Aucune installation requise
- ✅ Fichier unique `nawel.db`
- ❌ Pas de concurrence (mono-utilisateur)
- ❌ Performance limitée

**MySQL** :
- ✅ Production
- ✅ Concurrence multi-utilisateurs
- ✅ Performance optimale
- ✅ Transactions robustes
- ❌ Installation requise

### Configuration SQLite (Développement)

**appsettings.json** :
```json
{
  "ConnectionStrings": {
    "SqliteConnection": "Data Source=nawel.db"
  },
  "UseSqlite": true
}
```

**Activation** :
```csharp
if (useSqlite)
{
    builder.Services.AddDbContext<NawelDbContext>(options =>
        options.UseSqlite(builder.Configuration.GetConnectionString("SqliteConnection")));

    Console.WriteLine("Using SQLite database for development");
}
```

**Fichier généré** : `nawel.db` à la racine du projet.

**Commandes** :
```bash
# Lister les tables
sqlite3 nawel.db ".tables"

# Voir le schéma d'une table
sqlite3 nawel.db ".schema users"

# Exporter en SQL
sqlite3 nawel.db ".dump" > backup.sql
```

### Configuration MySQL (Production)

**appsettings.json** :
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=nawel;User=nawel_user;Password=nawel_pass;"
  },
  "UseSqlite": false
}
```

**Variables d'environnement** (recommandé) :
```bash
ConnectionStrings__DefaultConnection="Server=mysql;Port=3306;Database=nawel_prod;User=nawel_user;Password=***;"
UseSqlite=false
```

**Format Connection String** :
```
Server=<host>;Port=<port>;Database=<db_name>;User=<username>;Password=<password>;[options]
```

**Options disponibles** :
```
Server=mysql;
Port=3306;
Database=nawel_prod;
User=nawel_user;
Password=secure_password_here;
SslMode=Required;              # Forcer SSL
AllowUserVariables=true;       # Si besoin de variables SQL
CharSet=utf8mb4;               # Charset UTF-8 complet
ConnectionTimeout=30;          # Timeout connexion (secondes)
```

### Docker MySQL

**docker-compose.yml** :
```yaml
services:
  mysql:
    image: mysql:8.0
    environment:
      MYSQL_ROOT_PASSWORD: root_password
      MYSQL_DATABASE: nawel_prod
      MYSQL_USER: nawel_user
      MYSQL_PASSWORD: nawel_pass
    ports:
      - "3306:3306"
    volumes:
      - mysql_data:/var/lib/mysql
    restart: unless-stopped

volumes:
  mysql_data:
```

**Connection string** :
```
Server=mysql;Port=3306;Database=nawel_prod;User=nawel_user;Password=nawel_pass;
```

### Migrations

**Entity Framework Core Migrations** (si utilisé) :
```bash
# Créer une migration
dotnet ef migrations add InitialCreate

# Appliquer les migrations
dotnet ef database update

# Revenir en arrière
dotnet ef database update PreviousMigrationName

# Générer un script SQL
dotnet ef migrations script > migration.sql
```

**Scripts SQL Manuels** :
```bash
# Appliquer un script
mysql -u root -p nawel_prod < database/migrations/001_initial_schema.sql
```

### Seed Data (Développement)

**Automatique avec SQLite** :
```csharp
if (useSqlite && app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<NawelDbContext>();
    dbContext.Database.EnsureCreated();

    // Seed test data
    DbSeeder.SeedTestData(dbContext);
}
```

**Données de test créées** :
- Utilisateurs de test (admin, users normaux, enfants)
- Familles de test
- Cadeaux de test

---

## Configuration Rate Limiting

### Principe

Le rate limiting protège l'API contre :
- 🚫 Attaques par force brute (login)
- 🚫 Spam de requêtes
- 🚫 Abus de ressources
- 🚫 Déni de service (DoS)

**Librairie utilisée** : `AspNetCoreRateLimit` (IP-based)

### Configuration Globale

**appsettings.json** :
```json
{
  "IpRateLimiting": {
    "EnableEndpointRateLimiting": true,
    "StackBlockedRequests": false,
    "RealIpHeader": "X-Real-IP",
    "ClientIdHeader": "X-ClientId",
    "HttpStatusCode": 429,
    "GeneralRules": [
      {
        "Endpoint": "*",
        "Period": "1m",
        "Limit": 60
      },
      {
        "Endpoint": "*",
        "Period": "1h",
        "Limit": 1000
      }
    ],
    "QuotaExceededResponse": {
      "Content": "{{ \"message\": \"Rate limit exceeded. Please try again later.\" }}",
      "ContentType": "application/json"
    }
  }
}
```

**Paramètres** :

| Paramètre | Type | Description |
|-----------|------|-------------|
| `EnableEndpointRateLimiting` | `bool` | Active le rate limiting par endpoint |
| `StackBlockedRequests` | `bool` | Compte les requêtes bloquées (false = recommandé) |
| `RealIpHeader` | `string` | Header contenant la vraie IP (derrière proxy) |
| `ClientIdHeader` | `string` | Header contenant un ID client (optionnel) |
| `HttpStatusCode` | `int` | Code HTTP retourné (429 = Too Many Requests) |
| `GeneralRules` | `array` | Règles globales |
| `QuotaExceededResponse` | `object` | Réponse JSON personnalisée |

### Configuration par Endpoint

**appsettings.RateLimiting.json** :
```json
{
  "IpRateLimiting": {
    "GeneralRules": [
      {
        "Endpoint": "*:/api/auth/login",
        "Period": "1m",
        "Limit": 5
      },
      {
        "Endpoint": "*:/api/auth/login",
        "Period": "15m",
        "Limit": 10
      },
      {
        "Endpoint": "*:/api/auth/forgot-password",
        "Period": "1h",
        "Limit": 3
      },
      {
        "Endpoint": "*:/api/auth/reset-password",
        "Period": "1h",
        "Limit": 5
      },
      {
        "Endpoint": "*:/api/auth/*",
        "Period": "1m",
        "Limit": 20
      }
    ]
  }
}
```

**Syntaxe Endpoint** :
```
<method>:/<path>
```

**Exemples** :
- `*:/api/auth/login` : Toutes méthodes sur `/api/auth/login`
- `POST:/api/auth/login` : Seulement POST
- `GET:/api/gifts` : Seulement GET
- `*:/api/auth/*` : Tous les endpoints sous `/api/auth/`
- `*` : Tous les endpoints (règle globale)

**Syntaxe Period** :
- `1s` : 1 seconde
- `1m` : 1 minute
- `15m` : 15 minutes
- `1h` : 1 heure
- `1d` : 1 jour

### IP Whitelisting

**Exemples d'IPs whitelisted** :
```json
{
  "IpRateLimitPolicies": {
    "IpRules": [
      {
        "Ip": "127.0.0.1",
        "Rules": [
          {
            "Endpoint": "*",
            "Period": "1s",
            "Limit": 1000
          }
        ]
      },
      {
        "Ip": "192.168.1.100",
        "Rules": [
          {
            "Endpoint": "*",
            "Period": "1s",
            "Limit": 100
          }
        ]
      }
    ]
  }
}
```

**Whitelisting complet** (désactive rate limiting pour une IP) :
```json
{
  "IpRateLimiting": {
    "ClientWhitelist": ["127.0.0.1", "::1"]
  }
}
```

### Configuration Recommandée

**Développement** :
```json
{
  "IpRateLimiting": {
    "EnableEndpointRateLimiting": false
  }
}
```

**Production** :
```json
{
  "IpRateLimiting": {
    "EnableEndpointRateLimiting": true,
    "GeneralRules": [
      { "Endpoint": "*:/api/auth/login", "Period": "1m", "Limit": 5 },
      { "Endpoint": "*:/api/auth/login", "Period": "1h", "Limit": 20 },
      { "Endpoint": "*", "Period": "1m", "Limit": 100 },
      { "Endpoint": "*", "Period": "1h", "Limit": 2000 }
    ]
  }
}
```

### Réponse Bloquée

**HTTP 429 Too Many Requests** :
```json
{
  "message": "Rate limit exceeded. Please try again later."
}
```

**Headers de réponse** :
```
X-Rate-Limit-Limit: 5
X-Rate-Limit-Remaining: 0
X-Rate-Limit-Reset: 2024-12-09T14:32:00Z
Retry-After: 58
```

---

## Configuration CORS

### Principe

CORS (Cross-Origin Resource Sharing) permet au frontend (domaine différent) d'accéder à l'API.

**Exemple** :
- Frontend : `http://localhost:5173` (Vite dev server)
- Backend : `http://localhost:5000` (API)
- CORS nécessaire car domaines différents

### Configuration

**Program.cs** :
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy.WithOrigins(
                    "http://localhost:3000",   // React (CRA)
                    "http://localhost:5173",   // Vite
                    "http://localhost:5174",   // Vite (port alternatif)
                    "http://localhost:5175"    // Vite (port alternatif)
                )
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
});

// ...

app.UseCors("AllowFrontend");
```

**Ordre important** :
```csharp
app.UseRouting();
app.UseCors("AllowFrontend");  // APRÈS UseRouting
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
```

### Configuration Production

**Domaine Production** :
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            var allowedOrigins = builder.Configuration
                .GetSection("AllowedOrigins")
                .Get<string[]>() ?? Array.Empty<string>();

            policy.WithOrigins(allowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
});
```

**appsettings.Production.json** :
```json
{
  "AllowedOrigins": [
    "https://nawel.example.com",
    "https://www.nawel.example.com"
  ]
}
```

**Variables d'environnement** :
```bash
AllowedOrigins__0=https://nawel.example.com
AllowedOrigins__1=https://www.nawel.example.com
```

### CORS et Credentials

**AllowCredentials** nécessaire pour :
- Cookies
- Authorization header
- Client certificates

**⚠️ Important** : Ne peut pas être combiné avec `AllowAnyOrigin()`.

**Correct** :
```csharp
policy.WithOrigins("http://localhost:5173")
      .AllowCredentials();
```

**Incorrect** :
```csharp
policy.AllowAnyOrigin()  // ❌ Ne fonctionne pas avec AllowCredentials
      .AllowCredentials();
```

---

## Configuration File Storage

### Paramètres

**appsettings.json** :
```json
{
  "FileStorage": {
    "AvatarsPath": "uploads/avatars",
    "MaxFileSizeMB": 5,
    "AllowedExtensions": [".jpg", ".jpeg", ".png", ".gif", ".webp"]
  }
}
```

| Paramètre | Type | Description |
|-----------|------|-------------|
| `AvatarsPath` | `string` | Chemin relatif pour stocker les avatars |
| `MaxFileSizeMB` | `int` | Taille max d'upload (MB) |
| `AllowedExtensions` | `string[]` | Extensions de fichiers autorisées |

### Storage Local (Développement)

**Création automatique du dossier** :
```csharp
var uploadsPath = Path.Combine(app.Environment.ContentRootPath, "uploads");
Directory.CreateDirectory(uploadsPath);
```

**Servir les fichiers statiques** :
```csharp
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads"
});
```

**URL d'accès** :
```
http://localhost:5000/uploads/avatars/user_2_abc123.jpg
```

### Validation Upload

**Côté backend** :
```csharp
// Taille max
if (file.Length > _fileStorageSettings.MaxFileSizeMB * 1024 * 1024)
    throw new BadRequestException($"File size exceeds {_fileStorageSettings.MaxFileSizeMB}MB");

// Extension
var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
if (!_fileStorageSettings.AllowedExtensions.Contains(extension))
    throw new BadRequestException($"File type {extension} not allowed");
```

### Storage Cloud (Production)

**Options** :
- AWS S3
- Azure Blob Storage
- Google Cloud Storage
- Cloudinary

**Exemple avec Azure Blob** (à implémenter) :
```csharp
// Remplacer le storage local par Azure Blob
builder.Services.AddSingleton<IFileStorage, AzureBlobStorageService>();
```

---

## Configuration OpenGraph API

### Principe

L'API OpenGraph.io permet d'extraire les métadonnées d'une page web (titre, description, prix, image) pour remplir automatiquement les champs d'un cadeau.

**Site** : https://www.opengraph.io/

### Paramètres

**appsettings.json** :
```json
{
  "OpenGraph": {
    "Enabled": true,
    "ApiKey": "YOUR_OPENGRAPH_API_KEY_HERE",
    "Comment": "Get your free API key at https://www.opengraph.io/ (1000 requests/month free)"
  }
}
```

| Paramètre | Type | Description |
|-----------|------|-------------|
| `Enabled` | `bool` | Active/désactive l'extraction automatique |
| `ApiKey` | `string` | Clé API OpenGraph.io |

### Obtenir une Clé API

1. Créer un compte sur https://www.opengraph.io/
2. Plan gratuit : 1000 requêtes/mois
3. Copier la clé API
4. Configurer via variable d'environnement :

```bash
OPENGRAPH_API_KEY=your_api_key_here
```

### Utilisation

**Endpoint** : `POST /api/products/extract`

**Request** :
```json
{
  "url": "https://www.amazon.fr/dp/B08H93ZRK9"
}
```

**Response** :
```json
{
  "name": "PlayStation 5",
  "description": "Vivez une nouvelle génération de jeux PlayStation...",
  "cost": 499.99,
  "currency": "EUR",
  "image": "https://m.media-amazon.com/images/I/51vvdyr3WNL._AC_SL1500_.jpg"
}
```

### Désactivation

**Si pas de clé API ou désactivé** :
```json
{
  "OpenGraph": {
    "Enabled": false
  }
}
```

**Réponse API** :
```json
{
  "error": "Product info extraction is not enabled"
}
```

---

## Sécurité et Best Practices

### 1. Secrets et Mots de Passe

**❌ JAMAIS** :
- Commiter `appsettings.Production.json` avec secrets
- Commiter `.env` avec secrets
- Mettre des secrets en dur dans le code
- Partager JWT_SECRET publiquement

**✅ TOUJOURS** :
- Utiliser des variables d'environnement pour secrets
- Générer des secrets robustes (≥32 caractères aléatoires)
- Rotation régulière des secrets (tous les 3-6 mois)
- Utiliser un gestionnaire de secrets en production (Azure Key Vault, AWS Secrets Manager, etc.)

### 2. JWT Secret

**Générer un secret robuste** :
```bash
openssl rand -base64 48
```

**Longueur minimum** : 32 caractères (256 bits)

**Recommandation** : 48-64 caractères

### 3. Connection Strings

**❌ Mauvais** :
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=prod-db;User=admin;Password=admin123;"
  }
}
```

**✅ Bon** :
```bash
# Variable d'environnement
ConnectionStrings__DefaultConnection="Server=prod-db;User=nawel_app;Password=<strong_password>;"
```

### 4. HTTPS Obligatoire en Production

**Program.cs** :
```csharp
if (app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
    app.UseHsts();
}
```

**Reverse Proxy (Nginx)** :
```nginx
server {
    listen 443 ssl http2;
    ssl_certificate /etc/letsencrypt/live/nawel.example.com/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/nawel.example.com/privkey.pem;

    location / {
        proxy_pass http://localhost:5000;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

### 5. Rate Limiting Actif

**Production** : Toujours actif avec limites strictes

```json
{
  "IpRateLimiting": {
    "EnableEndpointRateLimiting": true,
    "GeneralRules": [
      { "Endpoint": "*:/api/auth/login", "Period": "1m", "Limit": 5 }
    ]
  }
}
```

### 6. CORS Restreint

**❌ Mauvais** :
```csharp
policy.AllowAnyOrigin();  // Trop permissif
```

**✅ Bon** :
```csharp
policy.WithOrigins("https://nawel.example.com");
```

### 7. Logs et Monitoring

**Logging Level Production** :
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning",
      "Nawel.Api": "Information"
    }
  }
}
```

**Ne jamais logger** :
- Mots de passe
- Tokens JWT complets
- Données sensibles (emails, noms complets)

---

## Configuration par Environnement

### Fichiers par Environnement

**Structure** :
```
appsettings.json                   # Base (commité)
appsettings.Development.json       # Dev (optionnel, commité)
appsettings.Production.json        # Prod (NON COMMITÉ)
appsettings.Testing.json           # Tests (commité)
```

### Development

**appsettings.Development.json** :
```json
{
  "UseSqlite": true,
  "Email": {
    "Enabled": false
  },
  "IpRateLimiting": {
    "EnableEndpointRateLimiting": false
  },
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Information"
    }
  }
}
```

**Lancement** :
```bash
export ASPNETCORE_ENVIRONMENT=Development
dotnet run
```

### Production

**appsettings.Production.json** (ne PAS commiter) :
```json
{
  "UseSqlite": false,
  "Email": {
    "Enabled": true
  },
  "IpRateLimiting": {
    "EnableEndpointRateLimiting": true
  },
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedOrigins": [
    "https://nawel.example.com"
  ]
}
```

**Variables d'environnement obligatoires** :
```bash
ASPNETCORE_ENVIRONMENT=Production
JWT_SECRET=<generated_secret>
ConnectionStrings__DefaultConnection=<mysql_connection>
EMAIL_SMTP_SERVER=smtp.gmail.com
EMAIL_SMTP_PORT=587
EMAIL_USERNAME=<email>
EMAIL_PASSWORD=<app_password>
OPENGRAPH_API_KEY=<key>
```

### Testing

**appsettings.Testing.json** :
```json
{
  "UseSqlite": true,
  "ConnectionStrings": {
    "SqliteConnection": "Data Source=:memory:"
  },
  "Email": {
    "Enabled": false
  },
  "IpRateLimiting": {
    "EnableEndpointRateLimiting": false
  }
}
```

---

## Docker et Déploiement

### docker-compose.yml (Production)

```yaml
version: '3.8'

services:
  mysql:
    image: mysql:8.0
    environment:
      MYSQL_ROOT_PASSWORD: ${MYSQL_ROOT_PASSWORD}
      MYSQL_DATABASE: nawel_prod
      MYSQL_USER: nawel_user
      MYSQL_PASSWORD: ${MYSQL_PASSWORD}
    volumes:
      - mysql_data:/var/lib/mysql
    restart: unless-stopped
    networks:
      - nawel-network

  backend:
    build:
      context: ./backend/Nawel.Api
      dockerfile: Dockerfile
    environment:
      ASPNETCORE_ENVIRONMENT: Production
      JWT_SECRET: ${JWT_SECRET}
      ConnectionStrings__DefaultConnection: "Server=mysql;Port=3306;Database=nawel_prod;User=nawel_user;Password=${MYSQL_PASSWORD};"
      UseSqlite: "false"
      Email__Enabled: "true"
      Email__SmtpHost: ${EMAIL_SMTP_SERVER}
      Email__SmtpPort: ${EMAIL_SMTP_PORT}
      Email__SmtpUsername: ${EMAIL_USERNAME}
      Email__SmtpPassword: ${EMAIL_PASSWORD}
      Email__UseSsl: "true"
      OpenGraph__ApiKey: ${OPENGRAPH_API_KEY}
    ports:
      - "5000:8080"
    depends_on:
      - mysql
    restart: unless-stopped
    networks:
      - nawel-network

  frontend:
    build:
      context: ./frontend/nawel-app
      dockerfile: Dockerfile
    ports:
      - "3000:80"
    depends_on:
      - backend
    restart: unless-stopped
    networks:
      - nawel-network

volumes:
  mysql_data:

networks:
  nawel-network:
```

### Fichier .env (Docker)

**Ne PAS commiter** ce fichier.

```.env
# MySQL
MYSQL_ROOT_PASSWORD=your_mysql_root_password
MYSQL_PASSWORD=your_nawel_user_password

# JWT
JWT_SECRET=your_super_secure_jwt_secret_at_least_32_characters_long

# Email
EMAIL_SMTP_SERVER=smtp.gmail.com
EMAIL_SMTP_PORT=587
EMAIL_USERNAME=your-email@gmail.com
EMAIL_PASSWORD=your_gmail_app_password

# OpenGraph
OPENGRAPH_API_KEY=your_opengraph_api_key
```

### Commandes Docker

**Démarrer** :
```bash
docker-compose up -d
```

**Voir les logs** :
```bash
docker-compose logs -f backend
```

**Redémarrer après changement de config** :
```bash
docker-compose restart backend
```

**Rebuild après changement de code** :
```bash
docker-compose up -d --build backend
```

**Arrêter** :
```bash
docker-compose down
```

**Supprimer volumes (réinitialiser BDD)** :
```bash
docker-compose down -v
```

---

## Dépannage

### Erreur: JWT Secret trop court

**Message** :
```
InvalidOperationException: JWT Secret must be at least 32 characters long for security
```

**Solution** :
```bash
# Générer un secret robuste
openssl rand -base64 48

# Définir la variable d'environnement
export JWT_SECRET="<generated_secret>"
```

### Erreur: Email SMTP Connection Failed

**Message** :
```
MailKit.Security.AuthenticationException: 535 Authentication failed
```

**Solutions** :
1. **Gmail** : Utilisez un mot de passe d'application (App Password)
2. Vérifiez `SmtpHost`, `SmtpPort`, `SmtpUsername`, `SmtpPassword`
3. Vérifiez `UseSsl: true` pour Gmail/Outlook
4. Testez avec MailHog en dev : `SmtpHost: localhost`, `SmtpPort: 1025`, `UseSsl: false`

### Erreur: MySQL Connection Failed

**Message** :
```
MySqlConnector.MySqlException: Unable to connect to any of the specified MySQL hosts
```

**Solutions** :
1. Vérifiez que MySQL est démarré :
   ```bash
   docker-compose ps
   ```
2. Vérifiez la connection string :
   ```bash
   Server=mysql;Port=3306;Database=nawel_prod;User=nawel_user;Password=***;
   ```
3. Vérifiez les credentials MySQL dans `docker-compose.yml`
4. Attendez que MySQL soit prêt (10-20 secondes au démarrage)

### Erreur: CORS Policy

**Message** :
```
Access to XMLHttpRequest at 'http://localhost:5000/api/auth/login' from origin 'http://localhost:5173' has been blocked by CORS policy
```

**Solutions** :
1. Vérifiez que l'origine est dans `AllowedOrigins` :
   ```csharp
   policy.WithOrigins("http://localhost:5173")
   ```
2. Vérifiez l'ordre des middleware :
   ```csharp
   app.UseCors("AllowFrontend");  // AVANT UseAuthentication
   ```
3. Vérifiez `AllowCredentials()` si vous utilisez Authorization header

### Erreur: Rate Limit Exceeded

**Message** :
```json
{
  "message": "Rate limit exceeded. Please try again later."
}
```

**Solutions** :
1. **Développement** : Désactivez rate limiting :
   ```json
   {
     "IpRateLimiting": {
       "EnableEndpointRateLimiting": false
     }
   }
   ```
2. **Production** : Attendez le délai indiqué dans `Retry-After` header
3. Whitelistez votre IP locale :
   ```json
   {
     "IpRateLimitPolicies": {
       "IpRules": [
         { "Ip": "127.0.0.1", "Rules": [{ "Endpoint": "*", "Period": "1s", "Limit": 1000 }] }
       ]
     }
   }
   ```

### Erreur: OpenGraph API Rate Limit

**Message** :
```json
{
  "error": "OpenGraph API rate limit exceeded"
}
```

**Solutions** :
1. Plan gratuit : 1000 requêtes/mois
2. Upgrade vers un plan payant sur https://www.opengraph.io/pricing
3. Désactivez temporairement :
   ```json
   {
     "OpenGraph": {
       "Enabled": false
     }
   }
   ```

### Erreur: File Upload Failed (413 Request Entity Too Large)

**Message** :
```
413 Payload Too Large
```

**Solutions** :
1. Augmentez `MaxFileSizeMB` :
   ```json
   {
     "FileStorage": {
       "MaxFileSizeMB": 10
     }
   }
   ```
2. Configurez Nginx (si reverse proxy) :
   ```nginx
   client_max_body_size 10M;
   ```

### Logs Détaillés

**Activer logs Debug** :
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Debug",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  }
}
```

**Console** :
```bash
dotnet run --verbose
```

---

## Récapitulatif Configuration

### Checklist Démarrage Rapide

**Développement Local** :
- [ ] Installer .NET 9 SDK
- [ ] Copier `appsettings.json` par défaut
- [ ] Définir `UseSqlite: true`
- [ ] Définir `Email.Enabled: false` (ou utiliser MailHog)
- [ ] Générer JWT_SECRET :
  ```bash
  export JWT_SECRET="dev_secret_key_minimum_32_characters_required_for_security"
  ```
- [ ] Lancer : `dotnet run`
- [ ] Vérifier : http://localhost:5000/swagger

**Production Docker** :
- [ ] Créer `.env` avec tous les secrets
- [ ] Vérifier `UseSqlite: false`
- [ ] Vérifier `Email.Enabled: true` avec credentials SMTP
- [ ] Générer JWT_SECRET robuste (48+ chars)
- [ ] Configurer CORS avec domaine production
- [ ] Activer Rate Limiting
- [ ] Lancer : `docker-compose up -d`
- [ ] Vérifier logs : `docker-compose logs -f backend`

### Variables Essentielles Production

```bash
# Obligatoires
JWT_SECRET=<48+ chars>
ConnectionStrings__DefaultConnection=<mysql_connection>
EMAIL_SMTP_SERVER=<smtp_host>
EMAIL_SMTP_PORT=587
EMAIL_USERNAME=<email>
EMAIL_PASSWORD=<app_password>

# Recommandées
OPENGRAPH_API_KEY=<key>
AllowedOrigins__0=https://nawel.example.com

# Optionnelles
JWT_ISSUER=NawelApi
JWT_AUDIENCE=NawelApp
Email__FromEmail=no-reply@nawel.example.com
Email__FromName="Nawel - Listes de Noël"
```

---

## Références

- [ASP.NET Core Configuration](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/)
- [Environment Variables in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/#environment-variables)
- [JWT Best Practices](https://tools.ietf.org/html/rfc8725)
- [AspNetCoreRateLimit Documentation](https://github.com/stefanprodan/AspNetCoreRateLimit)
- [CORS in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/security/cors)
- [OpenGraph.io Documentation](https://www.opengraph.io/documentation/)

---

**Version** : 2.0.0
**Dernière mise à jour** : Décembre 2024
