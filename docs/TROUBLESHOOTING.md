# Guide de Dépannage - Nawel

## Vue d'Ensemble

Ce guide couvre les problèmes courants rencontrés lors du développement, du déploiement et de l'utilisation de l'application Nawel, avec leurs solutions.

## Table des Matières

- [Problèmes Backend](#problèmes-backend)
- [Problèmes Frontend](#problèmes-frontend)
- [Problèmes Base de Données](#problèmes-base-de-données)
- [Problèmes Docker](#problèmes-docker)
- [Problèmes d'Authentification](#problèmes-dauthentification)
- [Problèmes Email](#problèmes-email)
- [Problèmes de Performance](#problèmes-de-performance)

---

## Problèmes Backend

### Erreur : "JWT Secret is too short"

**Symptôme** :
```
System.InvalidOperationException: JWT Secret must be at least 32 characters
```

**Cause** :
Le secret JWT configuré est trop court (< 32 caractères).

**Solution** :
1. Ouvrir `appsettings.json` ou définir la variable d'environnement
2. S'assurer que `JWT_SECRET` fait au moins 32 caractères

```bash
# Générer un secret sécurisé (Linux/Mac)
openssl rand -base64 32

# Ou (PowerShell)
[Convert]::ToBase64String((1..32 | ForEach-Object { Get-Random -Minimum 0 -Maximum 256 }))
```

3. Définir la variable :
```bash
export JWT_SECRET="votre_secret_de_32_caracteres_minimum"
```

---

### Erreur : "Cannot connect to MySQL server"

**Symptôme** :
```
MySqlConnector.MySqlException: Unable to connect to any of the specified MySQL hosts
```

**Causes possibles** :
1. Serveur MySQL non démarré
2. Mauvaises credentials
3. Firewall bloque le port 3306
4. Host incorrect dans connection string

**Solutions** :

**Vérifier que MySQL est actif** :
```bash
# Linux/Mac
sudo systemctl status mysql

# Windows
sc query MySQL80

# Docker
docker ps | grep mysql
```

**Tester la connexion** :
```bash
mysql -h localhost -u root -p
# Entrer le mot de passe
```

**Vérifier le connection string** :
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "server=localhost;port=3306;database=nawel_db;user=root;password=votre_password"
  }
}
```

**Vérifier le firewall** :
```bash
# Linux
sudo ufw allow 3306

# Vérifier le port
netstat -an | grep 3306
```

---

### Erreur : "Failed to apply migrations"

**Symptôme** :
```
Microsoft.EntityFrameworkCore.DbUpdateException: An error occurred while updating the entries
```

**Cause** :
Les migrations n'ont pas été appliquées ou la base de données est dans un état incohérent.

**Solution** :

**Vérifier l'état des migrations** :
```bash
cd backend/Nawel.Api
dotnet ef migrations list
```

**Appliquer les migrations** :
```bash
dotnet ef database update
```

**Si la DB est corrompue (DEV UNIQUEMENT)** :
```bash
# ATTENTION : Supprime toutes les données !
dotnet ef database drop
dotnet ef database update
```

---

### Erreur : "Rate limit exceeded"

**Symptôme** :
```
429 Too Many Requests
{ "message": "Rate limit exceeded. Try again later." }
```

**Cause** :
Trop de requêtes depuis la même IP (protection anti-brute-force).

**Solutions** :

**En développement** : Désactiver temporairement dans `appsettings.json`
```json
{
  "IpRateLimiting": {
    "EnableEndpointRateLimiting": false
  }
}
```

**En production** : Attendre l'expiration du compteur (1-15 minutes selon endpoint).

**Ajuster les limites** (si légitime) :
```json
{
  "IpRateLimitPolicies": {
    "IpRules": [
      {
        "Ip": "*",
        "Rules": [
          {
            "Endpoint": "POST:/api/auth/login",
            "Period": "1m",
            "Limit": 10  // Augmenter de 5 à 10
          }
        ]
      }
    ]
  }
}
```

---

### Erreur : "CORS policy error"

**Symptôme** (dans la console navigateur) :
```
Access to XMLHttpRequest at 'http://localhost:5000/api/...' from origin 'http://localhost:3000'
has been blocked by CORS policy
```

**Cause** :
L'origin du frontend n'est pas autorisé dans la configuration CORS du backend.

**Solution** :

**Vérifier Program.cs** :
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
            "http://localhost:3000",
            "http://localhost:5173",  // Vite default
            "http://localhost:5174",
            "http://localhost:5175"
        )
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();
    });
});
```

**Ajouter votre origin** si différent.

**En production**, remplacer par votre domaine :
```csharp
policy.WithOrigins("https://nawel.exemple.com")
```

---

## Problèmes Frontend

### Erreur : "VITE_API_URL is not defined"

**Symptôme** :
L'application ne peut pas se connecter à l'API, erreurs de connexion.

**Cause** :
La variable d'environnement `VITE_API_URL` n'est pas définie.

**Solution** :

**Créer `.env` à la racine de `frontend/nawel-app/`** :
```bash
VITE_API_URL=http://localhost:5000
```

**Redémarrer le serveur de dev** :
```bash
npm run dev
```

---

### Erreur : "Cannot upload avatar - 413 Payload Too Large"

**Symptôme** :
L'upload d'avatar échoue avec une erreur 413.

**Causes** :
1. Fichier > 5MB (limite par défaut)
2. Limite Nginx trop basse

**Solutions** :

**Augmenter la limite backend** (`appsettings.json`) :
```json
{
  "FileStorage": {
    "MaxFileSizeMB": 10
  }
}
```

**Augmenter la limite Nginx** (`nginx.conf`) :
```nginx
http {
    client_max_body_size 10M;
}
```

---

### Erreur : "Token expired" ou "401 Unauthorized"

**Symptôme** :
L'utilisateur est déconnecté de manière inattendue, erreurs 401.

**Cause** :
Le token JWT a expiré (durée par défaut : 7 jours).

**Solution** :

**Ajuster l'expiration** dans `appsettings.json` :
```json
{
  "Jwt": {
    "ExpiryDays": 30
  }
}
```

**Implémenter un refresh token** (futur) pour éviter les déconnexions.

**En développement**, augmenter à 365 jours :
```json
{ "Jwt": { "ExpiryDays": 365 } }
```

---

## Problèmes Base de Données

### Erreur : "Duplicate entry 'xxx' for key 'Login'"

**Symptôme** :
```
MySql.Data.MySqlClient.MySqlException: Duplicate entry 'john_doe' for key 'users.Login'
```

**Cause** :
Tentative de créer un utilisateur avec un login déjà existant.

**Solution** :

**Vérifier l'existence avant création** (déjà implémenté dans AdminController) :
```csharp
if (await _context.Users.AnyAsync(u => u.Login == dto.Login))
{
    return BadRequest(new { message = "Ce nom d'utilisateur existe déjà" });
}
```

**Trouver l'utilisateur existant** :
```sql
SELECT * FROM users WHERE Login = 'john_doe';
```

---

### Erreur : "Foreign key constraint fails"

**Symptôme** :
```
Cannot delete or update a parent row: a foreign key constraint fails
```

**Cause** :
Tentative de supprimer une entité référencée par une clé étrangère.

**Exemple** : Supprimer une famille qui contient encore des utilisateurs.

**Solution** :

**Supprimer d'abord les entités enfants** :
```sql
-- Supprimer tous les users de la famille d'abord
DELETE FROM users WHERE FamilyId = 5;

-- Puis supprimer la famille
DELETE FROM families WHERE Id = 5;
```

**Ou utiliser CASCADE DELETE** (déjà configuré pour certaines relations).

---

### Problème : Base de données SQLite verrouillée

**Symptôme** :
```
Microsoft.Data.Sqlite.SqliteException: database is locked
```

**Cause** :
Un autre processus (ou une connexion non fermée) verrouille le fichier `nawel.db`.

**Solutions** :

**Arrêter tous les processus** :
```bash
# Arrêter le backend
pkill -f "dotnet run"

# Ou fermer Visual Studio / Rider
```

**Supprimer les fichiers de verrouillage** :
```bash
rm nawel.db-wal
rm nawel.db-shm
```

**Redémarrer l'application** :
```bash
dotnet run
```

---

## Problèmes Docker

### Erreur : "Cannot connect to Docker daemon"

**Symptôme** :
```
Cannot connect to the Docker daemon at unix:///var/run/docker.sock
```

**Cause** :
Docker n'est pas démarré ou l'utilisateur n'a pas les permissions.

**Solutions** :

**Démarrer Docker** :
```bash
# Linux
sudo systemctl start docker

# Mac/Windows
# Ouvrir Docker Desktop
```

**Ajouter l'utilisateur au groupe docker** (Linux) :
```bash
sudo usermod -aG docker $USER
newgrp docker
```

---

### Erreur : "Port already in use"

**Symptôme** :
```
Error starting userland proxy: listen tcp 0.0.0.0:5000: bind: address already in use
```

**Cause** :
Le port est déjà utilisé par un autre processus.

**Solutions** :

**Trouver le processus** :
```bash
# Linux/Mac
lsof -i :5000

# Windows
netstat -ano | findstr :5000
```

**Tuer le processus** :
```bash
# Linux/Mac
kill -9 <PID>

# Windows
taskkill /PID <PID> /F
```

**Ou changer le port** dans `docker-compose.yml` :
```yaml
services:
  api:
    ports:
      - "5001:8080"  # Utiliser 5001 au lieu de 5000
```

---

### Problème : Images Docker trop volumineuses

**Symptôme** :
L'image Docker fait plusieurs GB.

**Cause** :
Build artifacts non nettoyés, pas de multi-stage build.

**Solution** :

**Vérifier le Dockerfile multi-stage** :
```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
# ... build stage ...

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
# Copier seulement les binaires, pas le SDK
COPY --from=build /app/publish .
```

**Nettoyer les images inutilisées** :
```bash
docker system prune -a
```

---

## Problèmes d'Authentification

### Problème : Impossible de se connecter avec ancien mot de passe MD5

**Symptôme** :
Message "Votre mot de passe doit être réinitialisé pour des raisons de sécurité".

**Cause** :
L'utilisateur a encore un mot de passe MD5 (ancien système).

**Solution** :

**Déclencher la migration** :
1. Utiliser l'endpoint `POST /api/Auth/request-migration-reset`
2. Entrer le login de l'utilisateur
3. Un email de réinitialisation sera envoyé
4. Cliquer sur le lien dans l'email
5. Définir un nouveau mot de passe (sera hashé avec BCrypt)

**Vérifier dans la DB** :
```sql
SELECT Login, LENGTH(Password) AS PasswordLength
FROM users
WHERE LENGTH(Password) = 32  -- MD5 = 32 chars
AND Password NOT LIKE '$2%';  -- BCrypt commence par $2
```

---

### Problème : Token JWT invalide après redémarrage serveur

**Symptôme** :
Tous les utilisateurs sont déconnectés après un redémarrage du backend.

**Cause** :
Le secret JWT a changé (régénéré aléatoirement).

**Solution** :

**Utiliser un secret fixe** via variable d'environnement :
```bash
# .env ou système
export JWT_SECRET="un_secret_fixe_de_32_caracteres_minimum"
```

**Ne JAMAIS commit le secret** dans Git.

---

## Problèmes Email

### Problème : Emails non envoyés

**Symptôme** :
Aucun email reçu, pas d'erreur visible.

**Causes possibles** :
1. SMTP mal configuré
2. Credentials incorrects
3. Firewall bloque le port
4. Email dans spam
5. Serveur SMTP en panne

**Solutions** :

**Vérifier la configuration** (`appsettings.json`) :
```json
{
  "Email": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "UseSsl": true,
    "Username": "votre.email@gmail.com",
    "Password": "votre_mot_de_passe_app",  // Pas le mot de passe Gmail normal !
    "FromEmail": "votre.email@gmail.com"
  }
}
```

**Tester manuellement la connexion SMTP** :
```bash
telnet smtp.gmail.com 587
# Si connexion OK, affiche "220 smtp.gmail.com ESMTP..."
```

**Utiliser Mailpit pour tester en local** (recommandé pour dev) :
```bash
docker run -d --name=mailpit -p 8025:8025 -p 1025:1025 axllent/mailpit

# Configurer dans appsettings.json :
{
  "Email": {
    "SmtpServer": "localhost",
    "SmtpPort": 1025,
    "UseSsl": false
  }
}

# Ouvrir http://localhost:8025 pour voir les emails
```

---

### Problème : Gmail rejette les emails ("Less secure apps")

**Symptôme** :
```
MailKit.Security.AuthenticationException: 535: Username and Password not accepted
```

**Cause** :
Gmail bloque les "apps moins sécurisées" par défaut.

**Solution** :

**Créer un mot de passe d'application** :
1. Aller sur https://myaccount.google.com/apppasswords
2. Créer un nouveau mot de passe d'application "Nawel App"
3. Copier le mot de passe généré (16 caractères)
4. Utiliser ce mot de passe dans `appsettings.json` :

```json
{
  "Email": {
    "Password": "abcd efgh ijkl mnop"  // Mot de passe app, pas votre password Gmail
  }
}
```

---

## Problèmes de Performance

### Problème : Requêtes API très lentes (> 1 seconde)

**Symptômes** :
Temps de réponse élevé, application qui rame.

**Causes possibles** :
1. Requêtes DB N+1
2. Pas d'indexes
3. Trop de données retournées
4. Connexions DB non fermées

**Solutions** :

**Activer le logging des requêtes EF Core** :
```json
{
  "Logging": {
    "LogLevel": {
      "Microsoft.EntityFrameworkCore.Database.Command": "Information"
    }
  }
}
```

**Vérifier les requêtes générées** dans les logs et optimiser.

**Utiliser `.Include()` pour éviter N+1** :
```csharp
// ❌ BAD : N+1
var gifts = await _context.Gifts.ToListAsync();
foreach (var gift in gifts)
{
    var userName = gift.TakenByUser.FirstName;  // Query par gift !
}

// ✅ GOOD : 1 query
var gifts = await _context.Gifts
    .Include(g => g.TakenByUser)
    .ToListAsync();
```

**Ajouter des indexes** si nécessaire :
```sql
CREATE INDEX idx_gifts_year ON gifts(Year);
CREATE INDEX idx_gifts_listid ON gifts(ListId);
```

---

### Problème : Frontend lent à charger

**Symptômes** :
Temps de chargement initial > 3 secondes.

**Causes** :
1. Bundle JavaScript trop gros
2. Pas de code splitting
3. Images non optimisées

**Solutions** :

**Analyser le bundle** :
```bash
npm run build
npx vite-bundle-visualizer
```

**Lazy load des pages** :
```typescript
// Au lieu de :
import AdminPage from './pages/Admin';

// Utiliser :
const AdminPage = lazy(() => import('./pages/Admin'));
```

**Optimiser les images** :
- Utiliser WebP au lieu de PNG/JPG
- Compresser avec TinyPNG ou similaire
- Lazy load les images

---

## FAQ Générale

### Comment réinitialiser complètement la base de données (DEV) ?

```bash
cd backend/Nawel.Api

# SQLite
rm nawel.db
dotnet ef database update

# MySQL
mysql -u root -p -e "DROP DATABASE nawel_db; CREATE DATABASE nawel_db;"
dotnet ef database update
```

### Comment voir les logs en temps réel ?

```bash
# Backend
cd backend/Nawel.Api
dotnet run | tee logs.txt

# Frontend
cd frontend/nawel-app
npm run dev | tee logs.txt

# Docker
docker logs -f nawel-api
```

### Comment débugger une requête API spécifique ?

1. **Browser DevTools** : Onglet Network
2. **Postman / Insomnia** : Tester l'endpoint isolément
3. **Swagger UI** : http://localhost:5000/swagger
4. **Logs backend** : Voir les exceptions et SQL queries

### Comment tester l'envoi d'emails en développement ?

**Option 1 : Mailpit** (recommandé)
```bash
docker run -d --name=mailpit -p 8025:8025 -p 1025:1025 axllent/mailpit
```
- SMTP : localhost:1025
- UI : http://localhost:8025

**Option 2 : Mailtrap**
- S'inscrire sur https://mailtrap.io
- Utiliser les credentials SMTP fournis

**Option 3 : Désactiver l'envoi**
- Commenter le code d'envoi temporairement

---

## Obtenir de l'Aide

Si le problème persiste après avoir suivi ce guide :

1. **Vérifier les logs** : Backend et Frontend
2. **Consulter la documentation** : [docs/](../docs/)
3. **Rechercher l'erreur** : Stack Overflow, GitHub Issues
4. **Créer une issue** : https://github.com/votre-repo/nawel/issues (si applicable)

### Informations à Fournir

Lors de la création d'une issue, inclure :
- **Environnement** : OS, versions (.NET, Node, MySQL)
- **Symptôme** : Description détaillée
- **Logs** : Messages d'erreur complets
- **Étapes pour reproduire** : Liste numérotée
- **Comportement attendu** : Ce qui devrait se passer
- **Captures d'écran** : Si applicable

---

## Références

- [ASP.NET Core Troubleshooting](https://learn.microsoft.com/en-us/aspnet/core/test/troubleshoot)
- [Entity Framework Core Troubleshooting](https://learn.microsoft.com/en-us/ef/core/logging-events-diagnostics/troubleshooting)
- [MySQL Error Codes](https://dev.mysql.com/doc/mysql-errors/8.0/en/)
- [React DevTools](https://react.dev/learn/react-developer-tools)
