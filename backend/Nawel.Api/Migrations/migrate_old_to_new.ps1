# ============================================================================
# Nawel Database Migration Script (PowerShell)
# ============================================================================
# This script automates the migration from the old database (nironico_nawel.sql)
# to the new Nawel system.
#
# Usage: .\migrate_old_to_new.ps1 [-BackupOnly] [-VerifyOnly] [-Help]
#
# Parameters:
#   -BackupOnly    Create backup only without migration
#   -VerifyOnly    Run verification queries only
#   -Help          Show this help message
#
# Prerequisites:
#   - Old database file: old/nironico_nawel.sql (relative to project root)
#   - SQLite3 installed (download from https://www.sqlite.org/download.html)
#   - Current directory: backend/Nawel.Api/
# ============================================================================

param(
    [switch]$BackupOnly,
    [switch]$VerifyOnly,
    [switch]$Help
)

# Strict mode
$ErrorActionPreference = "Stop"

# Directories
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$ApiDir = Split-Path -Parent $ScriptDir
$ProjectRoot = Split-Path -Parent (Split-Path -Parent $ApiDir)
$OldSql = Join-Path $ProjectRoot "old\nironico_nawel_update.sql"
$DbFile = Join-Path $ApiDir "nawel.db"
$BackupFile = Join-Path $ApiDir "nawel.db.backup_$(Get-Date -Format 'yyyyMMdd_HHmmss')"

# ============================================================================
# Functions
# ============================================================================

function Write-Header {
    param([string]$Message)
    Write-Host "`n========================================" -ForegroundColor Blue
    Write-Host $Message -ForegroundColor Blue
    Write-Host "========================================`n" -ForegroundColor Blue
}

function Write-Success {
    param([string]$Message)
    Write-Host "[OK] $Message" -ForegroundColor Green
}

function Write-ErrorMsg {
    param([string]$Message)
    Write-Host "[ERROR] $Message" -ForegroundColor Red
}

function Write-WarningMsg {
    param([string]$Message)
    Write-Host "[WARNING] $Message" -ForegroundColor Yellow
}

function Write-Info {
    param([string]$Message)
    Write-Host "[INFO] $Message" -ForegroundColor Cyan
}

function Test-Prerequisites {
    Write-Header "Checking Prerequisites"

    # Check if sqlite3 is installed or available locally
    $script:Sqlite3 = $null
    if (Test-Path ".\sqlite3.exe") {
        $script:Sqlite3 = ".\sqlite3.exe"
        Write-Success "sqlite3.exe found in current directory"
    }
    elseif (Get-Command sqlite3 -ErrorAction SilentlyContinue) {
        $script:Sqlite3 = "sqlite3"
        Write-Success "sqlite3 is installed in PATH"
    }
    else {
        Write-ErrorMsg "sqlite3 is not installed"
        Write-Host "Download from: https://www.sqlite.org/download.html"
        Write-Host "Add to PATH or place sqlite3.exe in the current directory"
        exit 1
    }

    # Check if old SQL file exists
    if (-not (Test-Path $OldSql)) {
        Write-ErrorMsg "Old database file not found: $OldSql"
        Write-Host "Expected path: old\nironico_nawel.sql (from project root)"
        exit 1
    }
    Write-Success "Old database file found"

    # Check if current database exists
    if (-not (Test-Path $DbFile)) {
        Write-WarningMsg "Database file not found: $DbFile"
        Write-Info "Creating new database..."
        New-Item -ItemType File -Path $DbFile -Force | Out-Null
    }
    Write-Success "Database file ready"
}

function New-Backup {
    Write-Header "Creating Backup"

    if ((Test-Path $DbFile) -and ((Get-Item $DbFile).Length -gt 0)) {
        Copy-Item $DbFile $BackupFile
        Write-Success "Backup created: $(Split-Path -Leaf $BackupFile)"
        Write-Host "  Location: $BackupFile" -ForegroundColor Gray
    }
    else {
        Write-Info "Database is empty, skipping backup"
    }
}

function Invoke-Migration {
    Write-Header "Running Migration"

    $TempDb = Join-Path $ApiDir "nawel_old_temp.db"

    Write-Info "Step 1/6: Creating temporary database..."
    if (Test-Path $TempDb) { Remove-Item $TempDb }
    Get-Content $OldSql | & $Sqlite3 $TempDb
    Write-Success "Temporary database created"

    Write-Info "Step 2/6: Migrating families..."
    @"
ATTACH DATABASE '$TempDb' AS old_db;
INSERT OR IGNORE INTO family (id, name, created_at)
SELECT id, name, datetime('now') FROM old_db.family;
DETACH DATABASE old_db;
"@ | & $Sqlite3 $DbFile
    $familyCount = & $Sqlite3 $DbFile "SELECT COUNT(*) FROM family;"
    Write-Success "Migrated $familyCount families"

    Write-Info "Step 3/6: Migrating users..."
    @"
ATTACH DATABASE '$TempDb' AS old_db;
INSERT OR IGNORE INTO user (
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
    CASE WHEN id = 1 THEN 1 ELSE 0 END,
    datetime('now'), datetime('now')
FROM old_db.user;
DETACH DATABASE old_db;
"@ | & $Sqlite3 $DbFile
    $userCount = & $Sqlite3 $DbFile "SELECT COUNT(*) FROM user;"
    Write-Success "Migrated $userCount users"

    Write-Info "Step 4/6: Migrating lists..."
    @"
ATTACH DATABASE '$TempDb' AS old_db;
INSERT OR IGNORE INTO lists (id, name, user_id, created_at, updated_at)
SELECT id, name, user_id, datetime('now'), datetime('now') FROM old_db.lists;
DETACH DATABASE old_db;
"@ | & $Sqlite3 $DbFile
    $listCount = & $Sqlite3 $DbFile "SELECT COUNT(*) FROM lists;"
    Write-Success "Migrated $listCount lists"

    Write-Info "Step 5/6: Migrating gifts..."
    @"
ATTACH DATABASE '$TempDb' AS old_db;
INSERT OR IGNORE INTO gifts (
    id, list_id, name, description, image, link, cost, currency,
    available, taken_by, is_group_gift, comment, year,
    created_at, updated_at
)
SELECT
    g.id, g.list_id, g.name, g.description, g.image, g.link,
    g.cost, g.currency, g.available, g.taken_by,
    CASE WHEN EXISTS (
        SELECT 1 FROM old_db.gift_participation gp
        WHERE gp.gift_id = g.id AND gp.is_active = 1
    ) THEN 1 ELSE 0 END,
    g.comment, g.year,
    datetime('now'), datetime('now')
FROM old_db.gifts g;
DETACH DATABASE old_db;
"@ | & $Sqlite3 $DbFile
    $giftCount = & $Sqlite3 $DbFile "SELECT COUNT(*) FROM gifts;"
    $groupGiftCount = & $Sqlite3 $DbFile "SELECT COUNT(*) FROM gifts WHERE is_group_gift = 1;"
    Write-Success "Migrated $giftCount gifts ($groupGiftCount group gifts)"

    Write-Info "Step 6/6: Migrating gift participations..."
    @"
ATTACH DATABASE '$TempDb' AS old_db;
INSERT OR IGNORE INTO gift_participation (id, gift_id, user_id, is_active, created_at)
SELECT id, gift_id, user_id, is_active, datetime('now') FROM old_db.gift_participation;
DETACH DATABASE old_db;
"@ | & $Sqlite3 $DbFile
    $participationCount = & $Sqlite3 $DbFile "SELECT COUNT(*) FROM gift_participation;"
    Write-Success "Migrated $participationCount participations"

    Write-Info "Updating auto-increment sequences..."
    @"
UPDATE sqlite_sequence SET seq = (SELECT IFNULL(MAX(id), 0) FROM family) WHERE name = 'family';
UPDATE sqlite_sequence SET seq = (SELECT IFNULL(MAX(id), 0) FROM user) WHERE name = 'user';
UPDATE sqlite_sequence SET seq = (SELECT IFNULL(MAX(id), 0) FROM lists) WHERE name = 'lists';
UPDATE sqlite_sequence SET seq = (SELECT IFNULL(MAX(id), 0) FROM gifts) WHERE name = 'gifts';
UPDATE sqlite_sequence SET seq = (SELECT IFNULL(MAX(id), 0) FROM gift_participation) WHERE name = 'gift_participation';
"@ | & $Sqlite3 $DbFile
    Write-Success "Auto-increment sequences updated"

    Write-Info "Cleaning up temporary files..."
    Remove-Item $TempDb -ErrorAction SilentlyContinue
    Write-Success "Cleanup complete"
}

function Copy-Avatars {
    Write-Header "Copying Avatar Files"

    $OldAvatarDir = Join-Path $ProjectRoot "old\uploads\avatars"
    $NewAvatarDir = Join-Path $ApiDir "uploads\avatars"

    if (-not (Test-Path $OldAvatarDir)) {
        Write-WarningMsg "Old avatar directory not found: $OldAvatarDir"
        Write-Info "Skipping avatar copy"
        return
    }

    if (-not (Test-Path $NewAvatarDir)) {
        New-Item -ItemType Directory -Path $NewAvatarDir -Force | Out-Null
    }

    $avatarFiles = Get-ChildItem $OldAvatarDir -File
    if ($avatarFiles.Count -eq 0) {
        Write-Info "No avatars to copy"
        return
    }

    Copy-Item "$OldAvatarDir\*" $NewAvatarDir -Force
    $copiedCount = (Get-ChildItem $NewAvatarDir -File).Count
    Write-Success "Copied $copiedCount avatar files"
}

function Invoke-Verification {
    Write-Header "Verification"

    Write-Info "Running verification queries..."

    # Count families
    $families = & $Sqlite3 $DbFile "SELECT COUNT(*) FROM family;"
    Write-Host "  Families: $families" -ForegroundColor Green

    # Count users
    $totalUsers = & $Sqlite3 $DbFile "SELECT COUNT(*) FROM user;"
    $md5Users = & $Sqlite3 $DbFile "SELECT COUNT(*) FROM user WHERE LENGTH(pwd) = 32 AND SUBSTR(pwd, 1, 2) != '`$2';"
    $adminUsers = & $Sqlite3 $DbFile "SELECT COUNT(*) FROM user WHERE is_admin = 1;"
    Write-Host "  Users: $totalUsers (MD5: $md5Users, Admins: $adminUsers)" -ForegroundColor Green

    # Count lists
    $lists = & $Sqlite3 $DbFile "SELECT COUNT(*) FROM lists;"
    Write-Host "  Lists: $lists" -ForegroundColor Green

    # Count gifts
    $totalGifts = & $Sqlite3 $DbFile "SELECT COUNT(*) FROM gifts;"
    $groupGifts = & $Sqlite3 $DbFile "SELECT COUNT(*) FROM gifts WHERE is_group_gift = 1;"
    $minYear = & $Sqlite3 $DbFile "SELECT MIN(year) FROM gifts;"
    $maxYear = & $Sqlite3 $DbFile "SELECT MAX(year) FROM gifts;"
    Write-Host "  Gifts: $totalGifts (Group: $groupGifts, Years: $minYear-$maxYear)" -ForegroundColor Green

    # Count participations
    $participations = & $Sqlite3 $DbFile "SELECT COUNT(*) FROM gift_participation;"
    Write-Host "  Participations: $participations" -ForegroundColor Green

    # Check for orphaned group gifts
    $orphaned = & $Sqlite3 $DbFile "SELECT COUNT(*) FROM gifts g WHERE g.is_group_gift = 1 AND NOT EXISTS (SELECT 1 FROM gift_participation gp WHERE gp.gift_id = g.id);"
    if ([int]$orphaned -gt 0) {
        Write-WarningMsg "$orphaned group gifts have no participations"
    }
    else {
        Write-Success "All group gifts have participations"
    }

    # Check admin user
    $adminLogin = & $Sqlite3 $DbFile "SELECT login FROM user WHERE is_admin = 1 LIMIT 1;"
    if ($adminLogin) {
        Write-Success "Admin user found: $adminLogin"
    }
    else {
        Write-WarningMsg "No admin user found"
    }
}

function Show-Help {
    Write-Host "Nawel Database Migration Script (PowerShell)"
    Write-Host ""
    Write-Host "Usage: .\migrate_old_to_new.ps1 [-BackupOnly] [-VerifyOnly] [-Help]"
    Write-Host ""
    Write-Host "Parameters:"
    Write-Host "  -BackupOnly    Create backup only without migration"
    Write-Host "  -VerifyOnly    Run verification queries only"
    Write-Host "  -Help          Show this help message"
    Write-Host ""
    Write-Host "This script migrates data from old/nironico_nawel.sql to the new database."
    Write-Host "It will automatically:"
    Write-Host "  - Create a backup of the current database"
    Write-Host "  - Migrate all tables (family, user, lists, gifts, gift_participation)"
    Write-Host "  - Copy avatar files"
    Write-Host "  - Run verification queries"
    Write-Host ""
    Write-Host "Prerequisites:"
    Write-Host "  - sqlite3 must be installed and in PATH"
    Write-Host "  - old/nironico_nawel.sql must exist"
    Write-Host "  - Current directory must be backend/Nawel.Api/"
}

function Show-NextSteps {
    Write-Header "Next Steps"

    Write-Host "Migration completed successfully!" -ForegroundColor Green
    Write-Host ""
    Write-Host "What to do next:"
    Write-Host ""
    Write-Host "1. Test Authentication" -ForegroundColor Blue
    Write-Host "   - Users with MD5 passwords will need to reset their passwords"
    Write-Host "   - The system will automatically guide them through the process"
    Write-Host "   - See MIGRATION_MD5_PLAN.md for details"
    Write-Host ""
    Write-Host "2. Verify Data" -ForegroundColor Blue
    Write-Host "   - Login to the application"
    Write-Host "   - Check that lists are visible"
    Write-Host "   - Verify avatars are displayed"
    Write-Host "   - Test gift reservations"
    Write-Host ""
    Write-Host "3. Admin Access" -ForegroundColor Blue
    Write-Host "   - Login as admin (user ID 1)"
    Write-Host "   - Verify admin panel access"
    Write-Host "   - Check family management"
    Write-Host ""
    Write-Host "4. Backup" -ForegroundColor Blue
    Write-Host "   - Backup file saved at:"
    Write-Host "     $BackupFile"
    Write-Host "   - Keep this backup safe!"
    Write-Host ""
    Write-Host "For troubleshooting, see: Migrations/MIGRATION_GUIDE.md"
}

# ============================================================================
# Main Script
# ============================================================================

try {
    Write-Header "Nawel Database Migration"

    # Handle parameters
    if ($Help) {
        Show-Help
        exit 0
    }

    if ($BackupOnly) {
        Test-Prerequisites
        New-Backup
        exit 0
    }

    if ($VerifyOnly) {
        Invoke-Verification
        exit 0
    }

    # Run full migration
    Test-Prerequisites
    New-Backup
    Invoke-Migration
    Copy-Avatars
    Invoke-Verification
    Show-NextSteps

    Write-Success "All done!"
}
catch {
    Write-Host "`nError during migration:" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    Write-Host ""
    Write-Host "If you encounter issues:" -ForegroundColor Yellow
    Write-Host "1. Restore the backup if needed: Copy-Item `$BackupFile nawel.db -Force" -ForegroundColor Yellow
    Write-Host "2. Check the prerequisites" -ForegroundColor Yellow
    Write-Host "3. See Migrations/MIGRATION_GUIDE.md for troubleshooting" -ForegroundColor Yellow
    exit 1
}
