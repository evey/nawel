# Project Statistics Analyzer
param([switch]$Detailed)

$ErrorActionPreference = "SilentlyContinue"

function Count-Lines {
    param($Path, $Pattern)
    $files = Get-ChildItem -Path $Path -Include $Pattern -Recurse -File
    $totalLines = 0
    $totalFiles = 0
    $codeLines = 0
    $commentLines = 0
    $blankLines = 0
    
    foreach ($file in $files) {
        $totalFiles++
        $content = Get-Content $file.FullName
        $totalLines += $content.Count
        
        foreach ($line in $content) {
            $trimmed = $line.Trim()
            if ($trimmed -eq "") {
                $blankLines++
            }
            elseif ($trimmed.StartsWith("//") -or $trimmed.StartsWith("#") -or $trimmed.StartsWith("<!--")) {
                $commentLines++
            }
            else {
                $codeLines++
            }
        }
    }
    
    return @{
        Files = $totalFiles
        Total = $totalLines
        Code = $codeLines
        Comments = $commentLines
        Blank = $blankLines
    }
}

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "       NAWEL PROJECT STATISTICS         " -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

# Frontend Analysis
Write-Host "📱 FRONTEND (React + TypeScript)" -ForegroundColor Green
Write-Host "─────────────────────────────────────" -ForegroundColor Gray

$tsxFiles = Count-Lines -Path "frontend\nawel-app\src" -Pattern "*.tsx"
Write-Host "  TypeScript Components (.tsx):" -ForegroundColor Yellow
Write-Host "    Files:    $($tsxFiles.Files)" -ForegroundColor White
Write-Host "    Lines:    $($tsxFiles.Total) (Code: $($tsxFiles.Code), Comments: $($tsxFiles.Comments), Blank: $($tsxFiles.Blank))" -ForegroundColor White

$tsFiles = Count-Lines -Path "frontend\nawel-app\src" -Pattern "*.ts"
Write-Host "`n  TypeScript Files (.ts):" -ForegroundColor Yellow
Write-Host "    Files:    $($tsFiles.Files)" -ForegroundColor White
Write-Host "    Lines:    $($tsFiles.Total) (Code: $($tsFiles.Code), Comments: $($tsFiles.Comments), Blank: $($tsFiles.Blank))" -ForegroundColor White

$cssFiles = Count-Lines -Path "frontend\nawel-app\src" -Pattern "*.less","*.css"
Write-Host "`n  Styles (.less/.css):" -ForegroundColor Yellow
Write-Host "    Files:    $($cssFiles.Files)" -ForegroundColor White
Write-Host "    Lines:    $($cssFiles.Total)" -ForegroundColor White

$frontendTotal = $tsxFiles.Total + $tsFiles.Total + $cssFiles.Total
Write-Host "`n  TOTAL FRONTEND: $frontendTotal lines" -ForegroundColor Cyan

# Backend Analysis
Write-Host "`n`n⚙️  BACKEND (ASP.NET Core 9)" -ForegroundColor Green
Write-Host "─────────────────────────────────────" -ForegroundColor Gray

$csFiles = Count-Lines -Path "backend\Nawel.Api" -Pattern "*.cs"
Write-Host "  C# Source Files (.cs):" -ForegroundColor Yellow
Write-Host "    Files:    $($csFiles.Files)" -ForegroundColor White
Write-Host "    Lines:    $($csFiles.Total) (Code: $($csFiles.Code), Comments: $($csFiles.Comments), Blank: $($csFiles.Blank))" -ForegroundColor White

# Tests
Write-Host "`n`n🧪 TESTS" -ForegroundColor Green
Write-Host "─────────────────────────────────────" -ForegroundColor Gray

$csTestFiles = Count-Lines -Path "backend\Nawel.Api.Tests" -Pattern "*.cs"
Write-Host "  Backend Tests (.cs):" -ForegroundColor Yellow
Write-Host "    Files:    $($csTestFiles.Files)" -ForegroundColor White
Write-Host "    Lines:    $($csTestFiles.Total)" -ForegroundColor White

$tsTestFiles = Count-Lines -Path "frontend\nawel-app\src" -Pattern "*.test.ts","*.test.tsx"
Write-Host "`n  Frontend Tests (.test.ts/.tsx):" -ForegroundColor Yellow
Write-Host "    Files:    $($tsTestFiles.Files)" -ForegroundColor White
Write-Host "    Lines:    $($tsTestFiles.Total)" -ForegroundColor White

$testTotal = $csTestFiles.Total + $tsTestFiles.Total
Write-Host "`n  TOTAL TESTS: $testTotal lines" -ForegroundColor Cyan

# Documentation
Write-Host "`n`n📚 DOCUMENTATION" -ForegroundColor Green
Write-Host "─────────────────────────────────────" -ForegroundColor Gray

$mdFiles = Count-Lines -Path "." -Pattern "*.md"
Write-Host "  Markdown Files (.md):" -ForegroundColor Yellow
Write-Host "    Files:    $($mdFiles.Files)" -ForegroundColor White
Write-Host "    Lines:    $($mdFiles.Total)" -ForegroundColor White

# SQL & Migrations
Write-Host "`n`n💾 DATABASE & MIGRATIONS" -ForegroundColor Green
Write-Host "─────────────────────────────────────" -ForegroundColor Gray

$sqlFiles = Count-Lines -Path "backend\Nawel.Api\Migrations" -Pattern "*.sql"
Write-Host "  SQL Scripts:" -ForegroundColor Yellow
Write-Host "    Files:    $($sqlFiles.Files)" -ForegroundColor White
Write-Host "    Lines:    $($sqlFiles.Total)" -ForegroundColor White

$shFiles = (Get-ChildItem -Path "backend\Nawel.Api\Migrations" -Include "*.sh","*.ps1" -Recurse -File).Count
Write-Host "`n  Migration Scripts (.sh/.ps1): $shFiles files" -ForegroundColor Yellow

# Configuration
Write-Host "`n`n⚙️  CONFIGURATION" -ForegroundColor Green
Write-Host "─────────────────────────────────────" -ForegroundColor Gray

$configFiles = Get-ChildItem -Path "." -Include "*.json","*.yaml","*.yml","*.config" -Recurse -File | Where-Object { $_.FullName -notlike "*node_modules*" -and $_.FullName -notlike "*bin*" -and $_.FullName -notlike "*obj*" }
Write-Host "  Config Files: $($configFiles.Count) files" -ForegroundColor Yellow

# Summary
Write-Host "`n`n📊 SUMMARY" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan

$totalCode = $frontendTotal + $csFiles.Total + $testTotal
$totalDocs = $mdFiles.Total + $sqlFiles.Total

Write-Host "  Total Code:          $totalCode lines" -ForegroundColor White
Write-Host "    - Frontend:        $frontendTotal lines" -ForegroundColor Gray
Write-Host "    - Backend:         $($csFiles.Total) lines" -ForegroundColor Gray
Write-Host "    - Tests:           $testTotal lines" -ForegroundColor Gray
Write-Host "`n  Total Documentation: $totalDocs lines" -ForegroundColor White
Write-Host "`n  GRAND TOTAL:         $($totalCode + $totalDocs) lines" -ForegroundColor Cyan

$totalFiles = $tsxFiles.Files + $tsFiles.Files + $cssFiles.Files + $csFiles.Files + $csTestFiles.Files + $tsTestFiles.Files + $mdFiles.Files + $sqlFiles.Files
Write-Host "`n  Total Files:         $totalFiles files" -ForegroundColor White

Write-Host "`n========================================`n" -ForegroundColor Cyan
