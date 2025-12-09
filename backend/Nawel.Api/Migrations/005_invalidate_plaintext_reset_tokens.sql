-- Migration 005: Invalidate existing plain-text reset tokens
-- This migration clears all existing reset tokens that were stored in plain-text
-- Execute this migration AFTER deploying the code that hashes reset tokens (Task 1.5)

-- ============================================================================
-- IMPORTANT: Run this AFTER code deployment (Task 1.5)
-- ============================================================================

-- Background:
-- Previous implementation stored reset tokens in plain-text (Base64 encoded random bytes)
-- New implementation stores SHA256 hashes of reset tokens
-- Existing plain-text tokens in the database won't match the new hashed comparison
-- Therefore, we need to invalidate all existing reset tokens

-- Step 1: Check how many active reset tokens exist
SELECT
    COUNT(*) as active_reset_tokens,
    COUNT(CASE WHEN token_expiry >= DATETIME('now') THEN 1 END) as non_expired_tokens  -- SQLite
FROM user
WHERE reset_token IS NOT NULL;

-- For MySQL, use this version instead:
/*
SELECT
    COUNT(*) as active_reset_tokens,
    COUNT(CASE WHEN token_expiry >= NOW() THEN 1 END) as non_expired_tokens
FROM user
WHERE reset_token IS NOT NULL;
*/

-- Step 2: List affected users (for notification purposes)
SELECT
    id,
    login,
    email,
    first_name,
    last_name,
    token_expiry
FROM user
WHERE reset_token IS NOT NULL
ORDER BY token_expiry DESC;

-- Step 3: Invalidate all existing plain-text reset tokens
-- This is safe because:
-- 1. Old tokens won't work with the new hashing system anyway
-- 2. Users can simply request a new password reset token
-- 3. Token validity is typically short (1 hour)

UPDATE user
SET
    reset_token = NULL,
    token_expiry = NULL
WHERE reset_token IS NOT NULL;

-- Step 4: Verify all tokens have been cleared
SELECT COUNT(*) as remaining_tokens
FROM user
WHERE reset_token IS NOT NULL;

-- Expected result: 0

-- ============================================================================
-- POST-MIGRATION NOTES
-- ============================================================================

-- Users who had active password reset tokens will need to request a new one
-- The new tokens will be properly hashed using SHA256

-- If any users complain about reset tokens not working:
-- 1. Verify this migration was applied successfully
-- 2. Ask them to request a new password reset email
-- 3. New tokens will be generated with proper SHA256 hashing

-- ============================================================================
-- NO ROLLBACK NEEDED
-- ============================================================================
-- This migration simply clears temporary data (reset tokens)
-- No schema changes or data loss concerns
-- Users can always request new reset tokens
-- ============================================================================
