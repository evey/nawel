-- ============================================================================
-- Import CSV data into SQLite with schema transformation
-- ============================================================================

-- Attach temp database with CSV data
ATTACH DATABASE 'temp_old_structure.db' AS temp_db;

-- Clear target tables
DELETE FROM gift_participation;
DELETE FROM gifts;
DELETE FROM lists;
DELETE FROM user;
DELETE FROM family;

-- ============================================================================
-- Import FAMILY
-- ============================================================================
INSERT INTO family (id, name, created_at)
SELECT id, name, datetime('now')
FROM temp_db.family;

-- ============================================================================
-- Import USER
-- ============================================================================
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
    CASE WHEN id = 1 OR id = 2 THEN 1 ELSE 0 END as is_admin,
    datetime('now'), datetime('now')
FROM temp_db.user;

-- ============================================================================
-- Import LISTS (skip filename column)
-- ============================================================================
INSERT INTO lists (id, name, user_id, created_at, updated_at)
SELECT id, name, user_id, datetime('now'), datetime('now')
FROM temp_db.lists;

-- ============================================================================
-- Import GIFTS
-- ============================================================================
INSERT INTO gifts (
    id, list_id, name, description, image, link, cost, currency,
    available, taken_by, is_group_gift, comment, year,
    created_at, updated_at
)
SELECT
    id, list_id, name, description, image, link, cost, currency,
    available, taken_by, 0 as is_group_gift, comment, year,
    datetime('now'), datetime('now')
FROM temp_db.gifts;

-- ============================================================================
-- Import GIFT_PARTICIPATION
-- ============================================================================
INSERT INTO gift_participation (id, gift_id, user_id, is_active, created_at)
SELECT id, gift_id, user_id, is_active, datetime('now')
FROM temp_db.gift_participation;

-- Update is_group_gift flag for gifts with active participation
UPDATE gifts SET is_group_gift = 1 WHERE id IN (
    SELECT DISTINCT gift_id FROM gift_participation WHERE is_active = 1
);

-- Detach
DETACH DATABASE temp_db;
