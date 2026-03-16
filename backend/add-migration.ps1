# Script pour generer les migrations SQLite ET MySQL
# Usage: .\add-migration.ps1 -Name "MaMigration"

param(
    [Parameter(Mandatory=$true)]
    [string]$Name
)

$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Generation de la migration: $Name" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# Sauvegarder le NawelDbContextFactory actuel
$factoryPath = "Nawel.Api\Data\NawelDbContextFactory.cs"
$factoryBackup = Get-Content $factoryPath -Raw

# Template SQLite
$sqliteFactory = @'
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Nawel.Api.Data;

public class NawelDbContextFactory : IDesignTimeDbContextFactory<NawelDbContext>
{
    public NawelDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<NawelDbContext>();
        var connectionString = "Data Source=nawel.db";
        optionsBuilder.UseSqlite(connectionString);
        return new NawelDbContext(optionsBuilder.Options);
    }
}
'@

# Template MySQL
$mysqlFactory = @'
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Nawel.Api.Data;

public class NawelDbContextFactory : IDesignTimeDbContextFactory<NawelDbContext>
{
    public NawelDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<NawelDbContext>();
        var connectionString = "Server=localhost;Port=3306;Database=nawel;User=root;Password=dummy;";
        var serverVersion = new MySqlServerVersion(new Version(8, 0, 0));
        optionsBuilder.UseMySql(connectionString, serverVersion);
        return new NawelDbContext(optionsBuilder.Options);
    }
}
'@

try {
    # Creer les dossiers de backup
    $backupDir = "Nawel.Api\Migrations\_backup"
    $sqliteBackup = "$backupDir\SQLite"
    $mysqlBackup = "$backupDir\MySQL"
    New-Item -ItemType Directory -Force -Path $sqliteBackup | Out-Null
    New-Item -ItemType Directory -Force -Path $mysqlBackup | Out-Null

    $snapshotPath = "Nawel.Api\Migrations\NawelDbContextModelSnapshot.cs"
    $sqliteSnapshotBackup = "$sqliteBackup\NawelDbContextModelSnapshot.cs"
    $mysqlSnapshotBackup = "$mysqlBackup\NawelDbContextModelSnapshot.cs"

    # 1. Generer migration SQLite
    Write-Host "`n[1/6] Configuration pour SQLite..." -ForegroundColor Yellow
    Set-Content -Path $factoryPath -Value $sqliteFactory

    # Restaurer le snapshot SQLite s'il existe
    if (Test-Path $sqliteSnapshotBackup) {
        Write-Host "    Restauration du snapshot SQLite..." -ForegroundColor Gray
        Copy-Item $sqliteSnapshotBackup $snapshotPath -Force
    }

    Write-Host "[2/6] Generation migration SQLite..." -ForegroundColor Yellow
    Push-Location "Nawel.Api"
    dotnet ef migrations add $Name --context NawelDbContext
    if ($LASTEXITCODE -ne 0) {
        Pop-Location
        throw "Erreur generation SQLite"
    }
    Pop-Location

    # Sauvegarder les fichiers SQLite
    Get-ChildItem "Nawel.Api\Migrations\*$Name*.cs" | Move-Item -Destination $sqliteBackup -Force
    Copy-Item $snapshotPath $sqliteSnapshotBackup -Force

    # 2. Generer migration MySQL
    Write-Host "`n[3/6] Configuration pour MySQL..." -ForegroundColor Yellow
    Set-Content -Path $factoryPath -Value $mysqlFactory

    # Restaurer le snapshot MySQL s'il existe
    if (Test-Path $mysqlSnapshotBackup) {
        Write-Host "    Restauration du snapshot MySQL..." -ForegroundColor Gray
        Copy-Item $mysqlSnapshotBackup $snapshotPath -Force
    } else {
        Write-Host "    ATTENTION: Pas de snapshot MySQL trouve, utilisation du snapshot SQLite" -ForegroundColor Yellow
        Write-Host "    Cette migration MySQL contiendra probablement des AlterColumn..." -ForegroundColor Yellow
    }

    Write-Host "[4/6] Generation migration MySQL..." -ForegroundColor Yellow
    Push-Location "Nawel.Api"
    dotnet ef migrations add $Name --context NawelDbContext
    if ($LASTEXITCODE -ne 0) {
        Pop-Location
        throw "Erreur generation MySQL"
    }
    Pop-Location

    # Sauvegarder les fichiers MySQL
    Get-ChildItem "Nawel.Api\Migrations\*$Name*.cs" | Move-Item -Destination $mysqlBackup -Force
    Copy-Item $snapshotPath $mysqlSnapshotBackup -Force

    # 3. Basculer vers SQLite par defaut (dev)
    Write-Host "`n[5/6] Activation migrations SQLite (dev)..." -ForegroundColor Yellow
    Get-ChildItem "Nawel.Api\Migrations\*$Name*.cs" -ErrorAction SilentlyContinue | Remove-Item -Force
    Copy-Item "$sqliteBackup\*$Name*.cs" "Nawel.Api\Migrations\" -Force

    Write-Host "[6/6] Restauration du snapshot SQLite..." -ForegroundColor Yellow
    Copy-Item $sqliteSnapshotBackup $snapshotPath -Force

    Write-Host "`n========================================" -ForegroundColor Green
    Write-Host "Migrations generees avec succes!" -ForegroundColor Green
    Write-Host "  - SQLite: Nawel.Api\Migrations\_backup\SQLite\" -ForegroundColor Green
    Write-Host "  - MySQL:  Nawel.Api\Migrations\_backup\MySQL\" -ForegroundColor Green
    Write-Host "  - Active: SQLite (dev)" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green

} catch {
    Write-Host "`nErreur: $_" -ForegroundColor Red
    exit 1
} finally {
    # Restaurer le factory original
    Write-Host "`nRestauration du NawelDbContextFactory..." -ForegroundColor Gray
    Set-Content -Path $factoryPath -Value $factoryBackup
}

Write-Host "`nProchaines etapes:" -ForegroundColor Cyan
Write-Host "  1. Verifier les migrations generees" -ForegroundColor White
Write-Host "  2. Modifier manuellement si besoin (NOW, types, etc.)" -ForegroundColor White
Write-Host "  3. Tester en dev avec SQLite" -ForegroundColor White
Write-Host "  4. Deployer en prod avec MySQL" -ForegroundColor White
