-- Data migration script
-- This script migrates data from the old structure to the new one
-- IMPORTANT: Run this AFTER importing the old data into a temporary database

-- Note: This script assumes you've imported the old nironico_nawel.sql dump
-- and now want to clean up the data for the new schema

-- Delete chat_messages table if it exists (not needed in new version)
-- Already handled in 001_initial_schema.sql

-- Remove filename column from lists (if migrating from existing DB)
-- ALTER TABLE `lists` DROP COLUMN IF EXISTS `filename`;

-- Update charset to utf8mb4 for better Unicode support
ALTER TABLE `family` CONVERT TO CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
ALTER TABLE `user` CONVERT TO CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
ALTER TABLE `lists` CONVERT TO CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
ALTER TABLE `gifts` CONVERT TO CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
ALTER TABLE `gift_participation` CONVERT TO CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

-- Note: Passwords are still in MD5 format
-- They will be migrated to BCrypt on first login by the application
