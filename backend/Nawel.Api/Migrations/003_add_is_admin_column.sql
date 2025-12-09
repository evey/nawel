-- Migration: Add is_admin column to user table
-- Date: 2025-12-08
-- Description: Adds role-based authorization support by adding is_admin column

-- Add is_admin column (defaults to FALSE for security)
ALTER TABLE user ADD COLUMN is_admin BOOLEAN DEFAULT FALSE NOT NULL;

-- Set the first user (id=1) as admin
-- NOTE: Adjust this if your admin user has a different ID
UPDATE user SET is_admin = TRUE WHERE id = 1;

-- Create index for faster admin queries (optional but recommended)
CREATE INDEX idx_user_is_admin ON user(is_admin);
