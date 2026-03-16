# Nawel - Documentation du projet

## Vue d'ensemble

**Nawel** est une application web de gestion de listes de cadeaux de Noël pour des groupes familiaux. Les utilisateurs créent leur liste de cadeaux, et les autres membres de la famille peuvent consulter ces listes et réserver discrètement des cadeaux.

---

## Architecture technique

```
nawel/
├── backend/
│   ├── Nawel.Api/          # API ASP.NET Core 9 (C#)
│   └── Nawel.Api.Tests/    # Tests xUnit
├── frontend/
│   └── nawel-app/          # React 19 + TypeScript + Vite
├── docker-compose.yml       # Déploiement production
├── docker-compose.dev.yml   # Développement Docker
└── DOCUMENTATION.md
```

### Stack technique

| Couche     | Technologie                        |
|------------|------------------------------------|
| Backend    | ASP.NET Core 9, Entity Framework Core |
| Base de données | MySQL (prod) / SQLite (dev)   |
| Frontend   | React 19, TypeScript, Vite, MUI v7 |
| Auth       | JWT Bearer tokens                  |
| Emails     | SMTP (System.Net.Mail)             |
| CSS        | LESS CSS Modules                   |
| Charts     | Recharts (panel admin)             |

---

## Modèle de données

```
Family (1) ──── (N) User (1) ──── (1) GiftList (1) ──── (N) Gift
                                                              |
                                               GiftParticipation (cadeaux groupés)
```

### Entités principales

**Family** (`family`)
- `id`, `name`, `created_at`

**User** (`user`)
- `id`, `login`, `pwd` (BCrypt), `email`, `first_name`, `last_name`
- `avatar` (chemin fichier, défaut: `avatar.png`)
- `pseudo`, `family_id`
- `is_children` (compte enfant), `is_admin`
- `notify_list_edit`, `notify_gift_taken`, `display_popup` (préférences)
- `reset_token`, `token_expiry` (réinitialisation mot de passe)

**GiftList** (`lists`) — une liste par utilisateur (1:1)
- `id`, `name`, `user_id`

**Gift** (`gifts`)
- `id`, `list_id`, `name`, `description`, `image`, `link`, `cost`, `currency`
- `year` (filtre annuel), `available`, `taken_by` (FK user)
- `is_group_gift` (cadeau groupé), `comment`

**GiftParticipation** (`gift_participation`) — pour les cadeaux groupés
- `id`, `gift_id`, `user_id`, `is_active`

---

## API Backend — Endpoints

Base URL: `/api`

### Auth (`/api/auth`)
| Méthode | Endpoint | Description |
|---------|----------|-------------|
| POST | `/login` | Connexion, retourne JWT + user |
| POST | `/reset-password-request` | Demande reset par email |
| POST | `/reset-password` | Confirmer reset avec token |
| POST | `/request-migration-reset` | Migration MD5 → BCrypt |
| GET  | `/validate-token?token=` | Valider un JWT |

### Users (`/api/users`) — authentifié
| Méthode | Endpoint | Description |
|---------|----------|-------------|
| GET  | `/me` | Profil utilisateur courant |
| PUT  | `/me` | Mettre à jour profil |
| POST | `/me/change-password` | Changer mot de passe |
| POST | `/me/avatar` | Upload avatar (multipart/form-data, clé `file`) |
| DELETE | `/me/avatar` | Supprimer avatar |
| GET  | `/{id}` | Profil d'un utilisateur par ID |

### Lists (`/api/lists`) — authentifié
| Méthode | Endpoint | Description |
|---------|----------|-------------|
| GET | `/` | Toutes les listes groupées par famille |
| GET | `/mine` | Ma liste (infos de base) |

### Gifts (`/api/gifts`) — authentifié
| Méthode | Endpoint | Description |
|---------|----------|-------------|
| GET  | `/my-list?year=` | Mes cadeaux (année courante par défaut) |
| GET  | `/{userId}?year=` | Cadeaux d'un autre utilisateur |
| GET  | `/manage-child/{childId}?year=` | Cadeaux d'un enfant (adultes) |
| GET  | `/years` | Années disponibles dans ma liste |
| POST | `/` | Créer un cadeau |
| POST | `/manage-child/{childId}` | Créer un cadeau pour un enfant |
| PUT  | `/{id}` | Modifier un cadeau |
| DELETE | `/{id}` | Supprimer un cadeau |
| POST | `/{id}/reserve` | Réserver un cadeau |
| DELETE | `/{id}/reserve` | Annuler une réservation |
| POST | `/import-from-year/{year}` | Importer cadeaux non-réservés d'une année passée |

### Products (`/api/products`) — authentifié
| Méthode | Endpoint | Description |
|---------|----------|-------------|
| POST | `/extract-info` | Extraire infos produit depuis URL (OpenGraph) |

### Admin (`/api/admin`) — admin seulement
| Méthode | Endpoint | Description |
|---------|----------|-------------|
| GET  | `/stats` | Statistiques globales |
| GET  | `/users` | Tous les utilisateurs |
| POST | `/users` | Créer un utilisateur |
| PUT  | `/users/{id}` | Modifier un utilisateur |
| DELETE | `/users/{id}` | Supprimer un utilisateur |
| GET  | `/families` | Toutes les familles |
| POST | `/families` | Créer une famille |
| PUT  | `/families/{id}` | Modifier une famille |
| DELETE | `/families/{id}` | Supprimer une famille |

---

## Frontend — Pages et navigation

| Route | Composant | Description |
|-------|-----------|-------------|
| `/login` | `Login.tsx` | Connexion + migration MD5 |
| `/reset-password` | `ResetPassword.tsx` | Reset mot de passe via token |
| `/` | `Home.tsx` | Liste de tous les utilisateurs avec leurs cadeaux |
| `/my-list` | `MyList.tsx` | Gestion de sa propre liste |
| `/list/:userId` | `UserList.tsx` | Consulter et réserver les cadeaux de quelqu'un |
| `/cart` | `Cart.tsx` | Mes réservations en cours |
| `/profile` | `Profile.tsx` | Profil, avatar, notifications |
| `/admin` | `Admin.tsx` | Panel administration |
| `/help` | `Help.tsx` | Page d'aide |

### Structure frontend (`src/`)
```
src/
├── pages/              # Pages principales
├── components/
│   ├── admin/          # AdminDashboard, AdminUsers, AdminFamilies
│   ├── gifts/          # GiftFormDialog, GiftListItem, ImportDialog
│   ├── userlist/       # ReserveDialog, UserGiftListItem
│   └── profile/        # ProfileForm, PasswordChangeForm, AvatarUpload
├── contexts/
│   └── AuthContext.tsx # Contexte d'authentification global
├── hooks/
│   └── useGifts.ts     # Hook pour la gestion des cadeaux
├── services/
│   └── api.ts          # Toutes les fonctions d'appel API (Axios)
└── css/                # LESS CSS Modules par composant
```

### Gestion de l'auth côté client
- Token JWT stocké dans `localStorage` (`token` et `user`)
- Intercepteur Axios → redirection `/login` si 401 (sauf `LEGACY_PASSWORD`)
- `AuthContext` fournit l'état auth à toute l'app

---

## Fonctionnalités clés

### Cadeaux groupés
Un cadeau peut passer en mode "groupe" :
- Si quelqu'un réserve un cadeau déjà réservé, il devient un cadeau groupé
- La liste des participants est dans `GiftParticipation`
- Si tous se désistent, le cadeau repasse disponible

### Comptes enfants
- `is_children = true` → compte géré par les adultes de la même famille
- Les adultes peuvent voir et gérer la liste d'un enfant via `/manage-child/{childId}`

### Notifications email (avec debouncing 2 min)
- **Modification de liste** (`notify_list_edit`) : agrégation des changements pendant 2 min avant envoi
- **Réservation** (`notify_gift_taken`) : agrégation des actions pendant 2 min avant envoi
- Email désactivé par défaut (`Enabled: false`)

### Migration MD5 → BCrypt
- Les anciens mots de passe MD5 (32 chars hex) sont détectés à la connexion
- Retourne une erreur `LEGACY_PASSWORD` → le frontend propose d'envoyer un email de reset
- Endpoint dédié : `POST /api/auth/request-migration-reset`

### Extraction produit (OpenGraph)
- `POST /api/products/extract-info` avec `{ url: "..." }`
- Appel à l'API opengraph.io pour récupérer nom, prix, image
- Usage tracé dans la table `OpenGraphRequests`

---

## Configuration

### Variables d'environnement (production)

| Variable | Description |
|----------|-------------|
| `JWT_SECRET` | Clé secrète JWT (min 32 chars) |
| `JWT_ISSUER` | Issuer JWT |
| `JWT_AUDIENCE` | Audience JWT |
| `SMTP_HOST` | Serveur SMTP |
| `SMTP_PORT` | Port SMTP |
| `SMTP_USERNAME` | Identifiant SMTP |
| `SMTP_PASSWORD` | Mot de passe SMTP |
| `SMTP_FROM_EMAIL` | Email expéditeur |
| `SMTP_FROM_NAME` | Nom expéditeur |
| `SMTP_USE_SSL` | SSL activé (bool) |
| `APP_URL` | URL de l'application (pour les liens email) |
| `OPENGRAPH_API_KEY` | Clé API opengraph.io |

### appsettings.json — points importants
- `UseSqlite: true` → SQLite (dev), `false` → MySQL (prod)
- `Email.Enabled: false` → désactive l'envoi d'emails
- `Email.NotificationDelayMinutes: 2` → debounce notifications liste
- `Email.ReservationNotificationDelayMinutes: 2` → debounce notifications réservation
- Rate limiting : 60 req/min et 1000 req/h par IP

### Frontend
- Variable d'environnement Vite : `VITE_API_URL` (défaut: `http://localhost:5284/api`)

---

## Lancer le projet

### Développement

**Backend (SQLite, pas besoin de MySQL)**
```bash
cd backend/Nawel.Api
dotnet run
# API sur http://localhost:5284
# Swagger sur http://localhost:5284/swagger
```

**Frontend**
```bash
cd frontend/nawel-app
npm install
npm run dev
# Frontend sur http://localhost:5173
```

### Production (Docker)

```bash
# Copier et adapter les variables dans docker-compose.yml
docker compose up -d
# Frontend sur http://localhost:3000
# Backend sur http://localhost:5000
```

### Tests

```bash
# Backend
cd backend
dotnet test

# Frontend
cd frontend/nawel-app
npm test
npm run test:coverage
```

---

## Ajouter une migration EF Core

> **IMPORTANT** : Ne jamais lancer `dotnet ef migrations add` directement. Le projet utilise SQLite en dev et MySQL en prod — les migrations doivent être générées pour les deux en même temps.

Utiliser le script depuis le dossier `backend/` :

```powershell
cd backend
.\add-migration.ps1 -Name "NomDeLaMigration"
```

Le script effectue 6 étapes automatiquement :
1. Configure le factory pour SQLite et génère la migration
2. Sauvegarde dans `Nawel.Api/Migrations/_backup/SQLite/`
3. Configure le factory pour MySQL et génère la migration (en restaurant le bon snapshot MySQL)
4. Sauvegarde dans `Nawel.Api/Migrations/_backup/MySQL/`
5. Remet les migrations SQLite actives (pour le dev)
6. Restaure le `NawelDbContextFactory.cs` original

**Après génération :**
1. Vérifier les deux migrations dans `_backup/SQLite/` et `_backup/MySQL/`
2. Ajuster manuellement si besoin (types `NOW()`, `decimal`, etc.)
3. Tester en dev avec SQLite
4. Commiter **tout** y compris `_backup/`

En production, le déploiement Docker copie automatiquement les migrations MySQL (`UseSqlite: false`) et les applique au démarrage.

---

## Notes importantes

- L'utilisateur admin système a toujours l'ID `1` et ne peut pas être supprimé
- Les migrations EF Core s'exécutent automatiquement au démarrage
- Les avatars sont stockés dans `uploads/avatars/` (servi statiquement sur `/uploads`)
- Les listes de cadeaux sont créées automatiquement si elles n'existent pas
- En dev SQLite, des données de test sont seédées automatiquement (`DbSeeder`)
- Swagger disponible dans tous les environnements sur `/swagger`
