-- Migration: Import data from old database (nironico_nawel)
-- Date: 2025-12-09
-- Description: Migrates all data from the legacy database to the new system
--
-- IMPORTANT NOTES:
-- 1. This script assumes you are running it from a clean database or want to merge data
-- 2. MD5 passwords are preserved - users will be prompted to reset on first login
-- 3. The old 'filename' column in lists is ignored (no longer used)
-- 4. Group gifts are detected based on gift_participation table
-- 5. Family ID 0 (admin's family) is migrated as-is
--
-- HOW TO USE:
-- 1. BACKUP YOUR CURRENT DATABASE FIRST!
-- 2. Make sure the old database dump is available
-- 3. For SQLite:
--    a. Attach old database: sqlite3 nawel.db ".read old/nironico_nawel.sql"
--    b. Then run this script: sqlite3 nawel.db < Migrations/006_migrate_from_old_database.sql
-- 4. For MySQL:
--    a. Create a temporary database: CREATE DATABASE nawel_old;
--    b. Import old data: mysql nawel_old < old/nironico_nawel.sql
--    c. Update database name in this script (replace 'nawel_old' if different)
--    d. Run: mysql nawel < Migrations/006_migrate_from_old_database.sql

-- ============================================================================
-- SECTION 1: Migrate Families
-- ============================================================================

-- Note: Family table structure is compatible, just add created_at timestamp
INSERT INTO family (id, name, created_at)
SELECT
    id,
    name,
    CURRENT_TIMESTAMP as created_at
FROM family
WHERE id NOT IN (SELECT id FROM family)
ON CONFLICT(id) DO NOTHING;

-- ============================================================================
-- SECTION 2: Migrate Users
-- ============================================================================

-- Note: MD5 passwords are preserved intentionally
-- The system will detect MD5 passwords on login and guide users through reset
-- See MIGRATION_MD5_PLAN.md for details on the password migration flow

INSERT INTO user (
    id,
    login,
    pwd,
    email,
    first_name,
    last_name,
    avatar,
    pseudo,
    notify_list_edit,
    notify_gift_taken,
    display_popup,
    reset_token,
    token_expiry,
    isChildren,
    family_id,
    is_admin,
    created_at,
    updated_at
)
SELECT
    id,
    login,
    pwd as pwd,  -- MD5 passwords preserved, will be migrated on first login
    email,
    first_name,
    last_name,
    CASE
        WHEN avatar = 'default.png' THEN 'avatar.png'  -- Normalize default avatar
        ELSE avatar
    END as avatar,
    pseudo,
    CAST(notify_list_edit AS BOOLEAN),
    CAST(notify_gift_taken AS BOOLEAN),
    CAST(display_popup AS BOOLEAN),
    reset_token,
    token_expiry,
    CAST(isChildren AS BOOLEAN),
    family_id,
    CASE WHEN id = 1 THEN TRUE ELSE FALSE END as is_admin,  -- Set admin flag for user ID 1
    CURRENT_TIMESTAMP as created_at,
    CURRENT_TIMESTAMP as updated_at
FROM user
WHERE id NOT IN (SELECT id FROM user)
ON CONFLICT(id) DO NOTHING;

-- ============================================================================
-- SECTION 3: Migrate Lists
-- ============================================================================

-- Note: 'filename' column is ignored as it's no longer used in the new system
INSERT INTO lists (id, name, user_id, created_at, updated_at)
SELECT
    id,
    name,
    user_id,
    CURRENT_TIMESTAMP as created_at,
    CURRENT_TIMESTAMP as updated_at
FROM lists
WHERE id NOT IN (SELECT id FROM lists)
ON CONFLICT(id) DO NOTHING;

-- ============================================================================
-- SECTION 4: Migrate Gifts
-- ============================================================================

-- Note: is_group_gift is determined by checking if the gift has participants
-- A gift is considered a group gift if it has entries in gift_participation table
INSERT INTO gifts (
    id,
    list_id,
    name,
    description,
    image,
    link,
    cost,
    currency,
    available,
    taken_by,
    is_group_gift,
    comment,
    year,
    created_at,
    updated_at
)
SELECT
    g.id,
    g.list_id,
    g.name,
    g.description,
    g.image,
    g.link,
    g.cost,
    g.currency,
    CAST(g.available AS BOOLEAN),
    g.taken_by,
    -- Determine if this is a group gift by checking participation table
    CASE
        WHEN EXISTS (
            SELECT 1 FROM gift_participation gp
            WHERE gp.gift_id = g.id AND gp.is_active = 1
        ) THEN TRUE
        ELSE FALSE
    END as is_group_gift,
    g.comment,
    g.year,
    CURRENT_TIMESTAMP as created_at,
    CURRENT_TIMESTAMP as updated_at
FROM gifts g
WHERE g.id NOT IN (SELECT id FROM gifts)
ON CONFLICT(id) DO NOTHING;

-- ============================================================================
-- SECTION 5: Migrate Gift Participations
-- ============================================================================

-- Note: gift_participation structure is compatible, just add created_at timestamp
INSERT INTO gift_participation (id, gift_id, user_id, is_active, created_at)
SELECT
    id,
    gift_id,
    user_id,
    CAST(is_active AS BOOLEAN),
    CURRENT_TIMESTAMP as created_at
FROM gift_participation
WHERE id NOT IN (SELECT id FROM gift_participation)
ON CONFLICT(id) DO NOTHING;

-- ============================================================================
-- SECTION 6: Update Auto-Increment Sequences (SQLite)
-- ============================================================================

-- For SQLite, update the sqlite_sequence table to continue from the highest ID
-- This prevents ID conflicts when inserting new records after migration

UPDATE sqlite_sequence SET seq = (SELECT MAX(id) FROM family) WHERE name = 'family';
UPDATE sqlite_sequence SET seq = (SELECT MAX(id) FROM user) WHERE name = 'user';
UPDATE sqlite_sequence SET seq = (SELECT MAX(id) FROM lists) WHERE name = 'lists';
UPDATE sqlite_sequence SET seq = (SELECT MAX(id) FROM gifts) WHERE name = 'gifts';
UPDATE sqlite_sequence SET seq = (SELECT MAX(id) FROM gift_participation) WHERE name = 'gift_participation';

-- ============================================================================
-- SECTION 7: Verification Queries
-- ============================================================================

-- Run these queries after migration to verify success:

-- Check family migration
-- SELECT COUNT(*) as family_count FROM family;

-- Check user migration (should see MD5 passwords as 32-char strings)
-- SELECT COUNT(*) as user_count FROM user;
-- SELECT id, login, LENGTH(pwd) as pwd_length, is_admin FROM user LIMIT 5;

-- Check lists migration
-- SELECT COUNT(*) as list_count FROM lists;

-- Check gifts migration
-- SELECT COUNT(*) as gift_count FROM gifts;
-- SELECT COUNT(*) as group_gift_count FROM gifts WHERE is_group_gift = TRUE;

-- Check gift_participation migration
-- SELECT COUNT(*) as participation_count FROM gift_participation;

-- Verify group gifts have participations
-- SELECT g.id, g.name, g.is_group_gift, COUNT(gp.id) as participant_count
-- FROM gifts g
-- LEFT JOIN gift_participation gp ON g.id = gp.gift_id
-- WHERE g.is_group_gift = TRUE
-- GROUP BY g.id, g.name, g.is_group_gift;

-- ============================================================================
-- SECTION 8: Post-Migration Notes
-- ============================================================================

-- IMPORTANT: After running this migration:
--
-- 1. MD5 Password Migration:
--    - All users with MD5 passwords will be prompted to reset on first login
--    - The system automatically detects MD5 passwords (32 chars, no $2 prefix)
--    - Users will receive an email to reset their password securely
--    - See MIGRATION_MD5_PLAN.md for complete flow documentation
--
-- 2. Avatar Files:
--    - Copy avatar files from old system to new uploads directory
--    - Old path: old/uploads/avatars/*.{jpg,png,webp}
--    - New path: backend/Nawel.Api/uploads/avatars/*.{jpg,png,webp}
--    - Command: cp -r old/uploads/avatars/* backend/Nawel.Api/uploads/avatars/
--
-- 3. Admin Access:
--    - User ID 1 is automatically set as admin
--    - Verify: SELECT id, login, is_admin FROM user WHERE is_admin = TRUE;
--    - If different user should be admin, update manually
--
-- 4. Data Integrity:
--    - Run the verification queries above
--    - Check that gift reservations (taken_by) reference valid user IDs
--    - Verify group gifts have corresponding participations
--    - Ensure all lists belong to existing users
--
-- 5. Family Structure:
--    - Family ID 0 is preserved (admin's family in old system)
--    - Verify all users belong to valid families
--    - Query: SELECT COUNT(*) FROM user WHERE family_id NOT IN (SELECT id FROM family);
--
-- 6. Year Data:
--    - Gifts span multiple years (check min/max year)
--    - Query: SELECT MIN(year) as oldest, MAX(year) as newest, COUNT(*) as total FROM gifts;
--    - Ensure year filtering works correctly in the application
--
-- 7. Testing Checklist:
--    ✓ Login with MD5 user (should trigger reset flow)
--    ✓ Login with migrated user after password reset
--    ✓ View gift lists for different years
--    ✓ Reserve a normal gift
--    ✓ Participate in a group gift
--    ✓ View avatar images
--    ✓ Test family member visibility
--    ✓ Admin panel access for admin user

-- ============================================================================
-- END OF MIGRATION
-- ============================================================================
