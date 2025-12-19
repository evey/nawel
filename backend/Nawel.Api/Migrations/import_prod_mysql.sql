-- =============================================================================
-- Script de Migration Production MySQL
-- =============================================================================
-- Ce script importe les données depuis les fichiers CSV exportés de phpMyAdmin
-- et met à jour les chemins des avatars pour le nouveau système.
--
-- Prérequis:
--   - Fichiers CSV dans /tmp/ : gifts_phpma.csv, gift_participation_phpma.csv
--   - Backup de la base effectué
--   - LOAD DATA LOCAL INFILE activé dans MySQL
--
-- Utilisation:
--   mysql -u nawel_user -p nawel < import_prod_mysql.sql
-- =============================================================================

-- Afficher les warnings
\W

-- =============================================================================
-- ÉTAPE 1: Créer les tables temporaires
-- =============================================================================

DROP TEMPORARY TABLE IF EXISTS temp_gifts_import;
DROP TEMPORARY TABLE IF EXISTS temp_participation_import;

CREATE TEMPORARY TABLE temp_gifts_import (
    id INT,
    list_id INT,
    name VARCHAR(255),
    description TEXT,
    image VARCHAR(255),
    link VARCHAR(500),
    cost VARCHAR(50),
    currency VARCHAR(10),
    available TINYINT,
    taken_by INT,
    comment TEXT,
    year INT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TEMPORARY TABLE temp_participation_import (
    id INT,
    gift_id INT,
    user_id INT,
    is_active TINYINT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

SELECT '✓ Tables temporaires créées' as status;

-- =============================================================================
-- ÉTAPE 2: Charger les fichiers CSV
-- =============================================================================

-- Charger les gifts
LOAD DATA LOCAL INFILE '/tmp/gifts_phpma.csv'
INTO TABLE temp_gifts_import
FIELDS TERMINATED BY ','
ENCLOSED BY '"'
LINES TERMINATED BY '\n'
IGNORE 1 ROWS
(id, list_id, name, description, image, link, cost, currency, available, @taken_by, comment, year)
SET taken_by = NULLIF(@taken_by, '');

SELECT CONCAT('✓ ', COUNT(*), ' gifts chargés depuis CSV') as status FROM temp_gifts_import;

-- Charger les participations
LOAD DATA LOCAL INFILE '/tmp/gift_participation_phpma.csv'
INTO TABLE temp_participation_import
FIELDS TERMINATED BY ','
ENCLOSED BY '"'
LINES TERMINATED BY '\n'
IGNORE 1 ROWS;

SELECT CONCAT('✓ ', COUNT(*), ' participations chargées depuis CSV') as status FROM temp_participation_import;

-- =============================================================================
-- ÉTAPE 3: Nettoyer les données existantes
-- =============================================================================

-- Compter les données avant suppression
SELECT COUNT(*) as 'Gifts avant suppression' FROM gifts;
SELECT COUNT(*) as 'Participations avant suppression' FROM gift_participation;

-- Désactiver temporairement les contraintes de clés étrangères
SET FOREIGN_KEY_CHECKS = 0;

-- Supprimer les données existantes
DELETE FROM gift_participation;
DELETE FROM gifts;

-- Réactiver les contraintes
SET FOREIGN_KEY_CHECKS = 1;

SELECT '✓ Données existantes supprimées' as status;

-- =============================================================================
-- ÉTAPE 4: Insérer les nouvelles données
-- =============================================================================

-- Insérer les gifts avec gestion des NULLs
INSERT INTO gifts (
    id, list_id, name, description, image, link,
    cost, currency, available, taken_by, is_group_gift,
    comment, year, created_at, updated_at
)
SELECT
    id,
    list_id,
    name,
    description,
    NULLIF(image, ''),
    NULLIF(link, ''),
    CASE WHEN cost = '' OR cost IS NULL THEN NULL ELSE CAST(cost AS DECIMAL(10,2)) END,
    NULLIF(currency, ''),
    available,
    taken_by,
    0, -- is_group_gift sera mis à jour après
    NULLIF(comment, ''),
    year,
    NOW(),
    NOW()
FROM temp_gifts_import
WHERE id IS NOT NULL AND id != 'id';

SELECT CONCAT('✓ ', ROW_COUNT(), ' gifts insérés') as status;

-- Insérer les participations
INSERT INTO gift_participation (id, gift_id, user_id, is_active, created_at)
SELECT id, gift_id, user_id, is_active, NOW()
FROM temp_participation_import
WHERE id IS NOT NULL AND id != 'id';

SELECT CONCAT('✓ ', ROW_COUNT(), ' participations insérées') as status;

-- =============================================================================
-- ÉTAPE 5: Mettre à jour les flags is_group_gift
-- =============================================================================

UPDATE gifts g
SET is_group_gift = 1
WHERE EXISTS (
    SELECT 1 FROM gift_participation gp
    WHERE gp.gift_id = g.id AND gp.is_active = 1
);

SELECT CONCAT('✓ ', ROW_COUNT(), ' gifts marqués comme cadeaux groupés') as status;

-- =============================================================================
-- ÉTAPE 6: Mettre à jour les chemins des avatars
-- =============================================================================

UPDATE user
SET avatar = CONCAT('uploads/avatars/', avatar)
WHERE avatar IS NOT NULL
  AND avatar != ''
  AND avatar NOT LIKE 'uploads/avatars/%'
  AND avatar != 'avatar.png';

SELECT CONCAT('✓ ', ROW_COUNT(), ' avatars mis à jour avec le bon chemin') as status;

-- =============================================================================
-- ÉTAPE 7: Mettre à jour l'auto-increment
-- =============================================================================

SET @max_gift_id = (SELECT MAX(id) FROM gifts);
SET @max_participation_id = (SELECT MAX(id) FROM gift_participation);

SET @sql_gift = CONCAT('ALTER TABLE gifts AUTO_INCREMENT = ', @max_gift_id + 1);
SET @sql_participation = CONCAT('ALTER TABLE gift_participation AUTO_INCREMENT = ', @max_participation_id + 1);

PREPARE stmt_gift FROM @sql_gift;
EXECUTE stmt_gift;
DEALLOCATE PREPARE stmt_gift;

PREPARE stmt_participation FROM @sql_participation;
EXECUTE stmt_participation;
DEALLOCATE PREPARE stmt_participation;

SELECT '✓ Auto-increment mis à jour' as status;

-- =============================================================================
-- ÉTAPE 8: Vérifications finales
-- =============================================================================

SELECT '========================================' as '';
SELECT 'VÉRIFICATIONS POST-MIGRATION' as '';
SELECT '========================================' as '';

SELECT COUNT(*) as 'Total Gifts' FROM gifts;

SELECT
    COUNT(*) as 'Total',
    SUM(CASE WHEN cost IS NOT NULL THEN 1 ELSE 0 END) as 'Avec coût',
    SUM(CASE WHEN cost IS NULL THEN 1 ELSE 0 END) as 'Sans coût'
FROM gifts;

SELECT COUNT(*) as 'Cadeaux groupés' FROM gifts WHERE is_group_gift = 1;

SELECT COUNT(*) as 'Total Participations' FROM gift_participation;

SELECT
    MIN(year) as 'Année la plus ancienne',
    MAX(year) as 'Année la plus récente'
FROM gifts;

SELECT
    COUNT(*) as 'Total utilisateurs',
    SUM(CASE WHEN avatar LIKE 'uploads/avatars/%' THEN 1 ELSE 0 END) as 'Avatars avec chemin',
    SUM(CASE WHEN avatar = 'avatar.png' THEN 1 ELSE 0 END) as 'Avatars par défaut',
    SUM(CASE WHEN LENGTH(pwd) = 32 THEN 1 ELSE 0 END) as 'Mots de passe MD5 (à migrer)'
FROM user;

-- Vérifier qu'aucun cadeau groupé n'a 0 participants (serait une erreur)
SELECT
    COUNT(*) as 'Cadeaux groupés sans participants (devrait être 0)'
FROM gifts g
WHERE g.is_group_gift = 1
AND NOT EXISTS (
    SELECT 1 FROM gift_participation gp
    WHERE gp.gift_id = g.id AND gp.is_active = 1
);

SELECT '========================================' as '';
SELECT '✓ MIGRATION TERMINÉE AVEC SUCCÈS!' as '';
SELECT '========================================' as '';
SELECT 'N\'oubliez pas de:' as '';
SELECT '1. Copier les fichiers avatar dans uploads/avatars/' as '';
SELECT '2. Vérifier les permissions (755 pour dossier, 644 pour fichiers)' as '';
SELECT '3. Redémarrer l\'application' as '';
SELECT '4. Tester la connexion et l\'affichage des gifts' as '';
