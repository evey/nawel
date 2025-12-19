# Gestion des migrations SQLite / MySQL

## Problème

L'application utilise **SQLite en développement** et **MySQL en production**. EF Core génère des migrations spécifiques à chaque provider, avec des différences :

- **Types de données** : `decimal` vs `TEXT`, `datetime` vs `INTEGER`
- **Fonctions SQL** : `NOW()` vs `datetime('now')`
- **Auto-increment** : `AUTO_INCREMENT` vs `AUTOINCREMENT`
- **Syntaxe** : incompatibilités diverses

## Solution

Utilisation de **deux ensembles de migrations** stockés dans des dossiers séparés avec des scripts PowerShell pour faciliter la gestion.

### Structure des dossiers

```
backend/
├── Nawel.Api/
│   └── Migrations/
│       ├── _backup/
│       │   ├── SQLite/           # Migrations SQLite (dev)
│       │   │   ├── 20251218_InitialCreate.cs
│       │   │   ├── 20251218_InitialCreate.Designer.cs
│       │   │   └── NawelDbContextModelSnapshot.cs
│       │   └── MySQL/            # Migrations MySQL (prod)
│       │       ├── 20251218_InitialCreate.cs
│       │       ├── 20251218_InitialCreate.Designer.cs
│       │       └── NawelDbContextModelSnapshot.cs
│       └── (migrations actives - selon l'environnement)
├── add-migration.ps1             # Créer une nouvelle migration
└── swap-migrations.ps1           # Basculer entre SQLite et MySQL
```

## Workflow

### 1. Créer une nouvelle migration

```powershell
# Génère automatiquement pour SQLite ET MySQL
.\add-migration.ps1 -Name "AddUserProfilePicture"
```

Ce script :
1. ✅ Génère la migration pour SQLite
2. ✅ Sauvegarde dans `_backup/SQLite/`
3. ✅ Génère la migration pour MySQL
4. ✅ Sauvegarde dans `_backup/MySQL/`
5. ✅ Active les migrations SQLite par défaut (dev)

**⚠️ Important** : Vérifier les deux migrations générées et ajuster manuellement si nécessaire :
- Remplacer `NOW()` par `datetime('now')` pour SQLite
- Vérifier les types de données incompatibles
- Adapter les fonctions SQL spécifiques

### 2. Développement (SQLite)

```powershell
# S'assurer que les migrations SQLite sont actives
.\swap-migrations.ps1 -Provider SQLite

# Lancer l'API
cd Nawel.Api
dotnet run
```

### 3. Déploiement Production (MySQL)

Le basculement vers les migrations MySQL est **automatique** lors du déploiement :

```bash
# Depuis le serveur de production
cd /path/to/infrastructure
./deploy.sh

# Ou avec rebuild
./deploy.sh --rebuild

# Ou uniquement Nawel
./deploy.sh --nawel-only
```

Le script `deploy.sh` :
1. ✅ Pull les dernières modifications
2. ✅ **Bascule automatiquement vers les migrations MySQL**
3. ✅ Arrête les containers
4. ✅ Rebuild (si demandé)
5. ✅ Démarre les containers avec MySQL
6. ✅ Vérifie l'application des migrations

**⚠️ Important** : Assurez-vous que les migrations MySQL sont commitées dans `Migrations/_backup/MySQL/` avant de déployer.

### 4. Basculement manuel (si nécessaire)

Si tu dois basculer manuellement entre les migrations (par exemple, après un déploiement pour revenir en dev) :

```powershell
# Basculer vers SQLite (dev)
.\swap-migrations.ps1 -Provider SQLite

# Basculer vers MySQL (test local MySQL)
.\swap-migrations.ps1 -Provider MySQL
```

Le script détecte automatiquement le provider actuel et sauvegarde avant de basculer.

## Avantages

✅ **Automatisation** : Un seul script génère les deux migrations
✅ **Séparation claire** : SQLite et MySQL complètement séparés
✅ **Pas de modification manuelle au déploiement** : Swap une fois, c'est tout
✅ **Backups sécurisés** : Toutes les versions sont conservées dans `_backup/`
✅ **Git-friendly** : Seuls les backups sont versionnés, pas les migrations actives

## Limitations

⚠️ Les migrations **doivent être vérifiées manuellement** après génération pour :
- Les fonctions SQL incompatibles (`NOW()`, `CONCAT()`, etc.)
- Les types de données spécifiques
- Les contraintes avec syntaxe différente

## Commandes utiles

```powershell
# Voir quelle migration est active
Get-ChildItem Nawel.Api\Migrations\*.cs | Select-Object Name

# Comparer SQLite vs MySQL
code --diff Nawel.Api\Migrations\_backup\SQLite\20251218_xxx.cs `
             Nawel.Api\Migrations\_backup\MySQL\20251218_xxx.cs

# Supprimer la dernière migration (avant add-migration)
.\swap-migrations.ps1 -Provider SQLite
cd Nawel.Api
dotnet ef migrations remove
```

## Alternative future

Si tu peux installer **Docker** ou **MySQL local** plus tard, la solution idéale serait :
- Utiliser MySQL partout (dev + prod)
- Un seul ensemble de migrations
- Zéro gestion de compatibilité

Avec Docker :
```yaml
# docker-compose.yml
services:
  mysql:
    image: mysql:8.0
    environment:
      MYSQL_ROOT_PASSWORD: dev_password
      MYSQL_DATABASE: nawel_dev
    ports:
      - "3306:3306"
```

Mais en attendant, la solution actuelle est **pérenne et fonctionnelle** pour des années de développement.
