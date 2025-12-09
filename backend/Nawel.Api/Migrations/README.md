# Migrations de Base de Données

Ce dossier contient les scripts de migration SQL pour la base de données Nawel.

## Comment appliquer les migrations

### SQLite (Développement)

```bash
# Naviguez vers le répertoire de l'API
cd backend/Nawel.Api

# Appliquez la migration
sqlite3 nawel.db < Migrations/003_add_is_admin_column.sql
```

### MySQL (Production)

```bash
# Connectez-vous à MySQL
mysql -u nawel_user -p nawel

# Exécutez le script
source /path/to/Migrations/003_add_is_admin_column.sql;
# OU copiez-collez le contenu du fichier
```

### Via l'application

Les migrations doivent être appliquées manuellement avant de déployer une nouvelle version de l'application.

## Liste des migrations

### 001_initial_schema.sql
- Création initiale du schéma de base de données
- Tables: user, family, gift_list, gift, gift_participation

### 002_add_opengraph_requests.sql
- Ajout de la table `opengraph_request` pour tracking
- Champs: user_id, url, success, error_message, created_at

### 003_add_is_admin_column.sql
- Ajout de la colonne `is_admin` à la table `user`
- Mise à jour de l'utilisateur ID=1 comme admin
- Index sur `is_admin` pour performance
- **IMPORTANT**: Cette migration est requise pour la nouvelle fonctionnalité d'authorization par rôles

### 004_force_md5_password_reset.sql
- Identification des comptes avec mots de passe MD5 legacy
- Force la réinitialisation des mots de passe MD5 pour raisons de sécurité
- **IMPORTANT**: À exécuter APRÈS le déploiement du code qui supprime le support MD5
- **ATTENTION**: Les utilisateurs avec MD5 ne pourront plus se connecter jusqu'à reset password

### 005_invalidate_plaintext_reset_tokens.sql ✅ **NOUVEAU**
- Invalide tous les tokens de reset password en clair existants
- Requis car les nouveaux tokens sont hashés avec SHA256
- **IMPORTANT**: À exécuter APRÈS le déploiement du code qui hash les tokens (Task 1.5)
- **Impact**: Les utilisateurs avec token actif devront en redemander un nouveau

### 006_migrate_from_old_database.sql ✅ **NOUVEAU** - Migration Complète
- Migre toutes les données de l'ancienne base (old/nironico_nawel.sql) vers le nouveau système
- Tables migrées: family, user, lists, gifts, gift_participation
- **IMPORTANT**: Utilisez plutôt les scripts automatisés pour faciliter la migration
- **Scripts disponibles**:
  - `migrate_old_to_new.sh` (Linux/Mac)
  - `migrate_old_to_new.ps1` (Windows PowerShell)
- **Documentation complète**: Voir `MIGRATION_GUIDE.md`
- **Caractéristiques**:
  - Préserve les mots de passe MD5 (migration automatique au premier login)
  - Détecte automatiquement les cadeaux groupés
  - Copie les fichiers avatar
  - Met à jour les séquences d'auto-incrémentation
  - Inclut des vérifications post-migration
- **Utilisation**:
  ```bash
  # Windows PowerShell
  cd backend\Nawel.Api
  .\Migrations\migrate_old_to_new.ps1

  # Linux/Mac
  cd backend/Nawel.Api
  chmod +x Migrations/migrate_old_to_new.sh
  ./Migrations/migrate_old_to_new.sh
  ```

## Vérification

Après avoir appliqué la migration 003, vérifiez :

```sql
-- SQLite
SELECT id, login, is_admin FROM user WHERE is_admin = 1;

-- MySQL
SELECT id, login, is_admin FROM user WHERE is_admin = TRUE;
```

Vous devriez voir au moins un utilisateur avec `is_admin = 1` (l'administrateur).

Après avoir appliqué la migration 004, vérifiez les comptes MD5 :

```sql
-- SQLite & MySQL
SELECT id, login, email, LENGTH(password) as pwd_length
FROM user
WHERE LENGTH(password) = 32 AND SUBSTR(password, 1, 2) != '$2';
```

Si des utilisateurs apparaissent, ils devront réinitialiser leur mot de passe via email avant de pouvoir se connecter.

Après avoir appliqué la migration 005, vérifiez qu'il n'y a plus de tokens :

```sql
-- SQLite & MySQL
SELECT COUNT(*) as remaining_tokens
FROM user
WHERE reset_token IS NOT NULL;
```

Le résultat devrait être `0`. Les nouveaux tokens seront générés avec SHA256 hashing.

## Rollback

Si vous devez annuler la migration 003 :

```sql
-- Supprimer l'index
DROP INDEX IF EXISTS idx_user_is_admin;

-- Supprimer la colonne (Attention: perte de données!)
ALTER TABLE user DROP COLUMN is_admin;
```

⚠️ **Note**: Le rollback supprimera définitivement les informations sur qui est admin. Sauvegardez votre base avant!

## Notes importantes

- **Toujours** sauvegarder la base avant d'appliquer une migration en production
- Tester les migrations en développement d'abord
- Les migrations doivent être appliquées dans l'ordre
- Ne modifiez jamais une migration déjà appliquée en production
