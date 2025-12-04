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

## Déploiement sur VPS (À venir)

Documentation complète à venir dans la Phase 9.

Points clés :
- Configuration Nginx reverse proxy
- Certificat SSL avec Let's Encrypt
- CI/CD avec GitHub Actions
- Déploiement automatique sur push

## Contribution

Projet personnel familial. Pas de contributions externes pour le moment.

## Licence

Projet privé - Tous droits réservés

## Contact

Pour toute question ou problème, créer une issue dans le repository.

---

**Version**: 2.0.0 (Refonte complète)
**Dernière mise à jour**: Décembre 2024
