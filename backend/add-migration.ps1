# Script pour generer automatiquement les migrations SQLite ET MySQL
# Usage: .\add-migration.ps1 -Name "MyMigrationName"

param(
    [Parameter(Mandatory=$true)]
    [string]$Name
)

$ErrorActionPreference = "Stop"

Write-Host "==================================================" -ForegroundColor Cyan
Write-Host "Generation automatique des migrations SQLite et MySQL" -ForegroundColor Cyan
Write-Host "==================================================" -ForegroundColor Cyan
Write-Host ""

$apiPath = "Nawel.Api"
$migrationsPath = "$apiPath\Migrations"
$backupPath = "$migrationsPath\_backup"
$sqliteBackup = "$backupPath\SQLite"
$mysqlBackup = "$backupPath\MySQL"
$factoryPath = "$apiPath\Data\NawelDbContextFactory.cs"

# Creer les dossiers de backup si necessaire
if (!(Test-Path $backupPath)) {
    New-Item -ItemType Directory -Path $backupPath | Out-Null
}
if (!(Test-Path $sqliteBackup)) {
    New-Item -ItemType Directory -Path $sqliteBackup | Out-Null
}
if (!(Test-Path $mysqlBackup)) {
    New-Item -ItemType Directory -Path $mysqlBackup | Out-Null
}

# Sauvegarder le factory actuel
$factoryBackup = Get-Content $factoryPath -Raw

Write-Host "Factory actuel sauvegarde" -ForegroundColor Green
Write-Host ""

# Template pour SQLite
$sqliteFactory = @'
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Nawel.Api.Data;

/// <summary>
/// Factory for creating NawelDbContext at design-time (for migrations).
/// This ensures migrations are always generated for SQLite.
/// </summary>
public class NawelDbContextFactory : IDesignTimeDbContextFactory<NawelDbContext>
{
    public NawelDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<NawelDbContext>();

        // Use SQLite for development
        var connectionString = "Data Source=nawel.db";
        optionsBuilder.UseSqlite(connectionString);

        return new NawelDbContext(optionsBuilder.Options);
    }
}
'@

# Template pour MySQL
$mysqlFactory = @'
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Nawel.Api.Data;

/// <summary>
/// Factory for creating NawelDbContext at design-time (for migrations).
/// This ensures migrations are always generated for MySQL.
/// </summary>
public class NawelDbContextFactory : IDesignTimeDbContextFactory<NawelDbContext>
{
    public NawelDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<NawelDbContext>();

        // Use a dummy MySQL connection string for migration generation
        // The actual connection string will be used at runtime
        var connectionString = "Server=localhost;Port=3306;Database=nawel;User=root;Password=dummy;";
        var serverVersion = new MySqlServerVersion(new Version(8, 0, 0));

        optionsBuilder.UseMySql(connectionString, serverVersion);

        return new NawelDbContext(optionsBuilder.Options);
    }
}
'@

try {
    # ===========================
    # 1. Generer migration SQLite
    # ===========================
    Write-Host "Etape 1/4 : Generation de la migration SQLite..." -ForegroundColor Yellow
    $sqliteFactory | Set-Content $factoryPath -NoNewline

    Push-Location $apiPath
    dotnet ef migrations add $Name --output-dir Migrations
    Pop-Location

    Write-Host "Migration SQLite generee" -ForegroundColor Green

    # Sauvegarder vers _backup/SQLite
    Write-Host "Sauvegarde vers _backup/SQLite/..." -ForegroundColor Yellow
    Get-ChildItem "$migrationsPath\*.cs" | Copy-Item -Destination $sqliteBackup -Force
    Write-Host "Migration SQLite sauvegardee" -ForegroundColor Green
    Write-Host ""

    # Supprimer les migrations actives pour la prochaine generation
    Remove-Item "$migrationsPath\*.cs" -Force

    # ===========================
    # 2. Generer migration MySQL
    # ===========================
    Write-Host "Etape 2/4 : Generation de la migration MySQL..." -ForegroundColor Yellow
    $mysqlFactory | Set-Content $factoryPath -NoNewline

    Push-Location $apiPath
    dotnet ef migrations add $Name --output-dir Migrations
    Pop-Location

    Write-Host "Migration MySQL generee" -ForegroundColor Green

    # Sauvegarder vers _backup/MySQL
    Write-Host "Sauvegarde vers _backup/MySQL/..." -ForegroundColor Yellow
    Get-ChildItem "$migrationsPath\*.cs" | Copy-Item -Destination $mysqlBackup -Force
    Write-Host "Migration MySQL sauvegardee" -ForegroundColor Green
    Write-Host ""

    # ===========================
    # 3. Restaurer le factory original
    # ===========================
    Write-Host "Etape 3/4 : Restauration du factory..." -ForegroundColor Yellow
    $factoryBackup | Set-Content $factoryPath -NoNewline
    Write-Host "Factory restaure" -ForegroundColor Green
    Write-Host ""

    # ===========================
    # 4. Activer SQLite par defaut
    # ===========================
    Write-Host "Etape 4/4 : Activation des migrations SQLite (dev)..." -ForegroundColor Yellow
    Remove-Item "$migrationsPath\*.cs" -Force -ErrorAction SilentlyContinue
    Get-ChildItem "$sqliteBackup\*.cs" | Copy-Item -Destination $migrationsPath -Force
    Write-Host "Migrations SQLite activees" -ForegroundColor Green
    Write-Host ""

    Write-Host "==================================================" -ForegroundColor Green
    Write-Host "Succes ! Les deux migrations ont ete generees" -ForegroundColor Green
    Write-Host "==================================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "Migrations generees :" -ForegroundColor Cyan
    Write-Host "   - SQLite : Nawel.Api\Migrations\_backup\SQLite\" -ForegroundColor White
    Write-Host "   - MySQL  : Nawel.Api\Migrations\_backup\MySQL\" -ForegroundColor White
    Write-Host "   - Active : SQLite (pour le developpement)" -ForegroundColor White
    Write-Host ""
    Write-Host "Important :" -ForegroundColor Yellow
    Write-Host "   1. Verifiez les deux migrations generees" -ForegroundColor White
    Write-Host "   2. Adaptez manuellement si necessaire (NOW(), decimal, etc.)" -ForegroundColor White
    Write-Host "   3. Testez en dev avec SQLite" -ForegroundColor White
    Write-Host "   4. Commitez TOUT (y compris _backup/)" -ForegroundColor White
    Write-Host ""
    Write-Host "Le deploiement basculera automatiquement vers MySQL" -ForegroundColor Cyan
    Write-Host ""

} catch {
    Write-Host ""
    Write-Host "Erreur lors de la generation : $_" -ForegroundColor Red
    Write-Host ""

    # Restaurer le factory en cas d'erreur
    Write-Host "Restauration du factory original..." -ForegroundColor Yellow
    $factoryBackup | Set-Content $factoryPath -NoNewline
    Write-Host "Factory restaure" -ForegroundColor Green

    exit 1
}
