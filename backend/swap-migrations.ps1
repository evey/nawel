# Script pour basculer manuellement entre les migrations SQLite et MySQL
# Usage: .\swap-migrations.ps1 -Provider SQLite
#        .\swap-migrations.ps1 -Provider MySQL

param(
    [Parameter(Mandatory=$true)]
    [ValidateSet("SQLite", "MySQL")]
    [string]$Provider
)

$ErrorActionPreference = "Stop"

Write-Host "==================================================" -ForegroundColor Cyan
Write-Host "Basculement vers les migrations $Provider" -ForegroundColor Cyan
Write-Host "==================================================" -ForegroundColor Cyan
Write-Host ""

$apiPath = "Nawel.Api"
$migrationsPath = "$apiPath\Migrations"
$backupPath = "$migrationsPath\_backup"
$sqliteBackup = "$backupPath\SQLite"
$mysqlBackup = "$backupPath\MySQL"

# Verifier que les dossiers de backup existent
if (!(Test-Path $sqliteBackup)) {
    Write-Host "Erreur : Dossier $sqliteBackup introuvable" -ForegroundColor Red
    Write-Host "   Utilisez .\add-migration.ps1 pour generer les migrations" -ForegroundColor Yellow
    exit 1
}

if (!(Test-Path $mysqlBackup)) {
    Write-Host "Erreur : Dossier $mysqlBackup introuvable" -ForegroundColor Red
    Write-Host "   Utilisez .\add-migration.ps1 pour generer les migrations" -ForegroundColor Yellow
    exit 1
}

# Detecter le provider actuel
$currentProvider = "Unknown"
if (Test-Path "$migrationsPath\*.cs") {
    $snapshot = Get-Content "$migrationsPath\NawelDbContextModelSnapshot.cs" -Raw -ErrorAction SilentlyContinue
    if ($snapshot) {
        if ($snapshot -match "UseSqlite") {
            $currentProvider = "SQLite"
        } elseif ($snapshot -match "UseMySql") {
            $currentProvider = "MySQL"
        }
    }
}

Write-Host "Provider actuel : $currentProvider" -ForegroundColor Cyan
Write-Host "Provider cible  : $Provider" -ForegroundColor Cyan
Write-Host ""

if ($currentProvider -eq $Provider) {
    Write-Host "Les migrations $Provider sont deja actives" -ForegroundColor Green
    exit 0
}

try {
    # Sauvegarder les migrations actuelles si elles existent
    if ($currentProvider -ne "Unknown" -and (Test-Path "$migrationsPath\*.cs")) {
        $currentBackup = if ($currentProvider -eq "SQLite") { $sqliteBackup } else { $mysqlBackup }
        Write-Host "Sauvegarde des migrations $currentProvider..." -ForegroundColor Yellow
        Get-ChildItem "$migrationsPath\*.cs" | Copy-Item -Destination $currentBackup -Force
        Write-Host "Migrations $currentProvider sauvegardees" -ForegroundColor Green
        Write-Host ""
    }

    # Nettoyer le dossier Migrations
    Write-Host "Nettoyage du dossier Migrations..." -ForegroundColor Yellow
    Remove-Item "$migrationsPath\*.cs" -Force -ErrorAction SilentlyContinue

    # Copier les migrations du provider cible
    $targetBackup = if ($Provider -eq "SQLite") { $sqliteBackup } else { $mysqlBackup }
    Write-Host "Activation des migrations $Provider..." -ForegroundColor Yellow

    $files = Get-ChildItem "$targetBackup\*.cs"
    if ($files.Count -eq 0) {
        Write-Host "Erreur : Aucune migration trouvee dans $targetBackup" -ForegroundColor Red
        Write-Host "   Utilisez .\add-migration.ps1 pour generer les migrations" -ForegroundColor Yellow
        exit 1
    }

    $files | Copy-Item -Destination $migrationsPath -Force

    Write-Host "Migrations $Provider activees" -ForegroundColor Green
    Write-Host ""

    Write-Host "==================================================" -ForegroundColor Green
    Write-Host "Basculement termine avec succes !" -ForegroundColor Green
    Write-Host "==================================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "Migrations actives : $Provider" -ForegroundColor Cyan
    Write-Host "Localisation : $migrationsPath" -ForegroundColor Cyan
    Write-Host ""

    if ($Provider -eq "SQLite") {
        Write-Host "Vous pouvez maintenant developper en local avec SQLite" -ForegroundColor Yellow
    } else {
        Write-Host "Vous pouvez maintenant tester avec MySQL" -ForegroundColor Yellow
    }
    Write-Host ""

} catch {
    Write-Host ""
    Write-Host "Erreur lors du basculement : $_" -ForegroundColor Red
    exit 1
}
