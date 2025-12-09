# Simple Project Stats

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "   NAWEL PROJECT STATISTICS" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

# Frontend TSX
$tsxFiles = Get-ChildItem -Path "frontend\nawel-app\src" -Filter "*.tsx" -Recurse -File
$tsxLines = ($tsxFiles | ForEach-Object { (Get-Content $_.FullName).Count } | Measure-Object -Sum).Sum
Write-Host "Frontend TSX Files: $($tsxFiles.Count) files, $tsxLines lines" -ForegroundColor Green

# Frontend TS
$tsFiles = Get-ChildItem -Path "frontend\nawel-app\src" -Filter "*.ts" -Recurse -File | Where-Object { $_.Name -notlike "*.test.*" }
$tsLines = ($tsFiles | ForEach-Object { (Get-Content $_.FullName).Count } | Measure-Object -Sum).Sum
Write-Host "Frontend TS Files:  $($tsFiles.Count) files, $tsLines lines" -ForegroundColor Green

# Frontend CSS/LESS
$cssFiles = Get-ChildItem -Path "frontend\nawel-app\src" -Include "*.css","*.less" -Recurse -File
$cssLines = ($cssFiles | ForEach-Object { (Get-Content $_.FullName).Count } | Measure-Object -Sum).Sum
Write-Host "Frontend Styles:    $($cssFiles.Count) files, $cssLines lines" -ForegroundColor Green

$frontendTotal = $tsxLines + $tsLines + $cssLines
Write-Host "TOTAL FRONTEND:     $frontendTotal lines`n" -ForegroundColor Cyan

# Backend C#
$csFiles = Get-ChildItem -Path "backend\Nawel.Api" -Filter "*.cs" -Recurse -File | Where-Object { $_.FullName -notlike "*\bin\*" -and $_.FullName -notlike "*\obj\*" }
$csLines = ($csFiles | ForEach-Object { (Get-Content $_.FullName).Count } | Measure-Object -Sum).Sum
Write-Host "Backend C# Files:   $($csFiles.Count) files, $csLines lines" -ForegroundColor Yellow
Write-Host "TOTAL BACKEND:      $csLines lines`n" -ForegroundColor Cyan

# Tests Backend
$csTestFiles = Get-ChildItem -Path "backend\Nawel.Api.Tests" -Filter "*.cs" -Recurse -File -ErrorAction SilentlyContinue
$csTestLines = ($csTestFiles | ForEach-Object { (Get-Content $_.FullName).Count } | Measure-Object -Sum).Sum
Write-Host "Backend Tests:      $($csTestFiles.Count) files, $csTestLines lines" -ForegroundColor Magenta

# Tests Frontend
$tsTestFiles = Get-ChildItem -Path "frontend\nawel-app\src" -Include "*.test.ts","*.test.tsx" -Recurse -File
$tsTestLines = ($tsTestFiles | ForEach-Object { (Get-Content $_.FullName).Count } | Measure-Object -Sum).Sum
Write-Host "Frontend Tests:     $($tsTestFiles.Count) files, $tsTestLines lines" -ForegroundColor Magenta

$testTotal = $csTestLines + $tsTestLines
Write-Host "TOTAL TESTS:        $testTotal lines`n" -ForegroundColor Cyan

# Documentation
$mdFiles = Get-ChildItem -Path "." -Filter "*.md" -Recurse -File | Where-Object { $_.FullName -notlike "*node_modules*" }
$mdLines = ($mdFiles | ForEach-Object { (Get-Content $_.FullName).Count } | Measure-Object -Sum).Sum
Write-Host "Documentation:      $($mdFiles.Count) files, $mdLines lines" -ForegroundColor Blue

# SQL
$sqlFiles = Get-ChildItem -Path "backend\Nawel.Api\Migrations" -Filter "*.sql" -Recurse -File -ErrorAction SilentlyContinue
$sqlLines = ($sqlFiles | ForEach-Object { (Get-Content $_.FullName).Count } | Measure-Object -Sum).Sum
Write-Host "SQL Migrations:     $($sqlFiles.Count) files, $sqlLines lines" -ForegroundColor Blue

# Scripts
$scriptFiles = Get-ChildItem -Path "backend\Nawel.Api\Migrations" -Include "*.sh","*.ps1" -Recurse -File -ErrorAction SilentlyContinue
$scriptLines = ($scriptFiles | ForEach-Object { (Get-Content $_.FullName).Count } | Measure-Object -Sum).Sum
Write-Host "Migration Scripts:  $($scriptFiles.Count) files, $scriptLines lines" -ForegroundColor Blue

$docsTotal = $mdLines + $sqlLines + $scriptLines
Write-Host "TOTAL DOCS:         $docsTotal lines`n" -ForegroundColor Cyan

# Project Structure
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "PROJECT STRUCTURE" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

$controllers = (Get-ChildItem -Path "backend\Nawel.Api\Controllers" -Filter "*.cs" -File -ErrorAction SilentlyContinue).Count
$services = (Get-ChildItem -Path "backend\Nawel.Api\Services" -Recurse -Filter "*.cs" -File -ErrorAction SilentlyContinue).Count
$models = (Get-ChildItem -Path "backend\Nawel.Api\Models" -Filter "*.cs" -File -ErrorAction SilentlyContinue).Count
$dtos = (Get-ChildItem -Path "backend\Nawel.Api\DTOs" -Filter "*.cs" -File -ErrorAction SilentlyContinue).Count

Write-Host "Backend:" -ForegroundColor Yellow
Write-Host "  Controllers:      $controllers" -ForegroundColor White
Write-Host "  Services:         $services" -ForegroundColor White
Write-Host "  Models:           $models" -ForegroundColor White
Write-Host "  DTOs:             $dtos`n" -ForegroundColor White

$pages = (Get-ChildItem -Path "frontend\nawel-app\src\pages" -Filter "*.tsx" -File -ErrorAction SilentlyContinue).Count
$components = (Get-ChildItem -Path "frontend\nawel-app\src\components" -Recurse -Filter "*.tsx" -File -ErrorAction SilentlyContinue).Count
$contexts = (Get-ChildItem -Path "frontend\nawel-app\src\contexts" -Filter "*.tsx" -File -ErrorAction SilentlyContinue).Count

Write-Host "Frontend:" -ForegroundColor Green
Write-Host "  Pages:            $pages" -ForegroundColor White
Write-Host "  Components:       $components" -ForegroundColor White
Write-Host "  Contexts:         $contexts`n" -ForegroundColor White

# Summary
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "SUMMARY" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

$totalCode = $frontendTotal + $csLines + $testTotal
$grandTotal = $totalCode + $docsTotal

Write-Host "Total Code:         $totalCode lines" -ForegroundColor White
Write-Host "  - Frontend:       $frontendTotal lines" -ForegroundColor Gray
Write-Host "  - Backend:        $csLines lines" -ForegroundColor Gray
Write-Host "  - Tests:          $testTotal lines" -ForegroundColor Gray
Write-Host ""
Write-Host "Total Documentation: $docsTotal lines" -ForegroundColor White
Write-Host ""
Write-Host "GRAND TOTAL:        $grandTotal lines" -ForegroundColor Cyan

$totalFiles = $tsxFiles.Count + $tsFiles.Count + $cssFiles.Count + $csFiles.Count + $csTestFiles.Count + $tsTestFiles.Count + $mdFiles.Count + $sqlFiles.Count
Write-Host "Total Files:        $totalFiles files" -ForegroundColor White

$testRatio = [math]::Round(($testTotal / ($frontendTotal + $csLines) * 100), 1)
Write-Host ""
Write-Host "Test Coverage:      $testRatio%" -ForegroundColor Magenta

Write-Host "`n========================================`n" -ForegroundColor Cyan
