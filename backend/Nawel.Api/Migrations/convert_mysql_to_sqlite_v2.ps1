# ============================================================================
# MySQL to SQLite Converter for Nawel Database v2
# ============================================================================

param(
    [string]$InputFile = "..\..\..\old\nironico_nawel_update.sql",
    [string]$OutputFile = "nawel_converted_v2.sql"
)

$ErrorActionPreference = "Stop"

Write-Host "Converting MySQL dump to SQLite format (v2)..." -ForegroundColor Cyan

# Read the file
$content = Get-Content $InputFile -Raw -Encoding UTF8

# Remove BOM if present
$content = $content -replace "^\uFEFF", ""

# Output array
$output = @()

# ============================================================================
# Process FAMILY table
# ============================================================================
Write-Host "Processing FAMILY table..." -ForegroundColor Yellow
if ($content -match "INSERT INTO ``family``[^;]+;") {
    $familyInsert = $matches[0]
    $familyInsert = $familyInsert -replace '`', ''
    # Add created_at column
    $familyInsert = $familyInsert -replace 'INSERT INTO family \(([^)]+)\) VALUES', 'INSERT INTO family ($1, created_at) VALUES'
    # Add created_at values to each row
    $familyInsert = $familyInsert -replace "(\(\d+, '[^']*'\)),", "`$1, datetime('now')),"
    $familyInsert = $familyInsert -replace "(\(\d+, '[^']*'\));", "`$1, datetime('now'));"
    $output += $familyInsert
    $output += ""
}

# ============================================================================
# Process USER table
# ============================================================================
Write-Host "Processing USER table..." -ForegroundColor Yellow
if ($content -match "INSERT INTO ``user``[^;]+;") {
    $userInsert = $matches[0]
    $userInsert = $userInsert -replace '`', ''

    # Split into header and values
    if ($userInsert -match '(INSERT INTO user \([^)]+\) VALUES)\s*(.+);') {
        $header = $matches[1]
        $values = $matches[2]

        # Add new columns to header
        $header = $header -replace '\) VALUES$', ', is_admin, created_at, updated_at) VALUES'

        # Process each value row
        $valueRows = @()
        $currentRow = ""
        $inString = $false
        $parenDepth = 0

        for ($i = 0; $i -lt $values.Length; $i++) {
            $char = $values[$i]
            $currentRow += $char

            if ($char -eq "'" -and $i -gt 0 -and $values[$i-1] -ne '\') {
                $inString = -not $inString
            }
            elseif (-not $inString) {
                if ($char -eq '(') { $parenDepth++ }
                elseif ($char -eq ')') {
                    $parenDepth--
                    if ($parenDepth -eq 0) {
                        $valueRows += $currentRow.Trim()
                        $currentRow = ""
                    }
                }
            }
        }

        # Add is_admin, timestamps to each row
        $processedRows = @()
        foreach ($row in $valueRows) {
            if ($row -match '^\((\d+),') {
                $userId = [int]$matches[1]
                $isAdmin = if ($userId -eq 1) { 1 } else { 0 }
                $row = $row.TrimEnd(',', ' ')
                $row = $row.TrimEnd(')')
                $row += ", $isAdmin, datetime('now'), datetime('now'))"
                $processedRows += $row
            }
        }

        $output += $header
        $output += ($processedRows -join ",`n") + ";"
        $output += ""
    }
}

# ============================================================================
# Process LISTS table
# ============================================================================
Write-Host "Processing LISTS table..." -ForegroundColor Yellow
if ($content -match "INSERT INTO ``lists``[^;]+;") {
    $listsInsert = $matches[0]
    $listsInsert = $listsInsert -replace '`', ''

    # Replace header (remove filename, add timestamps)
    $listsInsert = $listsInsert -replace 'INSERT INTO lists \(id, name, user_id, filename\) VALUES', 'INSERT INTO lists (id, name, user_id, created_at, updated_at) VALUES'

    # Process each row: remove filename value, add timestamps
    # Match pattern: (id, 'name', user_id, 'filename' or NULL)
    $listsInsert = $listsInsert -replace "(\(\d+, '[^']*', \d+), (?:'[^']*'|NULL)\)", "`$1, datetime('now'), datetime('now'))"

    $output += $listsInsert
    $output += ""
}

# ============================================================================
# Process GIFTS table
# ============================================================================
Write-Host "Processing GIFTS table..." -ForegroundColor Yellow
$giftPattern = "INSERT INTO ``gifts``[^;]+;"
$giftMatches = [regex]::Matches($content, $giftPattern, [System.Text.RegularExpressions.RegexOptions]::Singleline)

foreach ($match in $giftMatches) {
    $giftInsert = $match.Value
    $giftInsert = $giftInsert -replace '`', ''

    # Add created_at, updated_at to header
    $giftInsert = $giftInsert -replace '(INSERT INTO gifts \([^)]+)\) VALUES', '$1, created_at, updated_at) VALUES'

    # Add timestamps to each row
    $giftInsert = $giftInsert -replace "(\([^)]+)\)", "`$1, datetime('now'), datetime('now'))"

    $output += $giftInsert
    $output += ""
}

# ============================================================================
# Process GIFT_PARTICIPATION table
# ============================================================================
Write-Host "Processing GIFT_PARTICIPATION table..." -ForegroundColor Yellow
if ($content -match "INSERT INTO ``gift_participation``[^;]+;") {
    $partInsert = $matches[0]
    $partInsert = $partInsert -replace '`', ''

    # Add created_at to header
    $partInsert = $partInsert -replace '(INSERT INTO gift_participation \([^)]+)\) VALUES', '$1, created_at) VALUES'

    # Add timestamp to each row
    $partInsert = $partInsert -replace "(\([^)]+)\)", "`$1, datetime('now'))"

    $output += $partInsert
    $output += ""
}

# Write output
$output | Out-File $OutputFile -Encoding UTF8

Write-Host "`n[OK] Converted SQL saved to: $OutputFile" -ForegroundColor Green
Write-Host "Lines written: $($output.Count)" -ForegroundColor Gray
