-- =============================================================================
-- Script de Migration Production MySQL depuis dump SQL d'origine
-- =============================================================================
-- Ce script importe les données depuis le dump SQL MySQL d'origine
-- et met à jour les chemins des avatars pour le nouveau système.
--
-- Prérequis:
--   - Base de données MySQL vide (ou données de test à écraser)
--   - Dump SQL d'origine importé au préalable
--   - Backup de la base effectué
--
-- Utilisation:
--   1. Importer le dump d'origine:
--      mysql -u root -p nawel < /path/to/nironico_nawel_update.sql
--   2. Exécuter ce script:
--      mysql -u root -p nawel < import_prod_from_dump.sql
-- =============================================================================

-- Afficher les warnings
\W

-- =============================================================================
-- ÉTAPE 1: Vérification des données importées
-- =============================================================================

SELECT '========================================' as '';
SELECT 'VÉRIFICATION DES DONNÉES IMPORTÉES' as '';
SELECT '========================================' as '';

SELECT COUNT(*) as 'Familles importées' FROM family;
SELECT COUNT(*) as 'Utilisateurs importés' FROM user;
SELECT COUNT(*) as 'Listes importées' FROM lists;
SELECT COUNT(*) as 'Cadeaux importés' FROM gifts;
SELECT COUNT(*) as 'Participations importées' FROM gift_participation;

-- =============================================================================
-- ÉTAPE 2: Mettre à jour les chemins des avatars
-- =============================================================================

SELECT '========================================' as '';
SELECT 'MISE À JOUR DES CHEMINS AVATARS' as '';
SELECT '========================================' as '';

-- Sauvegarder les anciens chemins pour référence
SELECT 'Chemins avatars AVANT mise à jour:' as '';
SELECT DISTINCT avatar FROM user WHERE avatar IS NOT NULL AND avatar != '';

-- Mettre à jour les chemins (ajouter uploads/avatars/ devant)
UPDATE user
SET avatar = CONCAT('uploads/avatars/', avatar)
WHERE avatar IS NOT NULL
  AND avatar != ''
  AND avatar NOT LIKE 'uploads/avatars/%'
  AND avatar != 'avatar.png';

SELECT CONCAT('✓ ', ROW_COUNT(), ' avatars mis à jour avec le bon chemin') as status;

-- Vérifier les nouveaux chemins
SELECT 'Chemins avatars APRÈS mise à jour:' as '';
SELECT DISTINCT avatar FROM user WHERE avatar IS NOT NULL AND avatar != '';

-- =============================================================================
-- ÉTAPE 3: Mettre à jour les flags is_group_gift (si nécessaire)
-- =============================================================================

SELECT '========================================' as '';
SELECT 'MISE À JOUR DES CADEAUX GROUPÉS' as '';
SELECT '========================================' as '';

-- Compter les cadeaux marqués comme groupés
SELECT COUNT(*) as 'Cadeaux groupés AVANT' FROM gifts WHERE is_group_gift = 1;

-- S'assurer que tous les cadeaux avec participations sont marqués comme groupés
UPDATE gifts g
SET is_group_gift = 1
WHERE EXISTS (
    SELECT 1 FROM gift_participation gp
    WHERE gp.gift_id = g.id AND gp.is_active = 1
)
AND is_group_gift = 0;

SELECT CONCAT('✓ ', ROW_COUNT(), ' cadeaux mis à jour comme groupés') as status;

-- Compter après
SELECT COUNT(*) as 'Cadeaux groupés APRÈS' FROM gifts WHERE is_group_gift = 1;

-- =============================================================================
-- ÉTAPE 4: Ajouter les champs manquants si nécessaire (created_at, updated_at)
-- =============================================================================

SELECT '========================================' as '';
SELECT 'MISE À JOUR DES TIMESTAMPS' as '';
SELECT '========================================' as '';

-- Mettre à jour les timestamps NULL (normalement déjà gérés par les migrations EF)
UPDATE family SET created_at = NOW() WHERE created_at IS NULL;
UPDATE user SET created_at = NOW() WHERE created_at IS NULL;
UPDATE user SET updated_at = NOW() WHERE updated_at IS NULL;
UPDATE lists SET created_at = NOW() WHERE created_at IS NULL;
UPDATE lists SET updated_at = NOW() WHERE updated_at IS NULL;
UPDATE gifts SET created_at = NOW() WHERE created_at IS NULL;
UPDATE gifts SET updated_at = NOW() WHERE updated_at IS NULL;
UPDATE gift_participation SET created_at = NOW() WHERE created_at IS NULL;

SELECT '✓ Timestamps mis à jour' as status;

-- =============================================================================
-- ÉTAPE 5: Vérifications finales
-- =============================================================================

SELECT '========================================' as '';
SELECT 'VÉRIFICATIONS POST-MIGRATION' as '';
SELECT '========================================' as '';

-- Familles
SELECT COUNT(*) as 'Total Familles' FROM family;

-- Utilisateurs
SELECT
    COUNT(*) as 'Total Utilisateurs',
    SUM(CASE WHEN is_admin = 1 THEN 1 ELSE 0 END) as 'Admins',
    SUM(CASE WHEN isChildren = 1 THEN 1 ELSE 0 END) as 'Enfants',
    SUM(CASE WHEN LENGTH(pwd) = 32 THEN 1 ELSE 0 END) as 'Mots de passe MD5 (à migrer)'
FROM user;

-- Avatars
SELECT
    COUNT(*) as 'Total utilisateurs avec avatar',
    SUM(CASE WHEN avatar LIKE 'uploads/avatars/%' THEN 1 ELSE 0 END) as 'Avatars avec chemin correct',
    SUM(CASE WHEN avatar = 'avatar.png' THEN 1 ELSE 0 END) as 'Avatars par défaut'
FROM user
WHERE avatar IS NOT NULL AND avatar != '';

-- Listes
SELECT COUNT(*) as 'Total Listes' FROM lists;

-- Cadeaux
SELECT
    COUNT(*) as 'Total Cadeaux',
    SUM(CASE WHEN cost IS NOT NULL THEN 1 ELSE 0 END) as 'Avec coût',
    SUM(CASE WHEN cost IS NULL THEN 1 ELSE 0 END) as 'Sans coût',
    SUM(CASE WHEN is_group_gift = 1 THEN 1 ELSE 0 END) as 'Cadeaux groupés',
    SUM(CASE WHEN taken_by IS NOT NULL THEN 1 ELSE 0 END) as 'Réservés'
FROM gifts;

-- Années des cadeaux
SELECT
    MIN(year) as 'Année la plus ancienne',
    MAX(year) as 'Année la plus récente',
    COUNT(DISTINCT year) as 'Nombre d\'années différentes'
FROM gifts;

-- Participations
SELECT
    COUNT(*) as 'Total Participations',
    SUM(CASE WHEN is_active = 1 THEN 1 ELSE 0 END) as 'Actives',
    SUM(CASE WHEN is_active = 0 THEN 1 ELSE 0 END) as 'Inactives'
FROM gift_participation;

-- Vérifier qu'aucun cadeau groupé n'a 0 participants (serait une erreur)
SELECT
    COUNT(*) as 'Cadeaux groupés sans participants (devrait être 0)'
FROM gifts g
WHERE g.is_group_gift = 1
AND NOT EXISTS (
    SELECT 1 FROM gift_participation gp
    WHERE gp.gift_id = g.id AND gp.is_active = 1
);

-- Vérifier l'intégrité référentielle
SELECT 'Vérification de l\'intégrité référentielle:' as '';

SELECT COUNT(*) as 'Users sans famille (devrait être 0)'
FROM user WHERE family_id NOT IN (SELECT id FROM family);

SELECT COUNT(*) as 'Listes sans utilisateur (devrait être 0)'
FROM lists WHERE user_id NOT IN (SELECT id FROM user);

SELECT COUNT(*) as 'Cadeaux sans liste (devrait être 0)'
FROM gifts WHERE list_id NOT IN (SELECT id FROM lists);

SELECT COUNT(*) as 'Participations avec cadeau inexistant (devrait être 0)'
FROM gift_participation WHERE gift_id NOT IN (SELECT id FROM gifts);

SELECT COUNT(*) as 'Participations avec utilisateur inexistant (devrait être 0)'
FROM gift_participation WHERE user_id NOT IN (SELECT id FROM user);

-- =============================================================================
-- RÉSUMÉ
-- =============================================================================

SELECT '========================================' as '';
SELECT '✓ MIGRATION TERMINÉE AVEC SUCCÈS!' as '';
SELECT '========================================' as '';
SELECT '' as '';
SELECT 'Prochaines étapes:' as '';
SELECT '1. Copier les fichiers avatar dans uploads/avatars/' as '';
SELECT '2. Vérifier les permissions (755 pour dossier, 644 pour fichiers)' as '';
SELECT '3. Redémarrer l\'application Docker' as '';
SELECT '4. Tester la connexion avec un utilisateur' as '';
SELECT '5. Les utilisateurs avec mot de passe MD5 devront réinitialiser leur mot de passe' as '';
SELECT '' as '';
