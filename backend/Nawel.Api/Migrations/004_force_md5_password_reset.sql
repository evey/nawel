-- Migration 004: Force password reset for accounts with legacy MD5 passwords
-- This migration identifies and handles accounts that still use MD5 password hashing
-- Execute this migration AFTER deploying the code that removes MD5 support

-- ============================================================================
-- IMPORTANT: Run this AFTER code deployment (Task 1.4)
-- ============================================================================

-- Step 1: Identify users with MD5 passwords (32 character hex strings)
-- SQLite and MySQL compatible query
SELECT
    id,
    login,
    email,
    first_name,
    last_name,
    LENGTH(password) as pwd_length,
    SUBSTR(password, 1, 2) as pwd_prefix
FROM user
WHERE
    LENGTH(password) = 32
    AND SUBSTR(password, 1, 2) != '$2'
    AND id != 1; -- Exclude admin account

-- Step 2: Set reset tokens for affected users to force password reset
-- This will require users to reset their password via email before they can login

-- For each affected user, you have two options:

-- OPTION A (Recommended): Manually notify users via email and let them reset password
-- No database changes needed - users will be unable to login and must use password reset

-- OPTION B: Force immediate token generation (requires manual email sending)
-- Uncomment below to generate reset tokens (valid for 7 days)

/*
UPDATE user
SET
    reset_token = LOWER(HEX(RANDOMBLOB(16))),  -- SQLite: generates random token
    token_expiry = DATETIME('now', '+7 days')
WHERE
    LENGTH(password) = 32
    AND SUBSTR(password, 1, 2) != '$2'
    AND id != 1
    AND email IS NOT NULL;
*/

-- For MySQL, use this version of OPTION B instead:
/*
UPDATE user
SET
    reset_token = LOWER(HEX(RANDOM_BYTES(16))),  -- MySQL: generates random token
    token_expiry = DATE_ADD(NOW(), INTERVAL 7 DAY)
WHERE
    LENGTH(password) = 32
    AND SUBSTR(password, 1, 2) != '$2'
    AND id != 1
    AND email IS NOT NULL;
*/

-- Step 3: Log affected users for manual notification
-- Copy the output and send password reset emails manually
-- Or use a script to send emails via your email service

-- ============================================================================
-- POST-MIGRATION VERIFICATION
-- ============================================================================

-- Check how many users are affected:
SELECT COUNT(*) as affected_users
FROM user
WHERE
    LENGTH(password) = 32
    AND SUBSTR(password, 1, 2) != '$2'
    AND id != 1;

-- List all affected users with contact info:
SELECT
    id,
    login,
    email,
    CONCAT(first_name, ' ', last_name) as full_name
FROM user
WHERE
    LENGTH(password) = 32
    AND SUBSTR(password, 1, 2) != '$2'
    AND id != 1;

-- ============================================================================
-- ROLLBACK (Emergency Only)
-- ============================================================================
-- If you need to temporarily re-enable MD5 support, redeploy the previous
-- version of AuthService.cs. This is NOT recommended for security reasons.
-- ============================================================================
