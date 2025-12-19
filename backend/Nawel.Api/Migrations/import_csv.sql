-- ============================================================================
-- Import CSV data into temporary tables then migrate to final schema
-- ============================================================================

-- Clear existing data
DELETE FROM gift_participation;
DELETE FROM gifts;
DELETE FROM lists;
DELETE FROM user;
DELETE FROM family;

-- Create temporary tables for CSV import
CREATE TEMP TABLE temp_family (
    id INTEGER,
    name TEXT
);

CREATE TEMP TABLE temp_user (
    id INTEGER,
    login TEXT,
    pwd TEXT,
    email TEXT,
    first_name TEXT,
    last_name TEXT,
    avatar TEXT,
    pseudo TEXT,
    notify_list_edit INTEGER,
    notify_gift_taken INTEGER,
    display_popup INTEGER,
    reset_token TEXT,
    token_expiry TEXT,
    isChildren INTEGER,
    family_id INTEGER
);

CREATE TEMP TABLE temp_lists (
    id INTEGER,
    name TEXT,
    user_id INTEGER,
    filename TEXT
);

CREATE TEMP TABLE temp_gifts (
    id INTEGER,
    list_id INTEGER,
    name TEXT,
    description TEXT,
    image TEXT,
    link TEXT,
    cost REAL,
    currency TEXT,
    available INTEGER,
    taken_by INTEGER,
    comment TEXT,
    year INTEGER
);

CREATE TEMP TABLE temp_gift_participation (
    id INTEGER,
    gift_id INTEGER,
    user_id INTEGER,
    is_active INTEGER
);

-- Import CSV files
.mode csv
.import Migrations/csv_export/family.csv temp_family
.import Migrations/csv_export/user.csv temp_user
.import Migrations/csv_export/lists.csv temp_lists
.import Migrations/csv_export/gifts.csv temp_gifts
.import Migrations/csv_export/gift_participation.csv temp_gift_participation

-- Migrate to final schema
INSERT INTO family (id, name, created_at)
SELECT id, name, datetime('now') FROM temp_family;

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
    CASE WHEN id = 1 OR id = 2 THEN 1 ELSE 0 END,
    datetime('now'), datetime('now')
FROM temp_user;

INSERT INTO lists (id, name, user_id, created_at, updated_at)
SELECT id, name, user_id, datetime('now'), datetime('now') FROM temp_lists;

INSERT INTO gifts (
    id, list_id, name, description, image, link, cost, currency,
    available, taken_by, is_group_gift, comment, year,
    created_at, updated_at
)
SELECT
    id, list_id, name, description, image, link, cost, currency,
    available, taken_by, 0, comment, year,
    datetime('now'), datetime('now')
FROM temp_gifts;

INSERT INTO gift_participation (id, gift_id, user_id, is_active, created_at)
SELECT id, gift_id, user_id, is_active, datetime('now') FROM temp_gift_participation;

UPDATE gifts SET is_group_gift = 1 WHERE id IN (
    SELECT DISTINCT gift_id FROM gift_participation WHERE is_active = 1
);

-- Fix empty strings to NULL for nullable columns (CSV import creates empty strings)
UPDATE user SET email = NULL WHERE email = '';
UPDATE user SET first_name = NULL WHERE first_name = '';
UPDATE user SET last_name = NULL WHERE last_name = '';
UPDATE user SET pseudo = NULL WHERE pseudo = '';
UPDATE user SET reset_token = NULL WHERE reset_token = '';
UPDATE user SET token_expiry = NULL WHERE token_expiry = '';

UPDATE gifts SET description = NULL WHERE description = '';
UPDATE gifts SET image = NULL WHERE image = '';
UPDATE gifts SET link = NULL WHERE link = '';
UPDATE gifts SET taken_by = NULL WHERE taken_by = '';
UPDATE gifts SET comment = NULL WHERE comment = '';
UPDATE gifts SET cost = NULL WHERE CAST(cost AS TEXT) = '';
UPDATE gifts SET currency = NULL WHERE currency = '';
