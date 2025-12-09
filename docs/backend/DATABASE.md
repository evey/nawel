# Documentation Base de Données - Nawel

## Vue d'Ensemble

L'application Nawel utilise une base de données relationnelle pour stocker les utilisateurs, familles, listes de cadeaux, et réservations. Le système supporte deux providers :

- **MySQL 8.0** : Production
- **SQLite** : Développement local

## Choix du Provider

Le provider est configuré via la variable `UseSqlite` dans `appsettings.json` :

```json
{
  "UseSqlite": true,  // SQLite pour dev
  "ConnectionStrings": {
    "SqliteConnection": "Data Source=nawel.db",
    "DefaultConnection": "server=localhost;database=nawel_db;user=root;password=***"
  }
}
```

**Configuration dans Program.cs** :
```csharp
if (useSqlite)
{
    builder.Services.AddDbContext<NawelDbContext>(options =>
        options.UseSqlite(builder.Configuration.GetConnectionString("SqliteConnection")));
}
else
{
    var serverVersion = new MySqlServerVersion(new Version(8, 0, 0));
    builder.Services.AddDbContext<NawelDbContext>(options =>
        options.UseMySql(
            builder.Configuration.GetConnectionString("DefaultConnection"),
            serverVersion));
}
```

## Schéma de Base

La base de données contient **6 tables principales** et **1 table de tracking** :

| Table | Description | Lignes (estimation) |
|-------|-------------|---------------------|
| `families` | Groupes familiaux | 10-50 |
| `users` | Utilisateurs de l'application | 50-200 |
| `gift_lists` | Listes de cadeaux (1 par user) | 50-200 |
| `gifts` | Cadeaux individuels | 500-5000 |
| `gift_participations` | Participations cadeaux groupés | 100-1000 |
| `password_reset_tokens` | Tokens de réinitialisation | 0-50 (éphémère) |
| `opengraph_requests` | Tracking API externe | 1000+ |

## Tables Détaillées

### Table : `families`

Représente un groupe familial.

| Colonne | Type | Nullable | Default | Description |
|---------|------|----------|---------|-------------|
| `Id` | int | No | AUTO_INCREMENT | Identifiant unique |
| `Name` | nvarchar(100) | No | - | Nom de la famille (unique) |
| `CreatedAt` | datetime | No | CURRENT_TIMESTAMP | Date de création |
| `UpdatedAt` | datetime | No | CURRENT_TIMESTAMP | Date de mise à jour |

**Contraintes** :
- **PRIMARY KEY** : `Id`
- **UNIQUE** : `Name`

**Indexes** :
- Index unique sur `Name` (recherche rapide)

**Relations** :
- **1 → N** : Une famille a plusieurs utilisateurs

**Exemple** :
```json
{
  "id": 1,
  "name": "Famille Martin",
  "createdAt": "2025-01-01T00:00:00Z",
  "updatedAt": "2025-01-01T00:00:00Z"
}
```

---

### Table : `users`

Représente un utilisateur de l'application (adulte ou enfant).

| Colonne | Type | Nullable | Default | Description |
|---------|------|----------|---------|-------------|
| `Id` | int | No | AUTO_INCREMENT | Identifiant unique |
| `Login` | nvarchar(50) | No | - | Nom de connexion (unique) |
| `Password` | nvarchar(255) | No | - | Hash BCrypt (ou MD5 legacy) |
| `Email` | nvarchar(255) | Yes | NULL | Adresse email |
| `FirstName` | nvarchar(100) | Yes | NULL | Prénom |
| `LastName` | nvarchar(100) | Yes | NULL | Nom de famille |
| `Avatar` | nvarchar(500) | Yes | 'avatar.png' | Chemin relatif de l'avatar |
| `Pseudo` | nvarchar(100) | Yes | NULL | Surnom affiché |
| `IsChildren` | boolean | No | FALSE | Compte enfant (géré par parent) |
| `IsAdmin` | boolean | No | FALSE | Rôle administrateur |
| `FamilyId` | int | No | - | Référence à `families.Id` |
| `NotifyListEdit` | boolean | No | TRUE | Notif si liste modifiée |
| `NotifyGiftTaken` | boolean | No | TRUE | Notif si cadeau réservé |
| `DisplayPopup` | boolean | No | TRUE | Afficher popups d'info |
| `CreatedAt` | datetime | No | CURRENT_TIMESTAMP | Date de création |
| `UpdatedAt` | datetime | No | CURRENT_TIMESTAMP | Date de mise à jour |

**Contraintes** :
- **PRIMARY KEY** : `Id`
- **UNIQUE** : `Login`
- **FOREIGN KEY** : `FamilyId` → `families.Id` (ON DELETE RESTRICT)

**Indexes** :
- Index unique sur `Login` (authentification rapide)
- Index sur `FamilyId` (requêtes familiales)

**Relations** :
- **N → 1** : Plusieurs utilisateurs appartiennent à une famille
- **1 → 1** : Un utilisateur a une liste de cadeaux
- **1 → N** : Un utilisateur peut avoir réservé plusieurs cadeaux

**Particularités** :
- **Password** : BCrypt hash (60 chars) ou MD5 legacy (32 chars)
- **IsChildren** : Si TRUE, un parent peut gérer cette liste
- **Avatar** : Chemin relatif type `uploads/avatars/user_42_guid.jpg`

**Exemple** :
```json
{
  "id": 2,
  "login": "sylvain",
  "password": "$2a$11$...",
  "email": "sylvain@example.com",
  "firstName": "Sylvain",
  "lastName": "Nironi",
  "avatar": "uploads/avatars/user_2_abc123.jpg",
  "pseudo": null,
  "isChildren": false,
  "isAdmin": true,
  "familyId": 1,
  "notifyListEdit": true,
  "notifyGiftTaken": true,
  "displayPopup": true,
  "createdAt": "2025-01-01T00:00:00Z",
  "updatedAt": "2025-01-05T10:30:00Z"
}
```

---

### Table : `gift_lists`

Représente une liste de cadeaux (1 par utilisateur).

| Colonne | Type | Nullable | Default | Description |
|---------|------|----------|---------|-------------|
| `Id` | int | No | AUTO_INCREMENT | Identifiant unique |
| `Name` | nvarchar(200) | No | - | Nom de la liste |
| `UserId` | int | No | - | Propriétaire de la liste |
| `CreatedAt` | datetime | No | CURRENT_TIMESTAMP | Date de création |
| `UpdatedAt` | datetime | No | CURRENT_TIMESTAMP | Date de mise à jour |

**Contraintes** :
- **PRIMARY KEY** : `Id`
- **FOREIGN KEY** : `UserId` → `users.Id` (ON DELETE CASCADE)
- **UNIQUE** : `UserId` (une seule liste par user)

**Indexes** :
- Index unique sur `UserId`

**Relations** :
- **1 → 1** : Une liste appartient à un seul utilisateur
- **1 → N** : Une liste contient plusieurs cadeaux

**Exemple** :
```json
{
  "id": 10,
  "name": "Liste de Sylvain",
  "userId": 2,
  "createdAt": "2025-01-01T00:00:00Z",
  "updatedAt": "2025-01-01T00:00:00Z"
}
```

---

### Table : `gifts`

Représente un cadeau dans une liste.

| Colonne | Type | Nullable | Default | Description |
|---------|------|----------|---------|-------------|
| `Id` | int | No | AUTO_INCREMENT | Identifiant unique |
| `Name` | nvarchar(200) | No | - | Nom du cadeau |
| `Description` | nvarchar(1000) | Yes | NULL | Description détaillée |
| `Link` | nvarchar(2000) | Yes | NULL | URL du produit |
| `Cost` | decimal(10,2) | Yes | NULL | Prix estimé |
| `Currency` | nvarchar(10) | Yes | 'EUR' | Devise (EUR, USD, etc.) |
| `Image` | nvarchar(2000) | Yes | NULL | URL de l'image |
| `Year` | int | No | - | Année du cadeau |
| `ListId` | int | No | - | Référence à `gift_lists.Id` |
| `TakenBy` | int | Yes | NULL | Réservé par (user ID) |
| `Available` | boolean | No | TRUE | Disponible (pas réservé) |
| `Comment` | nvarchar(500) | Yes | NULL | Commentaire du réserveur |
| `IsGroupGift` | boolean | No | FALSE | Cadeau groupé |
| `CreatedAt` | datetime | No | CURRENT_TIMESTAMP | Date de création |
| `UpdatedAt` | datetime | No | CURRENT_TIMESTAMP | Date de mise à jour |

**Contraintes** :
- **PRIMARY KEY** : `Id`
- **FOREIGN KEY** : `ListId` → `gift_lists.Id` (ON DELETE CASCADE)
- **FOREIGN KEY** : `TakenBy` → `users.Id` (ON DELETE SET NULL)

**Indexes** :
- Index sur `ListId` (requêtes par liste)
- Index sur `Year` (requêtes par année)
- Index sur `TakenBy` (requêtes panier utilisateur)

**Relations** :
- **N → 1** : Plusieurs cadeaux appartiennent à une liste
- **N → 1** : Plusieurs cadeaux peuvent être réservés par un utilisateur
- **1 → N** : Un cadeau peut avoir plusieurs participations (si groupé)

**Logique Métier** :
- **Cadeau classique** : `IsGroupGift = FALSE`, `Available = FALSE` quand réservé, `TakenBy` renseigné
- **Cadeau groupé** : `IsGroupGift = TRUE`, `Available = TRUE` toujours, participations dans table dédiée

**Exemple Cadeau Classique** :
```json
{
  "id": 123,
  "name": "Nintendo Switch",
  "description": "Console de jeu portable",
  "link": "https://www.example.com/switch",
  "cost": 299.99,
  "currency": "EUR",
  "image": "https://www.example.com/image.jpg",
  "year": 2025,
  "listId": 10,
  "takenBy": 5,
  "available": false,
  "comment": "Avec joie !",
  "isGroupGift": false,
  "createdAt": "2025-12-01T00:00:00Z",
  "updatedAt": "2025-12-05T14:30:00Z"
}
```

**Exemple Cadeau Groupé** :
```json
{
  "id": 124,
  "name": "Vélo électrique",
  "description": "VTT électrique de montagne",
  "link": "https://www.example.com/velo",
  "cost": 1500.00,
  "currency": "EUR",
  "image": "https://www.example.com/velo.jpg",
  "year": 2025,
  "listId": 10,
  "takenBy": null,
  "available": true,
  "comment": null,
  "isGroupGift": true,
  "createdAt": "2025-12-01T00:00:00Z",
  "updatedAt": "2025-12-01T00:00:00Z"
}
```

---

### Table : `gift_participations`

Représente les participations aux cadeaux groupés.

| Colonne | Type | Nullable | Default | Description |
|---------|------|----------|---------|-------------|
| `Id` | int | No | AUTO_INCREMENT | Identifiant unique |
| `GiftId` | int | No | - | Référence au cadeau |
| `UserId` | int | No | - | Participant |
| `Comment` | nvarchar(500) | Yes | NULL | Commentaire du participant |
| `CreatedAt` | datetime | No | CURRENT_TIMESTAMP | Date de participation |

**Contraintes** :
- **PRIMARY KEY** : `Id`
- **FOREIGN KEY** : `GiftId` → `gifts.Id` (ON DELETE CASCADE)
- **FOREIGN KEY** : `UserId` → `users.Id` (ON DELETE CASCADE)
- **UNIQUE** : (`GiftId`, `UserId`) - un user ne peut participer qu'une fois

**Indexes** :
- Index composé sur (`GiftId`, `UserId`)
- Index sur `GiftId` (compter participants)

**Relations** :
- **N → 1** : Plusieurs participations pour un cadeau
- **N → 1** : Plusieurs participations pour un utilisateur

**Exemple** :
```json
{
  "id": 50,
  "giftId": 124,
  "userId": 5,
  "comment": "Je participe à hauteur de 200€",
  "createdAt": "2025-12-05T14:30:00Z"
}
```

---

### Table : `password_reset_tokens`

Stocke les tokens temporaires de réinitialisation de mot de passe.

| Colonne | Type | Nullable | Default | Description |
|---------|------|----------|---------|-------------|
| `Id` | int | No | AUTO_INCREMENT | Identifiant unique |
| `UserId` | int | No | - | Utilisateur concerné |
| `Token` | nvarchar(255) | No | - | Token unique (GUID) |
| `ExpiresAt` | datetime | No | - | Date d'expiration (24h typiquement) |
| `Used` | boolean | No | FALSE | Token déjà utilisé |
| `CreatedAt` | datetime | No | CURRENT_TIMESTAMP | Date de création |

**Contraintes** :
- **PRIMARY KEY** : `Id`
- **FOREIGN KEY** : `UserId` → `users.Id` (ON DELETE CASCADE)
- **UNIQUE** : `Token`

**Indexes** :
- Index unique sur `Token` (validation rapide)
- Index sur `UserId` (historique user)

**Relations** :
- **N → 1** : Plusieurs tokens peuvent exister pour un utilisateur

**Logique Métier** :
- **Génération** : Token GUID aléatoire, expire après 24h
- **Validation** : Vérifie non-expiré ET non-utilisé
- **Usage unique** : `Used = TRUE` après utilisation
- **Nettoyage** : Les tokens expirés peuvent être supprimés périodiquement

**Exemple** :
```json
{
  "id": 42,
  "userId": 2,
  "token": "f47ac10b-58cc-4372-a567-0e02b2c3d479",
  "expiresAt": "2025-12-10T14:30:00Z",
  "used": false,
  "createdAt": "2025-12-09T14:30:00Z"
}
```

---

### Table : `opengraph_requests`

Tracking des appels à l'API OpenGraph (monitoring et stats admin).

| Colonne | Type | Nullable | Default | Description |
|---------|------|----------|---------|-------------|
| `Id` | int | No | AUTO_INCREMENT | Identifiant unique |
| `UserId` | int | No | - | Utilisateur ayant fait la requête |
| `Url` | nvarchar(2000) | No | - | URL analysée |
| `Success` | boolean | No | FALSE | Requête réussie |
| `CreatedAt` | datetime | No | CURRENT_TIMESTAMP | Date de la requête |

**Contraintes** :
- **PRIMARY KEY** : `Id`
- **FOREIGN KEY** : `UserId` → `users.Id` (ON DELETE CASCADE)

**Indexes** :
- Index sur `CreatedAt` (stats par période)
- Index sur `UserId` (usage par utilisateur)

**Objectif** :
- **Monitoring** : Suivre l'utilisation de l'API externe (coût)
- **Stats admin** : Dashboard avec graphiques par mois
- **Debugging** : Identifier les URLs problématiques

**Exemple** :
```json
{
  "id": 1523,
  "userId": 2,
  "url": "https://www.amazon.fr/nintendo-switch",
  "success": true,
  "createdAt": "2025-12-09T15:45:00Z"
}
```

## Diagramme ERD

Voir [docs/diagrams/database-erd.md](../diagrams/database-erd.md) pour le diagramme complet avec Mermaid.

## Relations Récapitulatives

```
families (1) ──────────> (N) users
users (1) ──────────> (1) gift_lists
gift_lists (1) ──────────> (N) gifts
users (1) ──────────> (N) gifts (TakenBy)
gifts (1) ──────────> (N) gift_participations
users (1) ──────────> (N) gift_participations
users (1) ──────────> (N) password_reset_tokens
users (1) ──────────> (N) opengraph_requests
```

### Cascade Delete

| Relation | Comportement |
|----------|--------------|
| `family` supprimée | **RESTRICT** : Erreur si users existent |
| `user` supprimé | **CASCADE** : Liste + cadeaux + participations supprimés |
| `gift_list` supprimée | **CASCADE** : Tous les cadeaux supprimés |
| `gift` supprimé | **CASCADE** : Participations supprimées |
| `gift` réservé par user supprimé | **SET NULL** : `TakenBy = NULL` |

## Migrations Entity Framework Core

### Commandes Utiles

```bash
# Créer une nouvelle migration
dotnet ef migrations add NomDescriptifMigration

# Appliquer toutes les migrations en attente
dotnet ef database update

# Rollback vers une migration spécifique
dotnet ef database update PreviousMigrationName

# Générer script SQL (sans l'exécuter)
dotnet ef migrations script

# Supprimer la dernière migration (si pas encore appliquée)
dotnet ef migrations remove

# Lister toutes les migrations
dotnet ef migrations list
```

### Historique des Migrations

Les migrations sont stockées dans `backend/Nawel.Api/Migrations/` avec un timestamp :
```
20250101000000_InitialCreate.cs
20250115000000_AddPasswordResetTokens.cs
20250201000000_AddGiftParticipations.cs
...
```

### Appliquer les Migrations en Production

**Option 1 : Automatique au startup (dev)** :
```csharp
// Dans Program.cs (déjà implémenté pour SQLite)
if (useSqlite && app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<NawelDbContext>();
    dbContext.Database.EnsureCreated();
}
```

**Option 2 : Manuelle (prod recommandée)** :
```bash
# Se connecter au serveur de production
ssh user@production-server

# Naviguer vers l'application
cd /var/www/nawel-api

# Appliquer les migrations
dotnet ef database update --connection "Server=mysql;Database=nawel_db;..."
```

**Option 3 : Script SQL (très recommandée pour prod)** :
```bash
# Générer le script sur votre machine locale
dotnet ef migrations script --output migrations.sql

# Copier sur serveur
scp migrations.sql user@production-server:/tmp/

# Exécuter manuellement
mysql -h mysql-server -u root -p nawel_db < /tmp/migrations.sql
```

## Seeding (Données de Test)

### DbSeeder

Le fichier `Data/DbSeeder.cs` contient la logique de seeding pour le développement :

```csharp
public static void SeedTestData(NawelDbContext context)
{
    // Créer l'utilisateur admin si inexistant
    if (!context.Users.Any(u => u.Id == 1))
    {
        var adminFamily = new Family { Name = "Admin Family" };
        context.Families.Add(adminFamily);
        context.SaveChanges();

        var adminUser = new User
        {
            Login = "admin",
            Password = BCrypt.Net.BCrypt.HashPassword("admin"),
            Email = "admin@nawel.com",
            FirstName = "Admin",
            IsAdmin = true,
            FamilyId = adminFamily.Id
        };
        context.Users.Add(adminUser);
        context.SaveChanges();

        // Créer liste admin
        var adminList = new GiftList
        {
            Name = "Liste Admin",
            UserId = adminUser.Id
        };
        context.Lists.Add(adminList);
        context.SaveChanges();
    }

    // Autres données de test...
}
```

**Activation** :
```csharp
// Dans Program.cs
if (useSqlite && app.Environment.IsDevelopment())
{
    DbSeeder.SeedTestData(dbContext);
}
```

### Données de Test Recommandées

Pour un environnement de développement complet :
1. **Familles** : 2-3 familles
2. **Utilisateurs** : 5-10 users (admin, parents, enfants)
3. **Listes** : 1 par user
4. **Cadeaux** : 5-10 par liste (années 2024, 2025)
5. **Participations** : Quelques cadeaux groupés avec 2-3 participants

## Requêtes SQL Utiles

### Statistiques Globales

```sql
-- Nombre d'utilisateurs par famille
SELECT f.Name, COUNT(u.Id) AS UserCount
FROM families f
LEFT JOIN users u ON u.FamilyId = f.Id
WHERE u.Id != 1  -- Exclure admin
GROUP BY f.Id, f.Name
ORDER BY UserCount DESC;

-- Cadeaux les plus chers par année
SELECT Year, Name, Cost, Currency
FROM gifts
WHERE Cost IS NOT NULL
ORDER BY Year DESC, Cost DESC
LIMIT 10;

-- Taux de réservation par année
SELECT
    Year,
    COUNT(*) AS TotalGifts,
    SUM(CASE WHEN Available = FALSE THEN 1 ELSE 0 END) AS ReservedGifts,
    ROUND(100.0 * SUM(CASE WHEN Available = FALSE THEN 1 ELSE 0 END) / COUNT(*), 2) AS ReservationRate
FROM gifts
GROUP BY Year
ORDER BY Year DESC;

-- Utilisateurs les plus actifs (réservations)
SELECT
    u.FirstName,
    u.LastName,
    COUNT(g.Id) AS GiftsReserved,
    SUM(g.Cost) AS TotalValue
FROM users u
LEFT JOIN gifts g ON g.TakenBy = u.Id
WHERE g.Year = 2025
GROUP BY u.Id, u.FirstName, u.LastName
ORDER BY GiftsReserved DESC
LIMIT 10;
```

### Nettoyage

```sql
-- Supprimer les tokens expirés (à faire périodiquement)
DELETE FROM password_reset_tokens
WHERE ExpiresAt < NOW();

-- Supprimer les anciens cadeaux (ex: plus de 3 ans)
DELETE FROM gifts
WHERE Year < YEAR(NOW()) - 3;

-- Archiver les requêtes OpenGraph anciennes
DELETE FROM opengraph_requests
WHERE CreatedAt < DATE_SUB(NOW(), INTERVAL 1 YEAR);
```

## Optimisations

### Indexes Recommandés

Tous les indexes importants sont déjà créés via EF Core migrations :
- ✅ `families.Name` (UNIQUE)
- ✅ `users.Login` (UNIQUE)
- ✅ `users.FamilyId`
- ✅ `gift_lists.UserId` (UNIQUE)
- ✅ `gifts.ListId`
- ✅ `gifts.Year`
- ✅ `gifts.TakenBy`
- ✅ `gift_participations.(GiftId, UserId)` (UNIQUE)
- ✅ `password_reset_tokens.Token` (UNIQUE)
- ✅ `opengraph_requests.CreatedAt`

### Indexes Futurs (si Performance)

```sql
-- Si recherche par nom de cadeau fréquente
CREATE INDEX idx_gifts_name ON gifts(Name);

-- Si recherche d'emails fréquente
CREATE INDEX idx_users_email ON users(Email);
```

### Considérations Performance

1. **N+1 Queries** : Toujours utiliser `.Include()` dans EF Core
   ```csharp
   // ❌ BAD : N+1
   var gifts = await _context.Gifts.ToListAsync();
   foreach (var gift in gifts)
   {
       var user = gift.TakenByUser;  // Query for each gift!
   }

   // ✅ GOOD : 1 query
   var gifts = await _context.Gifts
       .Include(g => g.TakenByUser)
       .ToListAsync();
   ```

2. **Pagination** : Utiliser `Skip()` et `Take()` pour grandes listes
   ```csharp
   var gifts = await _context.Gifts
       .OrderBy(g => g.CreatedAt)
       .Skip(page * pageSize)
       .Take(pageSize)
       .ToListAsync();
   ```

3. **AsNoTracking** : Pour lectures seules (à implémenter)
   ```csharp
   var gifts = await _context.Gifts
       .AsNoTracking()  // Pas de change tracking
       .ToListAsync();
   ```

## Backups

### MySQL (Production)

```bash
# Backup complet
mysqldump -h mysql-server -u root -p nawel_db > backup_$(date +%Y%m%d).sql

# Backup avec compression
mysqldump -h mysql-server -u root -p nawel_db | gzip > backup_$(date +%Y%m%d).sql.gz

# Restore
mysql -h mysql-server -u root -p nawel_db < backup_20251209.sql
```

### SQLite (Développement)

```bash
# Backup (simple copie)
cp nawel.db nawel_backup_$(date +%Y%m%d).db

# Restore
cp nawel_backup_20251209.db nawel.db
```

### Stratégie Recommandée (Production)

- **Backups quotidiens** automatisés (cron)
- **Rétention** : 7 jours complets + 4 backups hebdomadaires + 12 mensuels
- **Stockage** : Hors serveur (S3, NAS, etc.)
- **Tests de restore** : Au moins 1 fois par trimestre

## Sécurité

### Permissions MySQL

```sql
-- Créer utilisateur application (lecture/écriture)
CREATE USER 'nawel_app'@'%' IDENTIFIED BY '***';
GRANT SELECT, INSERT, UPDATE, DELETE ON nawel_db.* TO 'nawel_app'@'%';

-- Créer utilisateur backup (lecture seule)
CREATE USER 'nawel_backup'@'localhost' IDENTIFIED BY '***';
GRANT SELECT, LOCK TABLES ON nawel_db.* TO 'nawel_backup'@'localhost';

-- Créer utilisateur admin (tous droits)
CREATE USER 'nawel_admin'@'localhost' IDENTIFIED BY '***';
GRANT ALL PRIVILEGES ON nawel_db.* TO 'nawel_admin'@'localhost';

FLUSH PRIVILEGES;
```

### Connexions Sécurisées

- ✅ **SSL/TLS** : Toujours en production
- ✅ **Firewall** : Restreindre accès MySQL au serveur app uniquement
- ✅ **Passwords** : Jamais en clair dans code (variables d'environnement)
- ✅ **SQL Injection** : Protection automatique via EF Core (parameterized queries)

## Monitoring

### Métriques à Surveiller

1. **Taille de la DB** : Croissance anormale
   ```sql
   SELECT
       table_schema AS 'Database',
       SUM(data_length + index_length) / 1024 / 1024 AS 'Size (MB)'
   FROM information_schema.tables
   WHERE table_schema = 'nawel_db'
   GROUP BY table_schema;
   ```

2. **Connexions actives**
   ```sql
   SHOW PROCESSLIST;
   ```

3. **Slow queries** : Activer slow query log MySQL

4. **Deadlocks** : Monitorer via logs

## Références

- [Entity Framework Core Documentation](https://learn.microsoft.com/en-us/ef/core/)
- [MySQL 8.0 Reference Manual](https://dev.mysql.com/doc/refman/8.0/en/)
- [Database Design Best Practices](https://www.sqlshack.com/learn-sql-database-design-best-practices/)
