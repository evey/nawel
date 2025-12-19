-- =============================================================================
-- Script d'import des anciennes données vers la nouvelle structure
-- =============================================================================
-- Ce script importe les données de l'ancien dump SQL vers la nouvelle base
-- créée par Entity Framework Core
-- =============================================================================

-- Désactiver les checks de clés étrangères
SET FOREIGN_KEY_CHECKS = 0;

-- =============================================================================
-- Table: family
-- =============================================================================
-- Anciennes colonnes: id, name
-- Nouvelles colonnes: id, name, created_at
-- Action: Ajouter created_at avec NOW()

INSERT INTO family (id, name, created_at)
SELECT id, name, NOW()
FROM (
    SELECT 1 as id, 'Nironi' as name UNION ALL
    SELECT 2, 'Elfassi'
) AS old_family;

-- =============================================================================
-- Table: user
-- =============================================================================
-- Anciennes colonnes: id, login, pwd, email, first_name, last_name, avatar,
--                     pseudo, notify_list_edit, notify_gift_taken, display_popup,
--                     reset_token, token_expiry, isChildren, family_id
-- Nouvelles colonnes: id, login, pwd, email, first_name, last_name, avatar,
--                     pseudo, notify_list_edit, notify_gift_taken, display_popup,
--                     reset_token, token_expiry, isChildren, is_admin, family_id,
--                     created_at, updated_at
-- Action: Ajouter is_admin (1 pour admin, 0 pour les autres), created_at, updated_at

INSERT INTO user (id, login, pwd, email, first_name, last_name, avatar, pseudo,
                  notify_list_edit, notify_gift_taken, display_popup, reset_token,
                  token_expiry, isChildren, is_admin, family_id, created_at, updated_at)
VALUES
(1, 'admin', '21232f297a57a5a743894a0e4a801fc3', '', 'admin', 'admin', 'default.png', 'admin', 0, 0, 1, NULL, NULL, 0, 1, 0, NOW(), NOW()),
(2, 'evey', '38eed12fd001cf7930f34723a1a2d1c0', 'evey.hyuu@gmail.com', 'Sylvain', 'Nironi', '2.jpg', 'Sylvain', 1, 1, 0, NULL, NULL, 0, 0, 1, NOW(), NOW()),
(3, 'Claire', '182e500f562c7b95a2ae0b4dd9f47bb2', 'claire.nironi@orange.fr', 'Claire', 'Nironi', '3.jpg', 'Claire', 1, 0, 0, NULL, NULL, 0, 0, 1, NOW(), NOW()),
(4, 'marie', 'b3725122c9d3bfef5664619e08e31877', 'scars@wanadoo.fr', 'Marie', 'Nironi', '4.jpg', 'Marie', 1, 0, 0, NULL, NULL, 0, 0, 1, NOW(), NOW()),
(5, 'Francois', 'eb7abf5f00d2dd1678fd3763b90d5ea7', 'Francois.nironi@orange.fr', 'Francois', 'Nironi', '5.jpg', 'Fran?ois', 0, 1, 0, NULL, NULL, 0, 0, 1, NOW(), NOW()),
(6, 'ethan', '7a56cb86e74d2afaacd7524253072fe3', NULL, 'Ethan', 'Nironi', '6.webp', 'Ethan', 0, 0, 0, NULL, NULL, 1, 0, 1, NOW(), NOW()),
(7, 'Fred', 'f71dbe52628a3f83a77ab494817525c6', 'fredfad@wanadoo.fr', 'Fred', 'de LA BAUME', '7.jpg', 'Fred', 1, 1, 0, NULL, NULL, 0, 0, 2, NOW(), NOW()),
(8, 'failor', '713e8c57fa0bcff4c56d30b99d0e0f58', 'elfassifailor4@gmail.com', 'Faïlor', 'Elfassi', '8.jpg', 'Faïlor', 0, 0, 0, NULL, NULL, 0, 0, 2, NOW(), NOW()),
(9, 'anoulak', 'eba7e374bded6c4b86ccd598c2be2e17', 'elfassi@etud.insa-toulouse.fr', 'Anoulak', 'Elfassi', '9.jpg', 'Anoulak', 0, 0, 0, NULL, NULL, 0, 0, 2, NOW(), NOW()),
(10, 'Djédjé', 'b002f2cae70b5669f8e645476bb694e8', NULL, 'Djévalyne', 'Elfassi', '10.jpg', 'Djévalyne', 0, 0, 0, NULL, NULL, 0, 0, 2, NOW(), NOW()),
(11, 'benjamin', '5d9f71b71b207b9e665820c0dce67bdb', 'benjamin.chapeau@gmail.com', 'Benjamin', 'Chapeau', '11.jpg', 'Benjamin', 0, 1, 0, NULL, NULL, 0, 0, 1, NOW(), NOW()),
(12, 'Nélo', '79703398b57464fc8bafff7392cb5bbf', 'alperraudin@gmail.com', 'Perraudin', 'Anne-Laure', '12.jpg', 'Perraudin', 0, 0, 0, NULL, NULL, 0, 0, 2, NOW(), NOW()),
(13, 'Chloe', '8393748820902afb24d969ee0d3e9868', 'chloe.gaimard@hotmail.fr', 'Chloe', 'Gaimard', '13.jpg', 'Chloe', 0, 1, 0, NULL, NULL, 0, 0, 1, NOW(), NOW()),
(14, 'alexis', '059bf68f71c80fce55214b411dd2280c', NULL, 'Alexis', 'Chapeau', '14.jpg', 'Alexis', 0, 0, 0, NULL, NULL, 1, 0, 1, NOW(), NOW()),
(15, 'taz', '2d3a3249cdc7b4663db44d4ca8252d75', NULL, 'Taz', 'Nironi', '15.jpg', 'taz', 0, 0, 0, NULL, NULL, 1, 0, 1, NOW(), NOW());

-- =============================================================================
-- Table: lists
-- =============================================================================
-- Anciennes colonnes: id, name, user_id, filename
-- Nouvelles colonnes: id, name, user_id, created_at, updated_at
-- Action: Supprimer filename, ajouter created_at, updated_at

INSERT INTO lists (id, name, user_id, created_at, updated_at)
VALUES
(1, 'Sylvain', 2, NOW(), NOW()),
(2, 'Admin', 1, NOW(), NOW()),
(3, 'Claire', 3, NOW(), NOW()),
(4, 'Marie', 4, NOW(), NOW()),
(5, 'Francois', 5, NOW(), NOW()),
(6, 'Ethan', 6, NOW(), NOW()),
(7, 'Frédérique', 7, NOW(), NOW()),
(8, 'Faïlor', 8, NOW(), NOW()),
(9, 'Anoulak', 9, NOW(), NOW()),
(10, 'Djévalyne', 10, NOW(), NOW()),
(11, 'Benjamin', 11, NOW(), NOW()),
(12, 'Anne-Laure', 12, NOW(), NOW()),
(13, 'Chloé', 13, NOW(), NOW()),
(14, 'Alexis', 14, NOW(), NOW()),
(16, 'Taz', 15, NOW(), NOW());

-- =============================================================================
-- Table: gifts
-- =============================================================================
-- IMPORTANT: Ce script charge uniquement les métadonnées des cadeaux.
-- Les données complètes (INSERT) doivent être extraites du dump original
-- puis modifiées pour ajouter is_group_gift (0), created_at (NOW()),
-- updated_at (NOW())
--
-- Format d'origine:
-- INSERT INTO `gifts` (`id`, `list_id`, `name`, `description`, `image`, `link`,
--                      `cost`, `currency`, `available`, `taken_by`, `comment`, `year`)
--
-- Format requis:
-- INSERT INTO gifts (id, list_id, name, description, image, link, cost, currency,
--                    available, taken_by, is_group_gift, comment, year,
--                    created_at, updated_at)
-- VALUES (..., 0, ..., NOW(), NOW());
--
-- INSTRUCTION: Copiez tous les INSERT INTO gifts du fichier
-- nironico_nawel_update.sql (lignes 248 à ~2850) et ajoutez pour chaque ligne:
-- - Remplacez `gifts` par gifts (sans backticks)
-- - Ajoutez , 0 après taken_by (pour is_group_gift)
-- - Ajoutez , NOW(), NOW() à la fin (pour created_at, updated_at)
--
-- Exemple de transformation:
-- AVANT: (1, 1, 'T-shirt', 'Desc', 'img.jpg', 'link', 19.99, 'EUR', 1, NULL, NULL, 2016),
-- APRES: (1, 1, 'T-shirt', 'Desc', 'img.jpg', 'link', 19.99, 'EUR', 1, NULL, 0, NULL, 2016, NOW(), NOW()),
-- =============================================================================

-- NOTE: Les données des cadeaux sont trop volumineuses pour être incluses ici.
-- Elles seront ajoutées dans un fichier séparé ou via un script de transformation.

-- =============================================================================
-- Table: gift_participation
-- =============================================================================
-- Anciennes colonnes: id, gift_id, user_id, is_active
-- Nouvelles colonnes: id, gift_id, user_id, is_active, created_at
-- Action: Ajouter created_at

-- NOTE: Comme pour gifts, les données sont volumineuses.
-- Format de transformation requis:
-- AVANT: (4, 153, 2, 1),
-- APRES: (4, 153, 2, 1, NOW()),

-- =============================================================================
-- Réactiver les checks
-- =============================================================================

SET FOREIGN_KEY_CHECKS = 1;

-- =============================================================================
-- Vérifications
-- =============================================================================

SELECT 'Import des données terminé' as status;
SELECT COUNT(*) as family_count FROM family;
SELECT COUNT(*) as user_count FROM user;
SELECT COUNT(*) as list_count FROM lists;
SELECT COUNT(*) as gift_count FROM gifts;
SELECT COUNT(*) as participation_count FROM gift_participation;
