# ============================================================================
# Import complet des gifts et gift_participation depuis phpMyAdmin export
# ============================================================================

param(
    [string]$FullCsv = "csv_export\gifts_full.csv",
    [string]$Sqlite3 = "..\sqlite3.exe",
    [string]$DbFile = "..\nawel.db"
)

$ErrorActionPreference = "Stop"

Write-Host "Importing gifts and gift_participation from phpMyAdmin export..." -ForegroundColor Cyan
Write-Host ""

# ============================================================================
# Step 1: Split the combined CSV into two files
# ============================================================================
Write-Host "Step 1: Splitting combined CSV..." -ForegroundColor Yellow

$giftsCsv = "csv_export\gifts_phpma.csv"
$partCsv = "csv_export\gift_participation_phpma.csv"

# Find the line number where gift_participation starts
$lineNum = 0
$splitLine = 0
Get-Content $FullCsv | ForEach-Object {
    $lineNum++
    if ($_ -match '"id","gift_id","user_id","is_active"') {
        $splitLine = $lineNum
    }
}

Write-Host "  Found gift_participation header at line: $splitLine" -ForegroundColor Gray

# Split the files
$allLines = Get-Content $FullCsv
$giftsLines = $allLines[0..($splitLine-2)]
$partLines = $allLines[($splitLine-1)..($allLines.Length-1)]

$Utf8NoBomEncoding = New-Object System.Text.UTF8Encoding $False
[System.IO.File]::WriteAllLines($giftsCsv, $giftsLines, $Utf8NoBomEncoding)
[System.IO.File]::WriteAllLines($partCsv, $partLines, $Utf8NoBomEncoding)

Write-Host "  [OK] Created $giftsCsv ($($giftsLines.Count) lines)" -ForegroundColor Green
Write-Host "  [OK] Created $partCsv ($($partLines.Count) lines)" -ForegroundColor Green

# ============================================================================
# Step 2: Backup current database
# ============================================================================
Write-Host "`nStep 2: Creating backup..." -ForegroundColor Yellow

$BackupFile = "nawel.db.backup_gifts_$(Get-Date -Format 'yyyyMMdd_HHmmss')"
Copy-Item $DbFile $BackupFile
Write-Host "  [OK] Backup created: $BackupFile" -ForegroundColor Green

# ============================================================================
# Step 3: Clear existing gifts data
# ============================================================================
Write-Host "`nStep 3: Clearing existing gifts data..." -ForegroundColor Yellow

@"
DELETE FROM gift_participation;
DELETE FROM gifts;
"@ | & $Sqlite3 $DbFile

Write-Host "  [OK] Tables cleared" -ForegroundColor Green

# ============================================================================
# Step 4: Import gifts from CSV
# ============================================================================
Write-Host "`nStep 4: Importing gifts..." -ForegroundColor Yellow

$importSql = @"
.mode csv
.import $giftsCsv temp_gifts_import

-- Insert with proper schema mapping
INSERT INTO gifts (
    id, list_id, name, description, image, link, cost, currency,
    available, taken_by, comment, year, is_group_gift,
    created_at, updated_at
)
SELECT
    CAST(id AS INTEGER),
    CAST(list_id AS INTEGER),
    name,
    NULLIF(description, ''),
    NULLIF(image, ''),
    NULLIF(link, ''),
    CASE WHEN cost = '' OR cost IS NULL THEN NULL ELSE CAST(cost AS REAL) END,
    NULLIF(currency, ''),
    CAST(available AS INTEGER),
    CASE WHEN taken_by = '' OR taken_by IS NULL THEN NULL ELSE CAST(taken_by AS INTEGER) END,
    NULLIF(comment, ''),
    CAST(year AS INTEGER),
    0,
    datetime('now'),
    datetime('now')
FROM temp_gifts_import
WHERE id != 'id';

DROP TABLE temp_gifts_import;
"@

$importSql | & $Sqlite3 $DbFile

$giftCount = & $Sqlite3 $DbFile "SELECT COUNT(*) FROM gifts;"
Write-Host "  [OK] Imported $giftCount gifts" -ForegroundColor Green

# ============================================================================
# Step 5: Import gift_participation from CSV
# ============================================================================
Write-Host "`nStep 5: Importing gift participations..." -ForegroundColor Yellow

$partImportSql = @"
.mode csv
.import $partCsv temp_part_import

INSERT INTO gift_participation (id, gift_id, user_id, is_active, created_at)
SELECT
    CAST(id AS INTEGER),
    CAST(gift_id AS INTEGER),
    CAST(user_id AS INTEGER),
    CAST(is_active AS INTEGER),
    datetime('now')
FROM temp_part_import
WHERE id != 'id';

DROP TABLE temp_part_import;
"@

$partImportSql | & $Sqlite3 $DbFile

$partCount = & $Sqlite3 $DbFile "SELECT COUNT(*) FROM gift_participation;"
Write-Host "  [OK] Imported $partCount participations" -ForegroundColor Green

# ============================================================================
# Step 6: Update is_group_gift flags
# ============================================================================
Write-Host "`nStep 6: Updating group gift flags..." -ForegroundColor Yellow

@"
UPDATE gifts SET is_group_gift = 1 WHERE id IN (
    SELECT DISTINCT gift_id FROM gift_participation WHERE is_active = 1
);
"@ | & $Sqlite3 $DbFile

$groupGiftCount = & $Sqlite3 $DbFile "SELECT COUNT(*) FROM gifts WHERE is_group_gift = 1;"
Write-Host "  [OK] Marked $groupGiftCount gifts as group gifts" -ForegroundColor Green

# ============================================================================
# Step 7: Verification
# ============================================================================
Write-Host "`nStep 7: Verification..." -ForegroundColor Yellow

$stats = & $Sqlite3 $DbFile @"
SELECT
    'Total gifts: ' || COUNT(*) || '
    - With cost: ' || SUM(CASE WHEN cost IS NOT NULL THEN 1 ELSE 0 END) || '
    - Without cost: ' || SUM(CASE WHEN cost IS NULL THEN 1 ELSE 0 END) || '
    - Group gifts: ' || SUM(CASE WHEN is_group_gift = 1 THEN 1 ELSE 0 END) || '
    - Years: ' || MIN(year) || '-' || MAX(year) || '
    - Participations: ' || (SELECT COUNT(*) FROM gift_participation)
FROM gifts;
"@

Write-Host $stats -ForegroundColor Green

# ============================================================================
# Step 8: Update EF migrations history for gifts
# ============================================================================
Write-Host "`nStep 8: Ensuring EF migrations are synced..." -ForegroundColor Yellow

@"
INSERT OR IGNORE INTO __EFMigrationsHistory (MigrationId, ProductVersion)
VALUES ('20251218122111_InitialCreate', '9.0.0');
"@ | & $Sqlite3 $DbFile

Write-Host "  [OK] Migration history updated" -ForegroundColor Green

Write-Host "`n===============================================" -ForegroundColor Cyan
Write-Host "Import completed successfully!" -ForegroundColor Green
Write-Host "===============================================" -ForegroundColor Cyan
Write-Host "Backup saved at: $BackupFile" -ForegroundColor Gray
