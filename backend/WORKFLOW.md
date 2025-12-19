# Workflow de développement Nawel

## Cycle de développement complet

### 1. Développer une nouvelle feature

```powershell
# En local (dev SQLite)
cd D:\Perso\nawel\backend

# Modifier les models/services...
# Par exemple, ajouter une propriété à User.cs
```

### 2. Créer la migration

```powershell
# Générer automatiquement SQLite ET MySQL
.\add-migration.ps1 -Name "AddUserBio"

# ✅ Le script génère :
#   - Migrations/_backup/SQLite/xxx_AddUserBio.cs
#   - Migrations/_backup/MySQL/xxx_AddUserBio.cs
#   - Active SQLite par défaut
```

### 3. Vérifier les migrations générées

```powershell
# Comparer les deux versions si besoin
code --diff Migrations\_backup\SQLite\xxx_AddUserBio.cs `
             Migrations\_backup\MySQL\xxx_AddUserBio.cs

# ⚠️ Vérifier manuellement :
# - Fonctions SQL incompatibles (NOW() vs datetime('now'))
# - Types de données (decimal vs TEXT)
# - Syntaxe spécifique
```

### 4. Tester en dev

```powershell
# L'API utilise automatiquement SQLite
cd Nawel.Api
dotnet run

# Tester la feature
# Vérifier que les migrations s'appliquent correctement
```

### 5. Commit et push

```bash
# Commiter TOUT (y compris les backups)
git add .
git commit -m "feat: add user bio field"
git push origin master
```

### 6. Déploiement production

```bash
# Sur le serveur de production
ssh user@nawel.nironi.com
cd /path/to/infrastructure

# Déployer (bascule automatiquement vers MySQL)
./deploy.sh

# Ou avec rebuild
./deploy.sh --rebuild

# Le script :
# 1. Pull les modifications
# 2. ✅ BASCULE AUTOMATIQUEMENT vers MySQL
# 3. Build et démarre les containers
# 4. Vérifie les migrations
```

### 7. Après le déploiement (optionnel)

Si tu veux continuer le dev en local après avoir déployé :

```powershell
# Revenir sur SQLite en local (si nécessaire)
cd D:\Perso\nawel\backend
.\swap-migrations.ps1 -Provider SQLite
```

## Commandes utiles

### Voir les migrations actives

```powershell
# Lister les migrations actives
ls Nawel.Api\Migrations\*.cs

# Lister les backups
ls Nawel.Api\Migrations\_backup\SQLite\
ls Nawel.Api\Migrations\_backup\MySQL\
```

### Rollback d'une migration

```powershell
# 1. Supprimer la dernière migration
cd Nawel.Api
dotnet ef migrations remove

# 2. Supprimer des deux backups
rm Migrations\_backup\SQLite\*LastMigrationName*.cs
rm Migrations\_backup\MySQL\*LastMigrationName*.cs

# 3. Commit et push
git add .
git commit -m "revert: remove LastMigrationName"
git push
```

### Vérifier l'état des migrations en prod

```bash
# SSH sur le serveur
ssh user@nawel.nironi.com

# Voir les logs du backend
docker logs nawel-backend -f

# Vérifier les migrations appliquées en base
docker exec -it menus-mysql mysql -u root -p
> USE nawel;
> SELECT * FROM __EFMigrationsHistory;
```

## Checklist avant chaque déploiement

- [ ] Toutes les migrations sont commitées dans `_backup/SQLite/` ET `_backup/MySQL/`
- [ ] Les migrations ont été testées en dev (SQLite)
- [ ] Les migrations MySQL ne contiennent pas de syntaxe SQLite-only
- [ ] Les changements de schema sont rétrocompatibles (si possible)
- [ ] Backup de la base prod effectué (si changements critiques)

## Cas d'erreur fréquents

### "Migrations MySQL introuvables"

```bash
# Erreur lors du deploy.sh
❌ Migrations MySQL introuvables dans Nawel.Api/Migrations/_backup/MySQL/

# Solution :
# 1. En local, générer les migrations MySQL
cd D:\Perso\nawel\backend
.\add-migration.ps1 -Name "MyMigration"

# 2. Commit et push
git add .
git commit -m "migrations: add MySQL migrations"
git push

# 3. Redéployer
./deploy.sh
```

### "Migration déjà appliquée en base"

```bash
# Erreur : Migration already applied

# Solution : Supprimer la migration de __EFMigrationsHistory
docker exec -it menus-mysql mysql -u root -p
> USE nawel;
> DELETE FROM __EFMigrationsHistory WHERE MigrationId = 'xxx_MigrationName';
> exit

# Redémarrer les containers
docker-compose -f docker-compose.production.yml restart nawel-backend
```

### "Conflit de types SQLite/MySQL"

```
# Exemple : decimal en MySQL → TEXT en SQLite

# Solution : Éditer manuellement la migration SQLite
code Migrations\_backup\SQLite\xxx_Migration.cs

# Remplacer :
decimal(10,2)  →  TEXT

# Puis tester en dev
dotnet run
```

## Architecture des migrations

```
backend/
├── Nawel.Api/
│   └── Migrations/
│       ├── _backup/           # ✅ Versionnés dans git
│       │   ├── SQLite/        # Migrations pour dev
│       │   └── MySQL/         # Migrations pour prod
│       ├── .gitignore         # Ignore les actives
│       └── (actives)          # ❌ PAS versionnées
├── add-migration.ps1          # Créer une migration
├── swap-migrations.ps1        # Basculer manuellement
├── MIGRATIONS.md              # Doc technique
└── WORKFLOW.md               # Ce guide (dev workflow)
```

## En résumé

1. **Développer** : Travailler normalement avec SQLite
2. **Migrer** : `.\add-migration.ps1` génère les deux versions
3. **Vérifier** : Comparer SQLite vs MySQL, tester en dev
4. **Commit** : Tout commit (y compris `_backup/`)
5. **Déployer** : `./deploy.sh` bascule automatiquement vers MySQL
6. **Monitorer** : Vérifier les logs et l'état des migrations

C'est tout ! Le système gère le reste automatiquement. 🚀
