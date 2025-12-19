-- =============================================================================
-- Script pour ajuster la structure des tables après import du dump SQL
-- =============================================================================
-- Ce script ajoute les colonnes manquantes attendues par EF Core
-- =============================================================================

-- Désactiver les checks
SET FOREIGN_KEY_CHECKS = 0;

-- =============================================================================
-- Table: family
-- =============================================================================

-- Ajouter created_at si manquant
ALTER TABLE family
ADD COLUMN IF NOT EXISTS created_at datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6);

-- =============================================================================
-- Table: user
-- =============================================================================

-- Ajouter created_at et updated_at si manquant
ALTER TABLE user
ADD COLUMN IF NOT EXISTS created_at datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6);

ALTER TABLE user
ADD COLUMN IF NOT EXISTS updated_at datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6);

-- =============================================================================
-- Table: lists
-- =============================================================================

ALTER TABLE lists
ADD COLUMN IF NOT EXISTS created_at datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6);

ALTER TABLE lists
ADD COLUMN IF NOT EXISTS updated_at datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6);

-- =============================================================================
-- Table: gifts
-- =============================================================================

ALTER TABLE gifts
ADD COLUMN IF NOT EXISTS created_at datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6);

ALTER TABLE gifts
ADD COLUMN IF NOT EXISTS updated_at datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6);

-- =============================================================================
-- Table: gift_participation
-- =============================================================================

ALTER TABLE gift_participation
ADD COLUMN IF NOT EXISTS created_at datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6);

-- =============================================================================
-- Réactiver les checks
-- =============================================================================

SET FOREIGN_KEY_CHECKS = 1;

-- =============================================================================
-- Vérifications
-- =============================================================================

SELECT 'Structure des tables mise à jour' as status;

-- Afficher la structure de user pour vérifier
DESCRIBE user;
