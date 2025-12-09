# Guide de Migration - Ancienne Base vers Nouveau Système

Ce guide explique comment migrer les données de l'ancienne base de données (`old/nironico_nawel.sql`) vers le nouveau système Nawel.

## 📋 Vue d'Ensemble

**Durée estimée**: 15-30 minutes
**Difficulté**: Moyenne
**Impact**: Migration complète des données (familles, utilisateurs, listes, cadeaux)

## ⚠️ Prérequis

1. **Sauvegarde obligatoire**
   ```bash
   # SQLite
   cp backend/Nawel.Api/nawel.db backend/Nawel.Api/nawel.db.backup

   # MySQL
   mysqldump -u nawel_user -p nawel > backup_$(date +%Y%m%d_%H%M%S).sql
   ```

2. **Fichiers requis**
   - `old/nironico_nawel.sql` - Base de données source
   - `backend/Nawel.Api/Migrations/006_migrate_from_old_database.sql` - Script de migration

3. **Accès**
   - Accès en écriture à la base de données
   - Droits d'administration sur la base

## 🔄 Méthode 1: Migration SQLite (Développement)

### Étape 1: Préparer une base temporaire

```bash
# Naviguer vers le dossier API
cd backend/Nawel.Api

# Créer une base temporaire avec les anciennes données
sqlite3 nawel_old.db < ../../old/nironico_nawel.sql
```

### Étape 2: Exporter les données de l'ancienne base

```bash
# Créer un script d'export temporaire
cat > export_old_data.sql << 'EOF'
.headers on
.mode insert family
SELECT * FROM family;
.mode insert user
SELECT * FROM user;
.mode insert lists
SELECT * FROM lists;
.mode insert gifts
SELECT * FROM gifts;
.mode insert gift_participation
SELECT * FROM gift_participation;
EOF

# Exporter
sqlite3 nawel_old.db < export_old_data.sql > old_data_dump.sql
```

### Étape 3: Adapter le script d'export

Le script généré contient des INSERT INTO pour l'ancienne structure. Nous devons le convertir.

**Option A - Utiliser le script Shell automatisé** (Recommandé):

```bash
# Donner les permissions d'exécution
chmod +x Migrations/migrate_old_to_new.sh

# Exécuter la migration
./Migrations/migrate_old_to_new.sh
```

**Option B - Manuelle avec SQLite ATTACH**:

```bash
# Créer un script de migration
cat > migrate.sql << 'EOF'
-- Attacher l'ancienne base de données
ATTACH DATABASE 'nawel_old.db' AS old_db;

-- Migrer les familles
INSERT INTO family (id, name, created_at)
SELECT id, name, CURRENT_TIMESTAMP
FROM old_db.family
WHERE id NOT IN (SELECT id FROM family);

-- Migrer les utilisateurs
INSERT INTO user (
    id, login, pwd, email, first_name, last_name, avatar, pseudo,
    notify_list_edit, notify_gift_taken, display_popup,
    reset_token, token_expiry, isChildren, family_id, is_admin,
    created_at, updated_at
)
SELECT
    id, login, pwd, email, first_name, last_name,
    CASE WHEN avatar = 'default.png' THEN 'avatar.png' ELSE avatar END,
    pseudo, notify_list_edit, notify_gift_taken, display_popup,
    reset_token, token_expiry, isChildren, family_id,
    CASE WHEN id = 1 THEN 1 ELSE 0 END,
    CURRENT_TIMESTAMP, CURRENT_TIMESTAMP
FROM old_db.user
WHERE id NOT IN (SELECT id FROM user);

-- Migrer les listes
INSERT INTO lists (id, name, user_id, created_at, updated_at)
SELECT id, name, user_id, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP
FROM old_db.lists
WHERE id NOT IN (SELECT id FROM lists);

-- Migrer les cadeaux avec détection des cadeaux groupés
INSERT INTO gifts (
    id, list_id, name, description, image, link, cost, currency,
    available, taken_by, is_group_gift, comment, year,
    created_at, updated_at
)
SELECT
    g.id, g.list_id, g.name, g.description, g.image, g.link,
    g.cost, g.currency, g.available, g.taken_by,
    CASE WHEN EXISTS (
        SELECT 1 FROM old_db.gift_participation gp
        WHERE gp.gift_id = g.id AND gp.is_active = 1
    ) THEN 1 ELSE 0 END,
    g.comment, g.year,
    CURRENT_TIMESTAMP, CURRENT_TIMESTAMP
FROM old_db.gifts g
WHERE g.id NOT IN (SELECT id FROM gifts);

-- Migrer les participations
INSERT INTO gift_participation (id, gift_id, user_id, is_active, created_at)
SELECT id, gift_id, user_id, is_active, CURRENT_TIMESTAMP
FROM old_db.gift_participation
WHERE id NOT IN (SELECT id FROM gift_participation);

-- Mettre à jour les séquences d'auto-incrémentation
UPDATE sqlite_sequence SET seq = (SELECT MAX(id) FROM family) WHERE name = 'family';
UPDATE sqlite_sequence SET seq = (SELECT MAX(id) FROM user) WHERE name = 'user';
UPDATE sqlite_sequence SET seq = (SELECT MAX(id) FROM lists) WHERE name = 'lists';
UPDATE sqlite_sequence SET seq = (SELECT MAX(id) FROM gifts) WHERE name = 'gifts';
UPDATE sqlite_sequence SET seq = (SELECT MAX(id) FROM gift_participation) WHERE name = 'gift_participation';

-- Détacher la base
DETACH DATABASE old_db;
EOF

# Exécuter la migration
sqlite3 nawel.db < migrate.sql
```

## 🔄 Méthode 2: Migration MySQL (Production)

### Étape 1: Créer une base temporaire

```sql
-- Se connecter à MySQL
mysql -u nawel_user -p

-- Créer une base temporaire
CREATE DATABASE nawel_old CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

-- Quitter MySQL
EXIT;
```

### Étape 2: Importer les anciennes données

```bash
# Importer le dump SQL dans la base temporaire
mysql -u nawel_user -p nawel_old < old/nironico_nawel.sql
```

### Étape 3: Migrer les données

```bash
# Créer un script de migration MySQL
cat > migrate_mysql.sql << 'EOF'
USE nawel;

-- Migrer les familles
INSERT INTO family (id, name, created_at)
SELECT id, name, NOW()
FROM nawel_old.family
WHERE id NOT IN (SELECT id FROM nawel.family);

-- Migrer les utilisateurs
INSERT INTO user (
    id, login, pwd, email, first_name, last_name, avatar, pseudo,
    notify_list_edit, notify_gift_taken, display_popup,
    reset_token, token_expiry, isChildren, family_id, is_admin,
    created_at, updated_at
)
SELECT
    id, login, pwd, email, first_name, last_name,
    CASE WHEN avatar = 'default.png' THEN 'avatar.png' ELSE avatar END,
    pseudo, notify_list_edit, notify_gift_taken, display_popup,
    reset_token, token_expiry, isChildren, family_id,
    IF(id = 1, TRUE, FALSE),
    NOW(), NOW()
FROM nawel_old.user
WHERE id NOT IN (SELECT id FROM nawel.user);

-- Migrer les listes
INSERT INTO lists (id, name, user_id, created_at, updated_at)
SELECT id, name, user_id, NOW(), NOW()
FROM nawel_old.lists
WHERE id NOT IN (SELECT id FROM nawel.lists);

-- Migrer les cadeaux
INSERT INTO gifts (
    id, list_id, name, description, image, link, cost, currency,
    available, taken_by, is_group_gift, comment, year,
    created_at, updated_at
)
SELECT
    g.id, g.list_id, g.name, g.description, g.image, g.link,
    g.cost, g.currency, g.available, g.taken_by,
    EXISTS (
        SELECT 1 FROM nawel_old.gift_participation gp
        WHERE gp.gift_id = g.id AND gp.is_active = 1
    ),
    g.comment, g.year,
    NOW(), NOW()
FROM nawel_old.gifts g
WHERE g.id NOT IN (SELECT id FROM nawel.gifts);

-- Migrer les participations
INSERT INTO gift_participation (id, gift_id, user_id, is_active, created_at)
SELECT id, gift_id, user_id, is_active, NOW()
FROM nawel_old.gift_participation
WHERE id NOT IN (SELECT id FROM nawel.gift_participation);

-- Mettre à jour l'auto-incrémentation
ALTER TABLE family AUTO_INCREMENT = (SELECT MAX(id) + 1 FROM family);
ALTER TABLE user AUTO_INCREMENT = (SELECT MAX(id) + 1 FROM user);
ALTER TABLE lists AUTO_INCREMENT = (SELECT MAX(id) + 1 FROM lists);
ALTER TABLE gifts AUTO_INCREMENT = (SELECT MAX(id) + 1 FROM gifts);
ALTER TABLE gift_participation AUTO_INCREMENT = (SELECT MAX(id) + 1 FROM gift_participation);
EOF

# Exécuter la migration
mysql -u nawel_user -p < migrate_mysql.sql
```

### Étape 4: Nettoyer

```sql
-- Se connecter à MySQL
mysql -u nawel_user -p

-- Supprimer la base temporaire
DROP DATABASE nawel_old;

-- Quitter
EXIT;
```

## ✅ Vérification Post-Migration

Après la migration, exécutez ces requêtes pour vérifier:

```sql
-- Vérifier le nombre de familles
SELECT COUNT(*) as families FROM family;

-- Vérifier les utilisateurs et leurs mots de passe
SELECT
    COUNT(*) as total_users,
    SUM(CASE WHEN LENGTH(pwd) = 32 THEN 1 ELSE 0 END) as md5_users,
    SUM(CASE WHEN is_admin = 1 THEN 1 ELSE 0 END) as admin_users
FROM user;

-- Vérifier les listes
SELECT COUNT(*) as lists FROM lists;

-- Vérifier les cadeaux
SELECT
    COUNT(*) as total_gifts,
    SUM(CASE WHEN is_group_gift = 1 THEN 1 ELSE 0 END) as group_gifts,
    MIN(year) as oldest_year,
    MAX(year) as newest_year
FROM gifts;

-- Vérifier les participations
SELECT COUNT(*) as participations FROM gift_participation;

-- Vérifier que tous les cadeaux groupés ont des participations
SELECT
    g.id,
    g.name,
    g.is_group_gift,
    COUNT(gp.id) as participant_count
FROM gifts g
LEFT JOIN gift_participation gp ON g.id = gp.gift_id
WHERE g.is_group_gift = 1
GROUP BY g.id, g.name, g.is_group_gift
HAVING participant_count = 0;  -- Should return no results
```

## 📦 Migration des Fichiers Avatar

Les fichiers avatar doivent être copiés manuellement:

```bash
# Créer le dossier de destination s'il n'existe pas
mkdir -p backend/Nawel.Api/uploads/avatars

# Copier les avatars (si l'ancien dossier existe)
if [ -d "old/uploads/avatars" ]; then
    cp -r old/uploads/avatars/* backend/Nawel.Api/uploads/avatars/
    echo "✓ Avatars copiés"
else
    echo "⚠ Dossier old/uploads/avatars introuvable"
fi
```

## 🔐 Gestion des Mots de Passe MD5

**Important**: Tous les mots de passe MD5 sont migrés tels quels.

### Comportement attendu:

1. **Première connexion** d'un utilisateur avec mot de passe MD5:
   - Le système détecte le format MD5 (32 caractères)
   - Retourne une erreur spécifique: `LEGACY_PASSWORD`
   - Le frontend affiche une interface de réinitialisation

2. **Réinitialisation**:
   - L'utilisateur clique sur "Recevoir un email"
   - Un email avec lien de réinitialisation est envoyé
   - Le mot de passe est réinitialisé en BCrypt sécurisé

3. **Connexions suivantes**:
   - Fonctionnent normalement avec le nouveau mot de passe BCrypt

### Pour plus de détails:
Voir `MIGRATION_MD5_PLAN.md` pour le flow complet de migration des mots de passe.

## 🧪 Tests Recommandés

Après migration, testez les scénarios suivants:

### 1. Authentification
- [ ] Login avec un utilisateur MD5 → Doit déclencher le flow de reset
- [ ] Reset du mot de passe via email
- [ ] Login avec le nouveau mot de passe → Doit fonctionner

### 2. Données
- [ ] Visualiser les listes par année (2016-2025)
- [ ] Vérifier les avatars des utilisateurs
- [ ] Consulter les cadeaux réservés (taken_by)
- [ ] Voir les cadeaux groupés avec leurs participants

### 3. Fonctionnalités
- [ ] Admin peut accéder au panel admin (user ID 1)
- [ ] Les familles sont correctement liées
- [ ] Les notifications (notify_gift_taken, etc.) sont préservées
- [ ] Les enfants sont marqués correctement (isChildren)

## ⚠️ Problèmes Connus et Solutions

### Problème: Conflit d'ID

**Symptôme**: Erreur "PRIMARY KEY constraint failed"

**Solution**:
```sql
-- Trouver le prochain ID disponible
SELECT MAX(id) + 1 FROM family;  -- Pour chaque table

-- Mettre à jour la séquence
UPDATE sqlite_sequence SET seq = [valeur] WHERE name = 'family';
```

### Problème: Avatars manquants

**Symptôme**: Avatars n'apparaissent pas

**Solution**:
1. Vérifier que les fichiers existent dans `uploads/avatars/`
2. Vérifier les permissions: `chmod 755 uploads/avatars/*`
3. Vérifier que le nom du fichier correspond à celui en base

### Problème: Utilisateurs ne peuvent pas se connecter

**Symptôme**: Tous les utilisateurs reçoivent "Invalid credentials"

**Solution**:
- C'est normal pour les utilisateurs MD5
- Ils doivent suivre le processus de réinitialisation
- Vérifier que le service d'email est configuré (SMTP)

## 📊 Statistiques de Migration Attendues

D'après le dump SQL fourni, vous devriez obtenir:

- **Familles**: ~2-3 familles
- **Utilisateurs**: ~15 utilisateurs
- **Listes**: ~15 listes
- **Cadeaux**: ~3000+ cadeaux (sur plusieurs années 2016-2025)
- **Participations**: ~100+ participations aux cadeaux groupés

## 🎯 Checklist Finale

- [ ] Sauvegarde de la base actuelle effectuée
- [ ] Migration des familles réussie
- [ ] Migration des utilisateurs réussie
- [ ] Migration des listes réussie
- [ ] Migration des cadeaux réussie
- [ ] Migration des participations réussie
- [ ] Séquences d'auto-incrémentation mises à jour
- [ ] Fichiers avatar copiés
- [ ] Requêtes de vérification exécutées
- [ ] Tests d'authentification effectués
- [ ] Admin peut se connecter
- [ ] Email de réinitialisation MD5 fonctionnel

## 📞 Support

En cas de problème lors de la migration:
1. Restaurer la sauvegarde: `cp nawel.db.backup nawel.db`
2. Consulter les logs: `backend/Nawel.Api/logs/`
3. Vérifier les prérequis listés en début de guide
4. Revoir les étapes une par une

---

**Bonne migration! 🎄🎁**
