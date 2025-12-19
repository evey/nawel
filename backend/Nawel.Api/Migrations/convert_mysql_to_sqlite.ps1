# ============================================================================
# MySQL to SQLite Converter for Nawel Database
# ============================================================================
# This script converts the MySQL dump to SQLite-compatible format
# ============================================================================

param(
    [string]$InputFile = "..\..\..\old\nironico_nawel_update.sql",
    [string]$OutputFile = "nawel_converted.sql"
)

$ErrorActionPreference = "Stop"

Write-Host "Converting MySQL dump to SQLite format..." -ForegroundColor Cyan

# Read the entire file
$content = Get-Content $InputFile -Raw -Encoding UTF8

# Tables we want to migrate
$tables = @('family', 'user', 'lists', 'gifts', 'gift_participation')

# Output file
$output = @()

foreach ($table in $tables) {
    Write-Host "Processing table: $table" -ForegroundColor Yellow

    # Extract INSERT statements for this table
    $pattern = "INSERT INTO ``$table``[^;]+;"
    $matches = [regex]::Matches($content, $pattern, [System.Text.RegularExpressions.RegexOptions]::Singleline)

    Write-Host "  Found $($matches.Count) INSERT statement(s)" -ForegroundColor Gray

    foreach ($match in $matches) {
        $insert = $match.Value

        # Remove backticks
        $insert = $insert -replace '`', ''

        # Convert MySQL escape sequences
        $insert = $insert -replace "\\r\\n", "`n"
        $insert = $insert -replace "\\n", "`n"
        $insert = $insert -replace "\\'", "''"
        $insert = $insert -replace '\\\\', '\'

        # Table-specific adjustments
        switch ($table) {
            'user' {
                # Add is_admin, created_at, updated_at columns
                $insert = $insert -replace 'INSERT INTO user \(([^)]+)\) VALUES', 'INSERT INTO user ($1, is_admin, created_at, updated_at) VALUES'
                # Add values: is_admin=1 for id=1, 0 otherwise, and timestamps
                $insert = $insert -replace '\(1,', "(1," # Start of first record
                $insert = $insert -replace '(\(1,[^)]+)\);', '$1, 1, datetime(''now''), datetime(''now''));' # id=1 gets is_admin=1
                $insert = $insert -replace '(\(\d+,[^)]+)(?<!\, 1, datetime\(''now''\), datetime\(''now''\))\);', '$1, 0, datetime(''now''), datetime(''now''));' # Others get 0
            }
            'lists' {
                # Remove filename column and add created_at, updated_at
                $insert = $insert -replace 'INSERT INTO lists \(id, name, user_id, filename\) VALUES', 'INSERT INTO lists (id, name, user_id, created_at, updated_at) VALUES'
                # Remove filename values and add timestamps
                $insert = $insert -replace '(\d+, ''[^'']*'', \d+), [^,)]+\)', '$1, datetime(''now''), datetime(''now''))'
            }
            'gifts' {
                # Add created_at, updated_at columns
                $insert = $insert -replace 'INSERT INTO gifts \(([^)]+)\) VALUES', 'INSERT INTO gifts ($1, created_at, updated_at) VALUES'
                $insert = $insert -replace '(\([^)]+)\);', '$1, datetime(''now''), datetime(''now''));'
            }
            'gift_participation' {
                # Add created_at column
                $insert = $insert -replace 'INSERT INTO gift_participation \(([^)]+)\) VALUES', 'INSERT INTO gift_participation ($1, created_at) VALUES'
                $insert = $insert -replace '(\([^)]+)\);', '$1, datetime(''now''));'
            }
        }

        $output += $insert
        $output += ""
    }
}

# Write output file
$output | Out-File $OutputFile -Encoding UTF8

Write-Host "`n[OK] Converted SQL saved to: $OutputFile" -ForegroundColor Green
Write-Host "Lines written: $($output.Count)" -ForegroundColor Gray
