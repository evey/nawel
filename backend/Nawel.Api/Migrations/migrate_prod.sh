#!/bin/bash

# =============================================================================
# Script de Migration Production MySQL
# =============================================================================
# Ce script automatise la migration des données en production MySQL.
# Il effectue le backup, l'import des CSV et les vérifications.
#
# Usage:
#   chmod +x migrate_prod.sh
#   ./migrate_prod.sh
#
# Prérequis:
#   - Accès MySQL avec les credentials de production
#   - Fichiers CSV dans /tmp/
#   - LOAD DATA LOCAL INFILE activé
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

# Demander les credentials MySQL
read -p "Nom d'utilisateur MySQL [nawel_user]: " MYSQL_USER
MYSQL_USER=${MYSQL_USER:-nawel_user}

read -sp "Mot de passe MySQL: " MYSQL_PASSWORD
echo

read -p "Nom de la base de données [nawel]: " MYSQL_DB
MYSQL_DB=${MYSQL_DB:-nawel}

# Vérifier les fichiers CSV
CSV_GIFTS="/tmp/gifts_phpma.csv"
CSV_PARTICIPATION="/tmp/gift_participation_phpma.csv"

if [ ! -f "$CSV_GIFTS" ]; then
    print_error "Fichier CSV des gifts non trouvé: $CSV_GIFTS"
    exit 1
fi

if [ ! -f "$CSV_PARTICIPATION" ]; then
    print_error "Fichier CSV des participations non trouvé: $CSV_PARTICIPATION"
    exit 1
fi

print_success "Fichiers CSV trouvés"

# =============================================================================
# ÉTAPE 1: Backup de la base actuelle
# =============================================================================

print_header "Étape 1: Backup de la base de données"

BACKUP_FILE="backup_nawel_$(date +%Y%m%d_%H%M%S).sql"

print_info "Création du backup dans: $BACKUP_FILE"

mysqldump -u "$MYSQL_USER" -p"$MYSQL_PASSWORD" "$MYSQL_DB" > "$BACKUP_FILE"

if [ $? -eq 0 ]; then
    BACKUP_SIZE=$(du -h "$BACKUP_FILE" | cut -f1)
    print_success "Backup créé avec succès ($BACKUP_SIZE)"
else
    print_error "Échec de la création du backup"
    exit 1
fi

# =============================================================================
# ÉTAPE 2: Vérifier la connexion MySQL
# =============================================================================

print_header "Étape 2: Vérification de la connexion MySQL"

mysql -u "$MYSQL_USER" -p"$MYSQL_PASSWORD" -e "SELECT 1" > /dev/null 2>&1

if [ $? -eq 0 ]; then
    print_success "Connexion MySQL OK"
else
    print_error "Impossible de se connecter à MySQL"
    exit 1
fi

# =============================================================================
# ÉTAPE 3: Confirmation avant migration
# =============================================================================

print_header "Étape 3: Confirmation"

echo -e "${YELLOW}Vous êtes sur le point de:${NC}"
echo "  - Supprimer toutes les données actuelles de gifts et gift_participation"
echo "  - Importer les données depuis les fichiers CSV"
echo "  - Mettre à jour les chemins des avatars"
echo ""
echo -e "${GREEN}Un backup a été créé: $BACKUP_FILE${NC}"
echo ""
read -p "Continuer? (oui/non): " CONFIRM

if [ "$CONFIRM" != "oui" ]; then
    print_warning "Migration annulée par l'utilisateur"
    exit 0
fi

# =============================================================================
# ÉTAPE 4: Exécuter le script SQL de migration
# =============================================================================

print_header "Étape 4: Exécution de la migration"

print_info "Import des données en cours..."

mysql -u "$MYSQL_USER" -p"$MYSQL_PASSWORD" --local-infile=1 "$MYSQL_DB" < import_prod_mysql.sql

if [ $? -eq 0 ]; then
    print_success "Migration exécutée avec succès"
else
    print_error "Échec de la migration"
    print_warning "Vous pouvez restaurer le backup avec:"
    echo "  mysql -u $MYSQL_USER -p $MYSQL_DB < $BACKUP_FILE"
    exit 1
fi

# =============================================================================
# ÉTAPE 5: Vérifications supplémentaires
# =============================================================================

print_header "Étape 5: Vérifications supplémentaires"

# Vérifier le nombre de gifts
GIFT_COUNT=$(mysql -u "$MYSQL_USER" -p"$MYSQL_PASSWORD" -s -N -e "SELECT COUNT(*) FROM gifts" "$MYSQL_DB")
print_info "Nombre de gifts importés: $GIFT_COUNT"

if [ "$GIFT_COUNT" -lt 2000 ]; then
    print_warning "Attention: nombre de gifts inférieur à 2000 (attendu: ~2583)"
else
    print_success "Nombre de gifts cohérent"
fi

# Vérifier les participations
PARTICIPATION_COUNT=$(mysql -u "$MYSQL_USER" -p"$MYSQL_PASSWORD" -s -N -e "SELECT COUNT(*) FROM gift_participation" "$MYSQL_DB")
print_info "Nombre de participations importées: $PARTICIPATION_COUNT"

if [ "$PARTICIPATION_COUNT" -lt 50 ]; then
    print_warning "Attention: nombre de participations inférieur à 50 (attendu: ~59)"
else
    print_success "Nombre de participations cohérent"
fi

# =============================================================================
# ÉTAPE 6: Instructions finales
# =============================================================================

print_header "Migration terminée!"

print_success "Données importées avec succès"
echo ""
echo -e "${YELLOW}Prochaines étapes:${NC}"
echo ""
echo "1. Copier les fichiers avatar:"
echo "   mkdir -p uploads/avatars"
echo "   cp /tmp/avatars/* uploads/avatars/"
echo "   chmod 755 uploads/avatars"
echo "   chmod 644 uploads/avatars/*"
echo ""
echo "2. Redémarrer l'application:"
echo "   docker-compose restart nawel-api"
echo "   # OU"
echo "   systemctl restart nawel-api"
echo ""
echo "3. Vérifier les logs:"
echo "   docker logs -f nawel-api"
echo "   # OU"
echo "   journalctl -u nawel-api -f"
echo ""
echo "4. Tester l'application dans le navigateur"
echo ""
echo -e "${GREEN}Backup disponible: $BACKUP_FILE${NC}"
echo ""
print_info "En cas de problème, restaurer avec:"
echo "  mysql -u $MYSQL_USER -p $MYSQL_DB < $BACKUP_FILE"
echo ""
