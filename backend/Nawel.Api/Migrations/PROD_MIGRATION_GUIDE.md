# Guide de Migration Production (MySQL)

Ce guide vous permet d'importer les données de l'ancienne base MySQL vers la nouvelle base MySQL en production.

## Vue d'Ensemble

**Prérequis**:
- Accès SSH au VPS
- Container Docker `shared-mysql` en cours d'exécution
- Fichier dump SQL d'origine: `old/nironico_nawel_update.sql`
- Fichiers avatar de l'ancienne application

**Durée estimée**: 15-20 minutes

**Ce qui sera importé**:
- ✅ Toutes les familles
- ✅ Tous les utilisateurs (avec mots de passe MD5 à migrer)
- ✅ Toutes les listes
- ✅ Tous les cadeaux (~3000+, années 2016-2025)
- ✅ Toutes les participations aux cadeaux groupés

---

## Méthode 1: Script Automatisé (Recommandé)

### Étape 1: Transférer les fichiers nécessaires

Depuis votre machine locale (Windows):

```powershell
# Transférer le dump SQL
scp D:\Perso\nawel\old\nironico_nawel_update.sql user@votre-vps.com:/tmp/

# Transférer le script de migration
scp D:\Perso\nawel\backend\Nawel.Api\Migrations\migrate_prod_from_dump.sh user@votre-vps.com:/tmp/
scp D:\Perso\nawel\backend\Nawel.Api\Migrations\import_prod_from_dump.sql user@votre-vps.com:/tmp/
```

### Étape 2: Exécuter le script

Sur le VPS:

```bash
cd /tmp
chmod +x migrate_prod_from_dump.sh
./migrate_prod_from_dump.sh
```

Le script va:
1. ✅ Créer un backup automatique
2. ✅ Vérifier que le container Docker existe
3. ✅ Copier le dump SQL dans le container
4. ✅ Importer toutes les données
5. ✅ Mettre à jour les chemins des avatars
6. ✅ Mettre à jour les flags des cadeaux groupés
7. ✅ Afficher toutes les vérifications

Suivez les instructions affichées à la fin du script pour copier les avatars et redémarrer l'application.

---

## Méthode 2: Manuelle (Si vous préférez)

### Étape 1: Sauvegarder la base actuelle

**TOUJOURS faire une sauvegarde avant toute migration!**

```bash
# Se connecter au VPS
ssh user@votre-vps.com

# Créer un backup avec timestamp (via Docker)
docker exec shared-mysql mysqldump -u root -p nawel > backup_nawel_$(date +%Y%m%d_%H%M%S).sql

# Vérifier que le backup existe
ls -lh backup_nawel_*.sql
```

**Garder ce fichier précieusement!** En cas de problème, vous pourrez restaurer avec:
```bash
docker exec -i shared-mysql mysql -u root -p nawel < backup_nawel_YYYYMMDD_HHMMSS.sql
```

---

## Étape 2: Transférer le dump SQL vers le VPS

Depuis votre machine locale (Windows):

```powershell
# Via SCP (depuis PowerShell)
scp D:\Perso\nawel\old\nironico_nawel_update.sql user@votre-vps.com:/tmp/
```

Ou utilisez WinSCP / FileZilla pour uploader le fichier dans `/tmp/`

---

## Étape 3: Copier le dump dans le container Docker

Sur le VPS:

```bash
# Copier le dump SQL dans le container
docker cp /tmp/nironico_nawel_update.sql shared-mysql:/tmp/

# Vérifier que le fichier est bien dans le container
docker exec shared-mysql ls -lh /tmp/nironico_nawel_update.sql
```

---

## Étape 4: Importer le dump SQL

```bash
# Se connecter au MySQL du container et importer
docker exec -i shared-mysql mysql -u root -p nawel < /tmp/nironico_nawel_update.sql

# Ou depuis l'intérieur du container
docker exec -it shared-mysql mysql -u root -p nawel
```

Puis dans le shell MySQL:
```sql
source /tmp/nironico_nawel_update.sql;
```

---

## Étape 5: Appliquer les ajustements (avatars, flags)

Transférer le script d'ajustements:

```bash
# Depuis votre machine locale
scp D:\Perso\nawel\backend\Nawel.Api\Migrations\import_prod_from_dump.sql user@votre-vps.com:/tmp/
```

Sur le VPS:

```bash
# Copier dans le container
docker cp /tmp/import_prod_from_dump.sql shared-mysql:/tmp/

# Exécuter le script
docker exec -it shared-mysql mysql -u root -p nawel
```

Dans MySQL:
```sql
source /tmp/import_prod_from_dump.sql;
```

Ce script va:
- ✅ Mettre à jour les chemins des avatars (`uploads/avatars/`)
- ✅ Mettre à jour les flags `is_group_gift`
- ✅ Mettre à jour les timestamps
- ✅ Afficher toutes les vérifications

---

## Étape 7: Vérification Post-Migration (si exécution manuelle)

Si vous avez exécuté le script `import_prod_from_dump.sql`, les vérifications sont déjà affichées. Sinon:

```bash
docker exec -it shared-mysql mysql -u root -p nawel
```

```sql
-- Vérifier toutes les tables
SELECT COUNT(*) as 'Familles' FROM family;
SELECT COUNT(*) as 'Utilisateurs' FROM user;
SELECT COUNT(*) as 'Listes' FROM lists;
SELECT COUNT(*) as 'Cadeaux' FROM gifts;
SELECT COUNT(*) as 'Participations' FROM gift_participation;

-- Vérifier les années
SELECT MIN(year) as oldest, MAX(year) as newest FROM gifts;

-- Vérifier les avatars
SELECT COUNT(*) as 'Avatars avec bon chemin'
FROM user
WHERE avatar LIKE 'uploads/avatars/%';
```

---

## Étape 8: Transférer les fichiers avatar

**Option A: Transférer depuis votre machine locale**

```powershell
# Depuis Windows PowerShell
scp -r D:\Perso\nawel\backend\Nawel.Api\uploads\avatars\* user@vps:/path/to/nawel-app/uploads/avatars/
```

**Option B: Récupérer depuis l'ancienne application**

Sur le VPS:

```bash
# Localiser l'ancienne application
cd /path/to/old-nawel-app

# Copier les avatars vers la nouvelle application
cp -r uploads/avatars/* /path/to/new-nawel-app/uploads/avatars/

# Vérifier les permissions
chmod 755 /path/to/new-nawel-app/uploads/avatars
chmod 644 /path/to/new-nawel-app/uploads/avatars/*

# Vérifier
ls -la /path/to/new-nawel-app/uploads/avatars/
```

---

## Étape 9: Redémarrer l'application

```bash
# Si l'API Nawel est un container Docker standalone
docker restart nawel-api

# Si elle fait partie d'un docker-compose
cd /path/to/infrastructure
docker-compose restart nawel-api

# Vérifier que le container a bien redémarré
docker ps | grep nawel

# Vérifier les logs pour détecter d'éventuelles erreurs
docker logs -f nawel-api
```

---

## Étape 10: Tester l'application

1. **Accéder à l'application** via le navigateur (votre domaine de production)
2. **Tester la connexion**:
   - Les utilisateurs avec mot de passe MD5 (tous les anciens utilisateurs) devront réinitialiser leur mot de passe
   - Ils verront un message de sécurité et recevront un email de réinitialisation
3. **Vérifier les avatars** sur la page d'accueil
4. **Consulter les listes** des différentes années (2016-2025)
5. **Vérifier les cadeaux groupés** avec leurs participants
6. **Tester la réservation** d'un cadeau

---

## Dépannage

### Restaurer en cas de problème

```bash
# Arrêter l'application
docker restart nawel-api

# Restaurer le backup
docker exec -i shared-mysql mysql -u root -p nawel < backup_nawel_YYYYMMDD_HHMMSS.sql

# Redémarrer l'application
docker restart nawel-api
```

### Problème: Avatars ne s'affichent pas

1. Vérifier que les fichiers sont bien dans `uploads/avatars/`
2. Vérifier les chemins en base:
   ```sql
   SELECT DISTINCT avatar FROM user WHERE avatar IS NOT NULL;
   ```
3. Vérifier les permissions:
   ```bash
   ls -la /path/to/nawel-app/uploads/avatars/
   chmod 755 /path/to/nawel-app/uploads/avatars
   chmod 644 /path/to/nawel-app/uploads/avatars/*
   ```

### Problème: Impossible de se connecter

C'est normal pour les anciens utilisateurs (mot de passe MD5):
1. Cliquer sur "Mot de passe oublié"
2. Recevoir l'email de réinitialisation
3. Définir un nouveau mot de passe sécurisé (BCrypt)

### Problème: Erreur lors de l'import du dump

- Vérifier que le fichier SQL est complet et non corrompu
- Vérifier les logs MySQL: `docker logs shared-mysql`
- S'assurer que la base est vide ou que les données existantes peuvent être écrasées

---

## Checklist Finale

- [ ] Backup de la base de données effectué
- [ ] Dump SQL transféré sur le VPS
- [ ] Dump importé dans MySQL (toutes les tables)
- [ ] Script d'ajustements exécuté (avatars, flags)
- [ ] Vérifications SQL OK
- [ ] Fichiers avatar copiés dans `uploads/avatars/`
- [ ] Permissions fichiers vérifiées (755/644)
- [ ] Application redémarrée
- [ ] Tests fonctionnels OK
- [ ] Utilisateurs peuvent réinitialiser leur mot de passe MD5

---

## Résumé - Migration Rapide

**Méthode automatisée (recommandée)**:
```bash
# 1. Transférer les fichiers
scp nironico_nawel_update.sql migrate_prod_from_dump.sh import_prod_from_dump.sql user@vps:/tmp/

# 2. Sur le VPS
cd /tmp
chmod +x migrate_prod_from_dump.sh
./migrate_prod_from_dump.sh

# 3. Copier les avatars
cp /old-app/uploads/avatars/* /new-app/uploads/avatars/

# 4. Redémarrer
docker restart nawel-api
```

**Méthode manuelle**:
```bash
# 1. Backup
docker exec shared-mysql mysqldump -u root -p nawel > backup.sql

# 2. Import dump
docker cp /tmp/nironico_nawel_update.sql shared-mysql:/tmp/
docker exec -it shared-mysql mysql -u root -p nawel
> source /tmp/nironico_nawel_update.sql;

# 3. Ajustements
docker cp /tmp/import_prod_from_dump.sql shared-mysql:/tmp/
docker exec -it shared-mysql mysql -u root -p nawel
> source /tmp/import_prod_from_dump.sql;

# 4. Avatars + redémarrage
cp /old-app/uploads/avatars/* /new-app/uploads/avatars/
docker restart nawel-api
```

---

**Bonne migration en production!**
