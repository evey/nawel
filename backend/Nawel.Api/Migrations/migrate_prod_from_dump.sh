#!/bin/bash

# =============================================================================
# Script de Migration Production MySQL depuis dump SQL d'origine
# =============================================================================
# Ce script automatise l'import du dump MySQL d'origine et les ajustements
# nécessaires pour le nouveau système.
#
# Usage:
#   chmod +x migrate_prod_from_dump.sh
#   ./migrate_prod_from_dump.sh
#
# Prérequis:
#   - Accès au container Docker shared-mysql
#   - Fichier nironico_nawel_update.sql disponible
#   - Fichiers avatar dans un dossier à copier
# =============================================================================

set -e  # Arrêter en cas d'erreur

# Couleurs pour l'affichage
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Fonctions d'affichage
print_header() {
    echo -e "\n${BLUE}========================================${NC}"
    echo -e "${BLUE}$1${NC}"
    echo -e "${BLUE}========================================${NC}\n"
}

print_success() {
    echo -e "${GREEN}✓ $1${NC}"
}

print_error() {
    echo -e "${RED}✗ $1${NC}"
}

print_warning() {
    echo -e "${YELLOW}⚠ $1${NC}"
}

print_info() {
    echo -e "${BLUE}ℹ $1${NC}"
}

# =============================================================================
# Configuration
# =============================================================================

print_header "Configuration de la migration"

CONTAINER_NAME="shared-mysql"
MYSQL_USER="root"
MYSQL_DB="nawel"

# Vérifier que le container existe
if ! docker ps | grep -q "$CONTAINER_NAME"; then
    print_error "Container $CONTAINER_NAME non trouvé ou non démarré"
    exit 1
fi

print_success "Container $CONTAINER_NAME trouvé"

# Demander le chemin du dump SQL
read -p "Chemin vers le fichier nironico_nawel_update.sql: " SQL_DUMP_PATH

if [ ! -f "$SQL_DUMP_PATH" ]; then
    print_error "Fichier non trouvé: $SQL_DUMP_PATH"
    exit 1
fi

print_success "Fichier dump SQL trouvé"

# =============================================================================
# ÉTAPE 1: Backup de la base actuelle
# =============================================================================

print_header "Étape 1: Backup de la base de données"

BACKUP_FILE="backup_nawel_$(date +%Y%m%d_%H%M%S).sql"

print_info "Création du backup dans: $BACKUP_FILE"

docker exec $CONTAINER_NAME mysqldump -u $MYSQL_USER -p $MYSQL_DB > "$BACKUP_FILE" 2>/dev/null || true

if [ -f "$BACKUP_FILE" ]; then
    BACKUP_SIZE=$(du -h "$BACKUP_FILE" | cut -f1)
    print_success "Backup créé avec succès ($BACKUP_SIZE)"
else
    print_warning "Pas de backup créé (base vide ou erreur)"
fi

# =============================================================================
# ÉTAPE 2: Vérifier la connexion
# =============================================================================

print_header "Étape 2: Vérification de la connexion"

docker exec -i $CONTAINER_NAME mysql -u $MYSQL_USER -p -e "SELECT 1" $MYSQL_DB > /dev/null 2>&1

if [ $? -eq 0 ]; then
    print_success "Connexion MySQL OK"
else
    print_error "Impossible de se connecter à MySQL"
    exit 1
fi

# =============================================================================
# ÉTAPE 3: Confirmation
# =============================================================================

print_header "Étape 3: Confirmation"

echo -e "${YELLOW}Vous êtes sur le point de:${NC}"
echo "  - Importer toutes les données depuis: $SQL_DUMP_PATH"
echo "  - Tables: family, user, lists, gifts, gift_participation"
echo "  - Mettre à jour les chemins des avatars"
echo ""
if [ -f "$BACKUP_FILE" ]; then
    echo -e "${GREEN}Un backup a été créé: $BACKUP_FILE${NC}"
fi
echo ""
read -p "Continuer? (oui/non): " CONFIRM

if [ "$CONFIRM" != "oui" ]; then
    print_warning "Migration annulée par l'utilisateur"
    exit 0
fi

# =============================================================================
# ÉTAPE 4: Copier le dump dans le container
# =============================================================================

print_header "Étape 4: Préparation de l'import"

print_info "Copie du dump SQL dans le container..."

docker cp "$SQL_DUMP_PATH" $CONTAINER_NAME:/tmp/nironico_nawel_update.sql

if [ $? -eq 0 ]; then
    print_success "Fichier copié dans le container"
else
    print_error "Échec de la copie"
    exit 1
fi

# =============================================================================
# ÉTAPE 5: Importer le dump SQL
# =============================================================================

print_header "Étape 5: Import du dump SQL"

print_info "Import en cours... (peut prendre quelques minutes)"

docker exec -i $CONTAINER_NAME mysql -u $MYSQL_USER -p $MYSQL_DB < <(docker exec $CONTAINER_NAME cat /tmp/nironico_nawel_update.sql)

if [ $? -eq 0 ]; then
    print_success "Dump SQL importé avec succès"
else
    print_error "Échec de l'import du dump"
    if [ -f "$BACKUP_FILE" ]; then
        print_warning "Restaurez le backup si nécessaire"
    fi
    exit 1
fi

# =============================================================================
# ÉTAPE 6: Appliquer les ajustements (avatars, etc.)
# =============================================================================

print_header "Étape 6: Ajustements post-import"

print_info "Copie du script d'ajustements dans le container..."

SCRIPT_PATH="$(dirname "$0")/import_prod_from_dump.sql"

if [ ! -f "$SCRIPT_PATH" ]; then
    print_error "Script d'ajustements non trouvé: $SCRIPT_PATH"
    exit 1
fi

docker cp "$SCRIPT_PATH" $CONTAINER_NAME:/tmp/

print_info "Exécution des ajustements (mise à jour avatars, flags, etc.)..."

docker exec -i $CONTAINER_NAME mysql -u $MYSQL_USER -p $MYSQL_DB < <(docker exec $CONTAINER_NAME cat /tmp/import_prod_from_dump.sql)

if [ $? -eq 0 ]; then
    print_success "Ajustements appliqués avec succès"
else
    print_error "Erreur lors des ajustements"
    exit 1
fi

# =============================================================================
# ÉTAPE 7: Nettoyer
# =============================================================================

print_info "Nettoyage des fichiers temporaires dans le container..."

docker exec $CONTAINER_NAME rm -f /tmp/nironico_nawel_update.sql /tmp/import_prod_from_dump.sql

# =============================================================================
# ÉTAPE 8: Instructions finales
# =============================================================================

print_header "Migration terminée!"

print_success "Données importées avec succès"
echo ""
echo -e "${YELLOW}Prochaines étapes:${NC}"
echo ""
echo "1. Copier les fichiers avatar depuis votre ancienne application:"
echo "   # Localiser le dossier uploads/avatars dans votre application"
echo "   cd /path/to/nawel-app"
echo "   mkdir -p uploads/avatars"
echo "   cp /path/to/old/uploads/avatars/* uploads/avatars/"
echo "   chmod 755 uploads/avatars"
echo "   chmod 644 uploads/avatars/*"
echo ""
echo "2. Redémarrer l'application:"
echo "   docker-compose restart nawel-api"
echo "   # OU si c'est un container standalone"
echo "   docker restart nawel-api"
echo ""
echo "3. Vérifier les logs:"
echo "   docker logs -f nawel-api"
echo ""
echo "4. Tester l'application dans le navigateur:"
echo "   - Tentez de vous connecter avec un utilisateur"
echo "   - Les utilisateurs avec mot de passe MD5 devront le réinitialiser"
echo "   - Vérifiez que les avatars s'affichent correctement"
echo "   - Consultez les listes et cadeaux de différentes années"
echo ""
if [ -f "$BACKUP_FILE" ]; then
    echo -e "${GREEN}Backup disponible: $BACKUP_FILE${NC}"
    echo ""
    print_info "En cas de problème, restaurer avec:"
    echo "  docker exec -i $CONTAINER_NAME mysql -u $MYSQL_USER -p $MYSQL_DB < $BACKUP_FILE"
fi
echo ""
print_success "Bonne mise en production!"
