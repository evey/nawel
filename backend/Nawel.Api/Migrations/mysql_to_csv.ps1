# ============================================================================
# MySQL Dump to CSV Extractor for Nawel
# ============================================================================
# Extracts data from MySQL INSERT statements and saves as CSV files
# ============================================================================

param(
    [string]$MysqlDump = "..\..\..\old\nironico_nawel_update.sql",
    [string]$OutputDir = "csv_export"
)

$ErrorActionPreference = "Stop"

Write-Host "Extracting MySQL data to CSV..." -ForegroundColor Cyan
Write-Host ""

# Create output directory
if (-not (Test-Path $OutputDir)) {
    New-Item -ItemType Directory -Path $OutputDir | Out-Null
}

# Read MySQL dump
$content = Get-Content $MysqlDump -Raw -Encoding UTF8

# Function to parse INSERT VALUES
function Parse-InsertValues {
    param([string]$InsertStatement)

    $values = @()

    # Remove INSERT header to get only VALUES part
    if ($InsertStatement -match 'VALUES\s+(.+);?$') {
        $valuesText = $matches[1]
        $valuesText = $valuesText.TrimEnd(';')

        # Simple parsing: split by "),(" to get individual rows
        # This is a simplified approach - might need refinement for complex data
        $rows = $valuesText -split '\),\s*\('

        foreach ($row in $rows) {
            # Clean up parentheses
            $row = $row.Trim('(', ')', ' ', ',', ';')

            # Parse values (this is simplified - real parsing would need to handle quoted commas)
            $fields = @()
            $currentField = ""
            $inQuote = $false
            $escapeNext = $false

            for ($i = 0; $i -lt $row.Length; $i++) {
                $char = $row[$i]

                if ($escapeNext) {
                    $currentField += $char
                    $escapeNext = $false
                    continue
                }

                if ($char -eq '\') {
                    $escapeNext = $true
                    continue
                }

                if ($char -eq "'" -and -not $escapeNext) {
                    $inQuote = -not $inQuote
                    continue
                }

                if ($char -eq ',' -and -not $inQuote) {
                    $fields += $currentField.Trim()
                    $currentField = ""
                    continue
                }

                $currentField += $char
            }

            # Add last field
            if ($currentField) {
                $fields += $currentField.Trim()
            }

            if ($fields.Count -gt 0) {
                $values += ,@($fields)
            }
        }
    }

    return $values
}

# Export each table
$tables = @{
    'family' = @('id', 'name')
    'user' = @('id', 'login', 'pwd', 'email', 'first_name', 'last_name', 'avatar', 'pseudo', 'notify_list_edit', 'notify_gift_taken', 'display_popup', 'reset_token', 'token_expiry', 'isChildren', 'family_id')
    'lists' = @('id', 'name', 'user_id', 'filename')
    'gifts' = @('id', 'list_id', 'name', 'description', 'image', 'link', 'cost', 'currency', 'available', 'taken_by', 'comment', 'year')
    'gift_participation' = @('id', 'gift_id', 'user_id', 'is_active')
}

foreach ($table in $tables.Keys) {
    Write-Host "Extracting table: $table" -ForegroundColor Yellow

    $pattern = "INSERT INTO ``$table``[^;]+;"
    $matches = [regex]::Matches($content, $pattern, [System.Text.RegularExpressions.RegexOptions]::Singleline)

    $csvFile = Join-Path $OutputDir "$table.csv"
    $csvLines = @()

    # Add header
    $csvLines += ($tables[$table] -join ',')

    $rowCount = 0
    foreach ($match in $matches) {
        $insert = $match.Value
        $rows = Parse-InsertValues $insert

        foreach ($row in $rows) {
            # Clean and escape values for CSV
            $csvRow = $row | ForEach-Object {
                $val = $_
                # Remove quotes
                $val = $val.Trim("'", '"', ' ')
                # Handle NULL
                if ($val -eq 'NULL') {
                    $val = ''
                }
                # Escape double quotes for CSV
                $val = $val -replace '"', '""'
                # Wrap in quotes if contains comma or newline
                if ($val -match '[,\n\r"]') {
                    $val = "`"$val`""
                }
                $val
            }

            $csvLines += ($csvRow -join ',')
            $rowCount++
        }
    }

    # Write CSV
    $Utf8NoBomEncoding = New-Object System.Text.UTF8Encoding $False
    [System.IO.File]::WriteAllLines($csvFile, $csvLines, $Utf8NoBomEncoding)

    Write-Host "  [OK] Exported $rowCount rows to $table.csv" -ForegroundColor Green
}

Write-Host "`n[OK] CSV export complete!" -ForegroundColor Green
Write-Host "Files saved in: $OutputDir" -ForegroundColor Gray
