# ============================================================================
# Direct MySQL to SQLite Data Import for Nawel
# ============================================================================
# This script imports data from MySQL dump using a different approach:
# 1. Creates a temporary SQLite database with MySQL structure
# 2. Imports the cleaned SQL
# 3. Migrates data to the new structure
# ============================================================================

param(
    [string]$MysqlDump = "..\..\..\old\nironico_nawel_update.sql",
    [string]$TargetDb = "..\nawel.db",
    [string]$Sqlite3 = "..\sqlite3.exe"
)

$ErrorActionPreference = "Stop"

Write-Host "Starting MySQL to SQLite data import..." -ForegroundColor Cyan
Write-Host "Source: $MysqlDump" -ForegroundColor Gray
Write-Host "Target: $TargetDb" -ForegroundColor Gray
Write-Host ""

# ============================================================================
# Step 1: Extract only INSERT statements from MySQL dump
# ============================================================================
Write-Host "Step 1: Extracting INSERT statements..." -ForegroundColor Yellow

$content = Get-Content $MysqlDump -Raw -Encoding UTF8
$tempSql = "temp_mysql_inserts.sql"

# Extract only INSERT statements for our tables
$inserts = @()
$tables = @('family', 'user', 'lists', 'gifts', 'gift_participation')

foreach ($table in $tables) {
    $pattern = "INSERT INTO ``$table``[^;]+;"
    $matches = [regex]::Matches($content, $pattern, [System.Text.RegularExpressions.RegexOptions]::Singleline)
    foreach ($match in $matches) {
        $insert = $match.Value

        # Remove backticks
        $insert = $insert -replace '`', ''

        # Clean MySQL-specific escapes for SQLite
        # IMPORTANT: Order matters - do \' before \\
        $insert = $insert -replace "\\r\\n", " "
        $insert = $insert -replace "\\n", " "
        $insert = $insert -replace "\\'", "''"  # MySQL \' -> SQLite ''
        $insert = $insert -replace "\\\\", "\"  # MySQL \\ -> SQLite \

        $inserts += $insert
    }
}

# Write with UTF-8 without BOM
$Utf8NoBomEncoding = New-Object System.Text.UTF8Encoding $False
[System.IO.File]::WriteAllLines($tempSql, $inserts, $Utf8NoBomEncoding)
Write-Host "[OK] Extracted $($inserts.Count) INSERT statements" -ForegroundColor Green

# ============================================================================
# Step 2: Create temporary database with old structure
# ============================================================================
Write-Host "`nStep 2: Creating temporary database..." -ForegroundColor Yellow

$tempDb = "temp_old_structure.db"
if (Test-Path $tempDb) { Remove-Item $tempDb }

# Create tables with old structure (matching MySQL)
$createTables = @"
CREATE TABLE family (
    id INTEGER PRIMARY KEY,
    name TEXT NOT NULL
);

CREATE TABLE user (
    id INTEGER PRIMARY KEY,
    login TEXT NOT NULL,
    pwd TEXT NOT NULL,
    email TEXT,
    first_name TEXT,
    last_name TEXT,
    avatar TEXT NOT NULL,
    pseudo TEXT,
    notify_list_edit INTEGER NOT NULL,
    notify_gift_taken INTEGER NOT NULL,
    display_popup INTEGER NOT NULL,
    reset_token TEXT,
    token_expiry TEXT,
    isChildren INTEGER NOT NULL,
    family_id INTEGER NOT NULL
);

CREATE TABLE lists (
    id INTEGER PRIMARY KEY,
    name TEXT NOT NULL,
    user_id INTEGER NOT NULL,
    filename TEXT
);

CREATE TABLE gifts (
    id INTEGER PRIMARY KEY,
    list_id INTEGER NOT NULL,
    name TEXT NOT NULL,
    description TEXT,
    image TEXT,
    link TEXT,
    cost REAL,
    currency TEXT,
    available INTEGER NOT NULL,
    taken_by INTEGER,
    comment TEXT,
    year INTEGER
);

CREATE TABLE gift_participation (
    id INTEGER PRIMARY KEY,
    gift_id INTEGER NOT NULL,
    user_id INTEGER NOT NULL,
    is_active INTEGER NOT NULL
);
"@

$createTables | & $Sqlite3 $tempDb
Write-Host "[OK] Temporary database created" -ForegroundColor Green

# ============================================================================
# Step 3: Import data into temporary database
# ============================================================================
Write-Host "`nStep 3: Importing data into temporary database..." -ForegroundColor Yellow

Get-Content $tempSql -Encoding UTF8 | & $Sqlite3 $tempDb 2>&1 | Out-Null
Write-Host "[OK] Data imported" -ForegroundColor Green

# ============================================================================
# Step 4: Verify temp database
# ============================================================================
Write-Host "`nStep 4: Verifying temporary database..." -ForegroundColor Yellow

$familyCount = & $Sqlite3 $tempDb "SELECT COUNT(*) FROM family;"
$userCount = & $Sqlite3 $tempDb "SELECT COUNT(*) FROM user;"
$listCount = & $Sqlite3 $tempDb "SELECT COUNT(*) FROM lists;"
$giftCount = & $Sqlite3 $tempDb "SELECT COUNT(*) FROM gifts;"
$partCount = & $Sqlite3 $tempDb "SELECT COUNT(*) FROM gift_participation;"

Write-Host "  Families: $familyCount" -ForegroundColor Gray
Write-Host "  Users: $userCount" -ForegroundColor Gray
Write-Host "  Lists: $listCount" -ForegroundColor Gray
Write-Host "  Gifts: $giftCount" -ForegroundColor Gray
Write-Host "  Participations: $partCount" -ForegroundColor Gray
Write-Host "[OK] Data verified" -ForegroundColor Green

# ============================================================================
# Step 5: Migrate to target database with new structure
# ============================================================================
Write-Host "`nStep 5: Migrating to target database..." -ForegroundColor Yellow

$migrationSql = @"
ATTACH DATABASE '$tempDb' AS old_db;

-- Clear existing data
DELETE FROM gift_participation;
DELETE FROM gifts;
DELETE FROM lists;
DELETE FROM user;
DELETE FROM family;

-- Insert families
INSERT INTO family (id, name, created_at)
SELECT id, name, datetime('now') FROM old_db.family;

-- Insert users
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
    CASE WHEN id = 1 THEN 1 ELSE 0 END as is_admin,
    datetime('now'), datetime('now')
FROM old_db.user;

-- Insert lists
INSERT INTO lists (id, name, user_id, created_at, updated_at)
SELECT id, name, user_id, datetime('now'), datetime('now') FROM old_db.lists;

-- Insert gifts
INSERT INTO gifts (
    id, list_id, name, description, image, link, cost, currency,
    available, taken_by, is_group_gift, comment, year,
    created_at, updated_at
)
SELECT
    id, list_id, name, description, image, link, cost, currency,
    available, taken_by, 0 as is_group_gift, comment, year,
    datetime('now'), datetime('now')
FROM old_db.gifts;

-- Insert gift_participation
INSERT INTO gift_participation (id, gift_id, user_id, is_active, created_at)
SELECT id, gift_id, user_id, is_active, datetime('now') FROM old_db.gift_participation;

-- Update gift is_group_gift flag
UPDATE gifts SET is_group_gift = 1 WHERE id IN (
    SELECT DISTINCT gift_id FROM gift_participation WHERE is_active = 1
);

DETACH DATABASE old_db;
"@

$migrationSql | & $Sqlite3 $TargetDb

Write-Host "[OK] Data migrated to target database" -ForegroundColor Green

# ============================================================================
# Step 6: Verify target database
# ============================================================================
Write-Host "`nStep 6: Verifying target database..." -ForegroundColor Yellow

$familyCount = & $Sqlite3 $TargetDb "SELECT COUNT(*) FROM family;"
$userCount = & $Sqlite3 $TargetDb "SELECT COUNT(*) FROM user;"
$listCount = & $Sqlite3 $TargetDb "SELECT COUNT(*) FROM lists;"
$giftCount = & $Sqlite3 $TargetDb "SELECT COUNT(*) FROM gifts;"
$partCount = & $Sqlite3 $TargetDb "SELECT COUNT(*) FROM gift_participation;"

Write-Host "  Families: $familyCount" -ForegroundColor Green
Write-Host "  Users: $userCount" -ForegroundColor Green
Write-Host "  Lists: $listCount" -ForegroundColor Green
Write-Host "  Gifts: $giftCount" -ForegroundColor Green
Write-Host "  Participations: $partCount" -ForegroundColor Green

# ============================================================================
# Step 7: Cleanup
# ============================================================================
Write-Host "`nStep 7: Cleaning up..." -ForegroundColor Yellow

Remove-Item $tempSql -ErrorAction SilentlyContinue
Remove-Item $tempDb -ErrorAction SilentlyContinue

Write-Host "[OK] Cleanup complete" -ForegroundColor Green

Write-Host "`n===============================================" -ForegroundColor Cyan
Write-Host "Migration completed successfully!" -ForegroundColor Green
Write-Host "===============================================" -ForegroundColor Cyan
