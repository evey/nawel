# =============================================================================
# Script PowerShell pour transformer les données du dump SQL
# =============================================================================
# Ce script extrait les INSERT de gifts et gift_participation du vieux dump
# et les transforme pour la nouvelle structure
# =============================================================================

$oldDumpPath = "D:\Perso\nawel\old\nironico_nawel_update.sql"
$outputPath = "D:\Perso\nawel\backend\Nawel.Api\Migrations\import_gifts_data.sql"

Write-Host "Lecture du dump original..." -ForegroundColor Cyan

# Lire le fichier
$content = Get-Content -Path $oldDumpPath -Raw -Encoding UTF8

Write-Host "Transformation des données gifts..." -ForegroundColor Cyan

# Extraire tous les INSERT INTO gifts
$giftsPattern = 'INSERT INTO `gifts` \(`id`, `list_id`, `name`, `description`, `image`, `link`, `cost`, `currency`, `available`, `taken_by`, `comment`, `year`\) VALUES\s*([\s\S]*?);'
$giftsMatches = [regex]::Matches($content, $giftsPattern)

$giftsOutput = @()
$giftsOutput += "-- ============================================================================="
$giftsOutput += "-- Import des données gifts"
$giftsOutput += "-- ============================================================================="
$giftsOutput += ""

foreach ($match in $giftsMatches) {
    $values = $match.Groups[1].Value

    # Transformer chaque ligne de valeurs
    # Format: (..., taken_by, comment, year)
    # Devient: (..., taken_by, is_group_gift, comment, year, created_at, updated_at)

    # Diviser par les parenthèses fermantes suivies d'une virgule
    $lines = $values -split '\),\s*\n'

    $transformedLines = @()
    foreach ($line in $lines) {
        # Nettoyer la ligne
        $line = $line.Trim()
        if ($line.Length -eq 0) { continue }

        # Si la ligne ne se termine pas par ), l'ajouter
        if (-not $line.EndsWith(')')) {
            $line += ')'
        }

        # Remplacer la dernière virgule avant year par une virgule + is_group_gift
        # Pattern: trouve la partie avant le dernier groupe (taken_by, comment, year)
        # et insère is_group_gift après taken_by

        # Regex plus simple: on cherche le pattern avant les 3 derniers champs
        # Exemple: (..., NULL, NULL, 2016)
        # On veut: (..., NULL, 0, NULL, 2016, NOW(), NOW())

        if ($line -match '^(.+,\s*)(.+),\s*(.+),\s*(.+)\)$') {
            $before = $matches[1]  # Tout avant taken_by
            $takenBy = $matches[2]  # taken_by
            $comment = $matches[3]  # comment
            $year = $matches[4]     # year

            $transformedLine = "$before$takenBy, 0, $comment, $year, NOW(), NOW())"
            $transformedLines += $transformedLine
        }
    }

    if ($transformedLines.Count -gt 0) {
        $giftsOutput += "INSERT INTO gifts (id, list_id, name, description, image, link, cost, currency, available, taken_by, is_group_gift, comment, year, created_at, updated_at)"
        $giftsOutput += "VALUES"

        # Joindre toutes les lignes sauf la dernière avec une virgule
        for ($i = 0; $i -lt $transformedLines.Count - 1; $i++) {
            $giftsOutput += "$($transformedLines[$i]),"
        }
        # Dernière ligne avec point-virgule
        $giftsOutput += "$($transformedLines[-1]);"
        $giftsOutput += ""
    }
}

Write-Host "Transformation des données gift_participation..." -ForegroundColor Cyan

# Extraire tous les INSERT INTO gift_participation
$participationPattern = 'INSERT INTO `gift_participation` \(`id`, `gift_id`, `user_id`, `is_active`\) VALUES\s*([\s\S]*?);'
$participationMatches = [regex]::Matches($content, $participationPattern)

$giftsOutput += "-- ============================================================================="
$giftsOutput += "-- Import des données gift_participation"
$giftsOutput += "-- ============================================================================="
$giftsOutput += ""

foreach ($match in $participationMatches) {
    $values = $match.Groups[1].Value

    # Transformer chaque ligne
    # Format: (id, gift_id, user_id, is_active)
    # Devient: (id, gift_id, user_id, is_active, created_at)

    $lines = $values -split '\),\s*\n'

    $transformedLines = @()
    foreach ($line in $lines) {
        $line = $line.Trim()
        if ($line.Length -eq 0) { continue }

        # Remplacer ) par , NOW())
        if ($line.EndsWith(')')) {
            $line = $line.Substring(0, $line.Length - 1) + ", NOW())"
        } else {
            $line += ", NOW())"
        }

        $transformedLines += $line
    }

    if ($transformedLines.Count -gt 0) {
        $giftsOutput += "INSERT INTO gift_participation (id, gift_id, user_id, is_active, created_at)"
        $giftsOutput += "VALUES"

        # Joindre toutes les lignes
        for ($i = 0; $i -lt $transformedLines.Count - 1; $i++) {
            $giftsOutput += "$($transformedLines[$i]),"
        }
        $giftsOutput += "$($transformedLines[-1]);"
        $giftsOutput += ""
    }
}

Write-Host "Écriture du fichier de sortie..." -ForegroundColor Cyan

# Sauvegarder
$giftsOutput | Out-File -FilePath $outputPath -Encoding UTF8

Write-Host "Terminé! Fichier généré: $outputPath" -ForegroundColor Green
Write-Host "Vous pouvez maintenant exécuter ce fichier après import_old_data.sql" -ForegroundColor Yellow
