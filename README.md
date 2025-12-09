# Nawel - Application de Gestion de Listes de Noël

Application web complète permettant de gérer les listes de cadeaux de Noël en famille.

## Stack Technique

- **Frontend**: React 18 + Vite + Material-UI + LESS
- **Backend**: ASP.NET Core 9.0 Web API
- **Base de données**: MySQL 8.0
- **Authentification**: JWT (JSON Web Tokens)
- **Containerisation**: Docker + Docker Compose
- **Sécurité**: BCrypt pour les mots de passe

## Fonctionnalités

- Authentification (login/password)
- Gestion de sa propre liste de cadeaux
- Consultation des listes des autres membres
- Réservation de cadeaux
- Participation à des cadeaux groupés
- Panier récapitulatif
- Notifications par email
- Gestion des avatars utilisateurs
- Historique des listes par année
- Import des cadeaux non pris de l'année précédente
- Interface d'administration

## Architecture

L'application suit une architecture en couches avec séparation stricte entre frontend et backend :

- **Frontend** : Application React avec gestion d'état via Context API, routage avec React Router, et composants Material-UI
- **Backend** : API REST ASP.NET Core suivant les principes de Clean Architecture (Controllers → Services → Data Layer)
- **Base de données** : MySQL 8.0 en production, SQLite en développement
- **Authentification** : JWT Bearer tokens avec BCrypt pour le hashing des mots de passe
- **Communication** : API REST JSON avec validation des données et gestion d'erreurs centralisée

📊 **Diagrammes détaillés** :
- [Architecture système complète](docs/diagrams/system-architecture.md) - Vue d'ensemble avec flux de données
- [Diagramme ERD de la base de données](docs/diagrams/database-erd.md) - Schéma relationnel complet

## Documentation

### 📚 Documentation Backend

| Document | Description |
|----------|-------------|
| [**API Documentation (Swagger)**](http://localhost:5000/swagger) | Documentation interactive de l'API REST (disponible une fois le backend démarré) |
| [Guide d'utilisation Swagger](docs/api/README.md) | Comment utiliser Swagger UI et tester les endpoints |
| [Architecture Backend](docs/backend/ARCHITECTURE.md) | Architecture en couches, services, middleware, patterns utilisés |
| [Base de Données](docs/backend/DATABASE.md) | Schéma complet, migrations, requêtes SQL, optimisations |

### 🎨 Documentation Frontend

| Document | Description |
|----------|-------------|
| [Architecture Frontend](docs/frontend/ARCHITECTURE.md) | Stack technique, architecture en couches, routing, patterns |
| [Pages](docs/frontend/PAGES.md) | Documentation des 7 pages (Home, MyList, UserList, Cart, Profile, Admin, Login) |
| [Composants](docs/frontend/COMPONENTS.md) | Documentation des 18 composants réutilisables (Layout, Gifts, Profile, Admin) |
| [State Management](docs/frontend/STATE-MANAGEMENT.md) | AuthContext, local state, persistence (localStorage) |
| [Services API](docs/frontend/API-SERVICES.md) | Client Axios, 6 groupes d'API (~40 endpoints), interceptors |
| [Styling](docs/frontend/STYLING.md) | Material-UI theme, CSS Modules + LESS, responsive design |
| [Tests](docs/frontend/TESTING.md) | Guide complet des tests (Vitest, RTL, mocking, patterns, bonnes pratiques) |

### 👥 Guides Utilisateurs

| Document | Description |
|----------|-------------|
| [Guide de Démarrage](docs/user-guide/GETTING-STARTED.md) | Premier pas avec Nawel : connexion, navigation, création de listes, réservations |
| [Guide des Fonctionnalités](docs/user-guide/FEATURES.md) | Guide détaillé de toutes les fonctionnalités (10 sections, cas d'usage avancés) |

### 🔧 Configuration Avancée

| Document | Description |
|----------|-------------|
| [Configuration Backend](docs/backend/CONFIGURATION.md) | Configuration complète : JWT, Email/SMTP, Database, Rate Limiting, CORS, déploiement |

### 📖 Documentation Générale

| Document | Description |
|----------|-------------|
| [Guide de Dépannage](docs/TROUBLESHOOTING.md) | Solutions aux problèmes courants (backend, frontend, Docker, DB) |

### 📊 Diagrammes

| Diagramme | Description |
|-----------|-------------|
| [Architecture Système](docs/diagrams/system-architecture.md) | Vue d'ensemble de l'architecture avec flux de données (Mermaid) |
| [Diagramme ERD](docs/diagrams/database-erd.md) | Schéma relationnel de la base de données (Mermaid) |
| [Flux Utilisateurs](docs/diagrams/user-flows.md) | 8 diagrammes de flux utilisateurs complets (connexion, cadeaux, réservation, etc.) |

### 🧪 Tests

Le projet inclut des tests unitaires et d'intégration. Pour un guide complet, voir [Documentation des Tests Frontend](docs/frontend/TESTING.md).

**Frontend** :
```bash
cd frontend/nawel-app

# Exécuter tous les tests (mode watch)
npm test

# Tests avec UI interactive
npm run test:ui

# Tests avec couverture
npm run test:coverage
```

**Backend** :
```bash
cd backend/Nawel.Api

# Exécuter tous les tests
dotnet test

# Tests avec détails
dotnet test --logger "console;verbosity=detailed"

# Tests avec couverture
dotnet test --collect:"XPlat Code Coverage"
```

**Couverture actuelle** :
- Frontend : 49 tests passants (Avatar, AuthContext, Login, helpers)
  - Stack : Vitest 4.0.15 + React Testing Library 16.3.0
- Backend : Tests d'intégration à implémenter

## Structure du Projet

```
nawel/
├── backend/
│   └── Nawel.Api/          # API ASP.NET Core
│       ├── Controllers/
│       ├── Models/
│       ├── Services/
│       ├── Data/
│       └── Dockerfile
├── frontend/
│   └── nawel-app/          # Application React
│       ├── src/
│       ├── public/
│       ├── Dockerfile
│       └── nginx.conf
├── database/
│   ├── migrations/         # Scripts de migration SQL
│   └── seeds/              # Données de test
├── old/                    # Ancienne version (référence)
├── docker-compose.yml      # Composition complète (prod)
├── docker-compose.dev.yml  # MySQL uniquement (dev)
└── README.md
```

## Installation et Démarrage

### Prérequis

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- [Node.js 20+](https://nodejs.org/)
- [Docker Desktop](https://www.docker.com/products/docker-desktop) (optionnel mais recommandé)

### Option 1: Développement Local (sans Docker)

#### 1. Base de données MySQL

Démarrez uniquement MySQL avec Docker :

```bash
docker-compose -f docker-compose.dev.yml up -d
```

Ou installez MySQL localement et importez le schéma :

```bash
mysql -u root -p < database/migrations/001_initial_schema.sql
```

#### 2. Backend (API)

```bash
cd backend/Nawel.Api

# Configurer la connection string dans appsettings.json
# Ensuite démarrer l'API
dotnet run
```

L'API sera disponible sur `http://localhost:5000`

#### 3. Frontend (React)

```bash
cd frontend/nawel-app

# Installer les dépendances (si pas déjà fait)
npm install

# Démarrer le serveur de développement
npm run dev
```

Le frontend sera disponible sur `http://localhost:5173`

### Option 2: Docker Compose (Environnement Complet)

Démarrer toute l'application (MySQL + Backend + Frontend) :

```bash
# Build et démarrage
docker-compose up --build -d

# Voir les logs
docker-compose logs -f

# Arrêter
docker-compose down
```

Accès :
- Frontend: `http://localhost:3000`
- Backend API: `http://localhost:5000`
- MySQL: `localhost:3306`

## Configuration

### Backend (appsettings.json)

Créez un fichier `backend/Nawel.Api/appsettings.json` :

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=nawel;User=nawel_user;Password=nawel_pass;"
  },
  "Jwt": {
    "Secret": "your_super_secret_jwt_key_change_me_minimum_32_characters",
    "Issuer": "NawelApi",
    "Audience": "NawelApp",
    "ExpirationMinutes": 60
  },
  "Smtp": {
    "Host": "smtp.example.com",
    "Port": 587,
    "Username": "",
    "Password": "",
    "FromEmail": "no-reply@nawel.com",
    "FromName": "Nawel App",
    "EnableSsl": true
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

### Frontend (.env)

Créez un fichier `frontend/nawel-app/.env` :

```env
VITE_API_URL=http://localhost:5000/api
```

## Migration des Données Existantes

Pour migrer depuis l'ancienne base de données :

```bash
# 1. Importer les données existantes
mysql -u root -p nawel < old/nironico_nawel.sql

# 2. Appliquer les migrations
mysql -u root -p nawel < database/migrations/002_migrate_data.sql
```

**Note**: Les mots de passe existants (MD5) seront automatiquement migrés vers BCrypt lors de la première connexion de chaque utilisateur.

## Scripts Utiles

### Backend

```bash
cd backend/Nawel.Api

# Restaurer les packages
dotnet restore

# Build
dotnet build

# Run
dotnet run

# Watch mode (redémarrage auto)
dotnet watch run

# Tests
dotnet test
```

### Frontend

```bash
cd frontend/nawel-app

# Installer les dépendances
npm install

# Démarrage dev
npm run dev

# Build production
npm run build

# Preview du build
npm run preview

# Linter
npm run lint
```

### Docker

```bash
# Démarrer en dev (MySQL uniquement)
docker-compose -f docker-compose.dev.yml up -d

# Démarrer complet (prod-like)
docker-compose up --build -d

# Voir les logs
docker-compose logs -f [service_name]

# Arrêter
docker-compose down

# Supprimer volumes (réinitialiser BDD)
docker-compose down -v

# Rebuild un service spécifique
docker-compose build backend
docker-compose up -d backend
```

## Plan de Développement

### Phase 1: Setup & Infrastructure ✅ (TERMINÉE)
- Structure du projet
- Configuration Docker
- Scripts de migration BDD

### Phase 2: Authentification & Gestion utilisateurs (EN COURS)
- Login/Logout avec JWT
- Reset password
- Gestion profil utilisateur
- Upload avatar

### Phase 3: Homepage & Navigation
- Liste des utilisateurs groupés par famille
- Navigation entre les pages

### Phase 4: Ma liste (édition/consultation)
- Édition de sa liste (année courante uniquement)
- Consultation historique (mode lecture)
- Import des cadeaux non pris

### Phase 5: Listes des autres utilisateurs
- Consultation des listes
- Réservation de cadeaux
- Participation aux cadeaux groupés
- Commentaires

### Phase 6: Panier
- Récapitulatif des cadeaux réservés
- Calcul des totaux

### Phase 7: Administration
- Gestion des utilisateurs
- Création de comptes

### Phase 8: Tests & Finitions
- Tests manuels
- Templates d'emails
- Responsive design

### Phase 9: Déploiement VPS
- Configuration serveur
- CI/CD avec GitHub Actions
- SSL/HTTPS

## Déploiement

### Déploiement Docker (Recommandé)

L'application est prête pour le déploiement via Docker Compose :

```bash
# Production avec toutes les variables d'environnement
docker-compose up -d

# Vérifier les logs
docker-compose logs -f

# Arrêter et supprimer les conteneurs
docker-compose down
```

**Variables d'environnement requises** :

```bash
# JWT Configuration (OBLIGATOIRE)
JWT_SECRET=your_secret_minimum_32_characters_required

# Base de données (MySQL)
MYSQL_ROOT_PASSWORD=your_mysql_root_password
ConnectionStrings__DefaultConnection=Server=mysql;Database=nawel_db;User=root;Password=***

# Email (optionnel en dev, requis en prod)
EMAIL_SMTP_SERVER=smtp.gmail.com
EMAIL_SMTP_PORT=587
EMAIL_USERNAME=your-email@gmail.com
EMAIL_PASSWORD=your_app_password

# OpenGraph API (pour extraction métadonnées produits)
OPENGRAPH_API_KEY=your_api_key
```

📖 **Voir aussi** :
- [Architecture système - Déploiement](docs/diagrams/system-architecture.md#déploiement) - Architecture de déploiement recommandée
- [Backend Architecture - Docker](docs/backend/ARCHITECTURE.md#déploiement) - Configuration Docker détaillée
- [Guide de dépannage - Docker](docs/TROUBLESHOOTING.md#problèmes-docker) - Résolution de problèmes Docker

### Déploiement sur VPS

Points clés pour un déploiement sur VPS :

1. **Nginx Reverse Proxy** : Configuration pour servir le frontend et proxy vers l'API
2. **SSL/HTTPS** : Certificat Let's Encrypt avec renouvellement automatique
3. **Sécurité** :
   - JWT secret robuste (256+ bits)
   - Firewall configuré (ports 80, 443, 22 uniquement)
   - Rate limiting activé
   - CORS configuré avec origines explicites
4. **Monitoring** : Logs centralisés, alertes sur erreurs critiques
5. **Backups** : Backups quotidiens automatiques de MySQL

📖 **Configuration détaillée** : Voir [docs/backend/ARCHITECTURE.md - Déploiement](docs/backend/ARCHITECTURE.md#déploiement)

## Contribution

Projet personnel familial. Pas de contributions externes pour le moment.

## Licence

Projet privé - Tous droits réservés

## Contact

Pour toute question ou problème, créer une issue dans le repository.

---

**Version**: 2.0.0 (Refonte complète)
**Dernière mise à jour**: Décembre 2024
