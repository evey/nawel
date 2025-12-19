#!/bin/bash

# ============================================================================
# Nawel Database Migration Script
# ============================================================================
# This script automates the migration from the old database (nironico_nawel.sql)
# to the new Nawel system.
#
# Usage: ./migrate_old_to_new.sh [options]
#
# Options:
#   --backup-only    Create backup only without migration
#   --verify-only    Run verification queries only
#   --help           Show this help message
#
# Prerequisites:
#   - Old database file: old/nironico_nawel.sql (relative to project root)
#   - SQLite3 installed
#   - Current directory: backend/Nawel.Api/
# ============================================================================

set -e  # Exit on error

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Directories
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
API_DIR="$(dirname "$SCRIPT_DIR")"
PROJECT_ROOT="$(dirname "$(dirname "$API_DIR")")"
OLD_SQL="$PROJECT_ROOT/old/nironico_nawel_update.sql"
DB_FILE="$API_DIR/nawel.db"
BACKUP_FILE="$API_DIR/nawel.db.backup_$(date +%Y%m%d_%H%M%S)"

# ============================================================================
# Functions
# ============================================================================

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

check_prerequisites() {
    print_header "Checking Prerequisites"

    # Check if sqlite3 is installed
    if ! command -v sqlite3 &> /dev/null; then
        print_error "sqlite3 is not installed"
        echo "Install it with: sudo apt install sqlite3 (Linux) or brew install sqlite3 (Mac)"
        exit 1
    fi
    print_success "sqlite3 is installed"

    # Check if old SQL file exists
    if [ ! -f "$OLD_SQL" ]; then
        print_error "Old database file not found: $OLD_SQL"
        echo "Expected path: old/nironico_nawel.sql (from project root)"
        exit 1
    fi
    print_success "Old database file found"

    # Check if current database exists
    if [ ! -f "$DB_FILE" ]; then
        print_warning "Database file not found: $DB_FILE"
        print_info "Creating new database..."
        touch "$DB_FILE"
    fi
    print_success "Database file ready"
}

create_backup() {
    print_header "Creating Backup"

    if [ -f "$DB_FILE" ] && [ -s "$DB_FILE" ]; then
        cp "$DB_FILE" "$BACKUP_FILE"
        print_success "Backup created: $(basename "$BACKUP_FILE")"
        echo "  Location: $BACKUP_FILE"
    else
        print_info "Database is empty, skipping backup"
    fi
}

run_migration() {
    print_header "Running Migration"

    print_info "Step 1/6: Creating temporary database..."
    local TEMP_DB="$API_DIR/nawel_old_temp.db"
    rm -f "$TEMP_DB"
    sqlite3 "$TEMP_DB" < "$OLD_SQL"
    print_success "Temporary database created"

    print_info "Step 2/6: Migrating families..."
    sqlite3 "$DB_FILE" <<EOF
ATTACH DATABASE '$TEMP_DB' AS old_db;
INSERT OR IGNORE INTO family (id, name, created_at)
SELECT id, name, datetime('now') FROM old_db.family;
DETACH DATABASE old_db;
EOF
    local family_count=$(sqlite3 "$DB_FILE" "SELECT COUNT(*) FROM family;")
    print_success "Migrated $family_count families"

    print_info "Step 3/6: Migrating users..."
    sqlite3 "$DB_FILE" <<EOF
ATTACH DATABASE '$TEMP_DB' AS old_db;
INSERT OR IGNORE INTO user (
    id, login, pwd, email, first_name, last_name, avatar, pseudo,
    notify_list_edit, notify_gift_taken, display_popup,
    reset_token, token_expiry, isChildren, family_id, is_admin,
    created_at, updated_at
)
SELECT
    id, login, pwd, email, first_name, last_name,
    CASE WHEN avatar = 'default.png' THEN 'avatar.png' ELSE avatar END,
    pseudo, notify_list_edit, notify_gift_taken, display_popup,
    reset_token, token_expiry, isChildren, family_id,
    CASE WHEN id = 1 THEN 1 ELSE 0 END,
    datetime('now'), datetime('now')
FROM old_db.user;
DETACH DATABASE old_db;
EOF
    local user_count=$(sqlite3 "$DB_FILE" "SELECT COUNT(*) FROM user;")
    print_success "Migrated $user_count users"

    print_info "Step 4/6: Migrating lists..."
    sqlite3 "$DB_FILE" <<EOF
ATTACH DATABASE '$TEMP_DB' AS old_db;
INSERT OR IGNORE INTO lists (id, name, user_id, created_at, updated_at)
SELECT id, name, user_id, datetime('now'), datetime('now') FROM old_db.lists;
DETACH DATABASE old_db;
EOF
    local list_count=$(sqlite3 "$DB_FILE" "SELECT COUNT(*) FROM lists;")
    print_success "Migrated $list_count lists"

    print_info "Step 5/6: Migrating gifts..."
    sqlite3 "$DB_FILE" <<EOF
ATTACH DATABASE '$TEMP_DB' AS old_db;
INSERT OR IGNORE INTO gifts (
    id, list_id, name, description, image, link, cost, currency,
    available, taken_by, is_group_gift, comment, year,
    created_at, updated_at
)
SELECT
    g.id, g.list_id, g.name, g.description, g.image, g.link,
    g.cost, g.currency, g.available, g.taken_by,
    CASE WHEN EXISTS (
        SELECT 1 FROM old_db.gift_participation gp
        WHERE gp.gift_id = g.id AND gp.is_active = 1
    ) THEN 1 ELSE 0 END,
    g.comment, g.year,
    datetime('now'), datetime('now')
FROM old_db.gifts g;
DETACH DATABASE old_db;
EOF
    local gift_count=$(sqlite3 "$DB_FILE" "SELECT COUNT(*) FROM gifts;")
    local group_gift_count=$(sqlite3 "$DB_FILE" "SELECT COUNT(*) FROM gifts WHERE is_group_gift = 1;")
    print_success "Migrated $gift_count gifts ($group_gift_count group gifts)"

    print_info "Step 6/6: Migrating gift participations..."
    sqlite3 "$DB_FILE" <<EOF
ATTACH DATABASE '$TEMP_DB' AS old_db;
INSERT OR IGNORE INTO gift_participation (id, gift_id, user_id, is_active, created_at)
SELECT id, gift_id, user_id, is_active, datetime('now') FROM old_db.gift_participation;
DETACH DATABASE old_db;
EOF
    local participation_count=$(sqlite3 "$DB_FILE" "SELECT COUNT(*) FROM gift_participation;")
    print_success "Migrated $participation_count participations"

    print_info "Updating auto-increment sequences..."
    sqlite3 "$DB_FILE" <<EOF
UPDATE sqlite_sequence SET seq = (SELECT IFNULL(MAX(id), 0) FROM family) WHERE name = 'family';
UPDATE sqlite_sequence SET seq = (SELECT IFNULL(MAX(id), 0) FROM user) WHERE name = 'user';
UPDATE sqlite_sequence SET seq = (SELECT IFNULL(MAX(id), 0) FROM lists) WHERE name = 'lists';
UPDATE sqlite_sequence SET seq = (SELECT IFNULL(MAX(id), 0) FROM gifts) WHERE name = 'gifts';
UPDATE sqlite_sequence SET seq = (SELECT IFNULL(MAX(id), 0) FROM gift_participation) WHERE name = 'gift_participation';
EOF
    print_success "Auto-increment sequences updated"

    print_info "Cleaning up temporary files..."
    rm -f "$TEMP_DB"
    print_success "Cleanup complete"
}

copy_avatars() {
    print_header "Copying Avatar Files"

    local OLD_AVATAR_DIR="$PROJECT_ROOT/old/uploads/avatars"
    local NEW_AVATAR_DIR="$API_DIR/uploads/avatars"

    if [ ! -d "$OLD_AVATAR_DIR" ]; then
        print_warning "Old avatar directory not found: $OLD_AVATAR_DIR"
        print_info "Skipping avatar copy"
        return
    fi

    mkdir -p "$NEW_AVATAR_DIR"

    local avatar_count=$(find "$OLD_AVATAR_DIR" -type f | wc -l)
    if [ "$avatar_count" -eq 0 ]; then
        print_info "No avatars to copy"
        return
    fi

    cp -r "$OLD_AVATAR_DIR"/* "$NEW_AVATAR_DIR/" 2>/dev/null || true
    local copied_count=$(find "$NEW_AVATAR_DIR" -type f | wc -l)
    print_success "Copied $copied_count avatar files"
}

run_verification() {
    print_header "Verification"

    print_info "Running verification queries..."

    # Count families
    local families=$(sqlite3 "$DB_FILE" "SELECT COUNT(*) FROM family;")
    echo -e "  ${GREEN}Families:${NC} $families"

    # Count users
    local total_users=$(sqlite3 "$DB_FILE" "SELECT COUNT(*) FROM user;")
    local md5_users=$(sqlite3 "$DB_FILE" "SELECT COUNT(*) FROM user WHERE LENGTH(pwd) = 32 AND SUBSTR(pwd, 1, 2) != '\$2';")
    local admin_users=$(sqlite3 "$DB_FILE" "SELECT COUNT(*) FROM user WHERE is_admin = 1;")
    echo -e "  ${GREEN}Users:${NC} $total_users (MD5: $md5_users, Admins: $admin_users)"

    # Count lists
    local lists=$(sqlite3 "$DB_FILE" "SELECT COUNT(*) FROM lists;")
    echo -e "  ${GREEN}Lists:${NC} $lists"

    # Count gifts
    local total_gifts=$(sqlite3 "$DB_FILE" "SELECT COUNT(*) FROM gifts;")
    local group_gifts=$(sqlite3 "$DB_FILE" "SELECT COUNT(*) FROM gifts WHERE is_group_gift = 1;")
    local min_year=$(sqlite3 "$DB_FILE" "SELECT MIN(year) FROM gifts;")
    local max_year=$(sqlite3 "$DB_FILE" "SELECT MAX(year) FROM gifts;")
    echo -e "  ${GREEN}Gifts:${NC} $total_gifts (Group: $group_gifts, Years: $min_year-$max_year)"

    # Count participations
    local participations=$(sqlite3 "$DB_FILE" "SELECT COUNT(*) FROM gift_participation;")
    echo -e "  ${GREEN}Participations:${NC} $participations"

    # Check for orphaned group gifts (group gifts without participations)
    local orphaned=$(sqlite3 "$DB_FILE" "SELECT COUNT(*) FROM gifts g WHERE g.is_group_gift = 1 AND NOT EXISTS (SELECT 1 FROM gift_participation gp WHERE gp.gift_id = g.id);")
    if [ "$orphaned" -gt 0 ]; then
        print_warning "$orphaned group gifts have no participations"
    else
        print_success "All group gifts have participations"
    fi

    # Check admin user
    local admin_login=$(sqlite3 "$DB_FILE" "SELECT login FROM user WHERE is_admin = 1 LIMIT 1;")
    if [ -n "$admin_login" ]; then
        print_success "Admin user found: $admin_login"
    else
        print_warning "No admin user found"
    fi
}

show_help() {
    echo "Nawel Database Migration Script"
    echo ""
    echo "Usage: $0 [options]"
    echo ""
    echo "Options:"
    echo "  --backup-only    Create backup only without migration"
    echo "  --verify-only    Run verification queries only"
    echo "  --help           Show this help message"
    echo ""
    echo "This script migrates data from old/nironico_nawel.sql to the new database."
    echo "It will automatically:"
    echo "  - Create a backup of the current database"
    echo "  - Migrate all tables (family, user, lists, gifts, gift_participation)"
    echo "  - Copy avatar files"
    echo "  - Run verification queries"
    echo ""
    echo "Prerequisites:"
    echo "  - sqlite3 must be installed"
    echo "  - old/nironico_nawel.sql must exist"
    echo "  - Current directory must be backend/Nawel.Api/"
}

print_next_steps() {
    print_header "Next Steps"

    echo -e "${GREEN}Migration completed successfully!${NC}"
    echo ""
    echo "What to do next:"
    echo ""
    echo "1. ${BLUE}Test Authentication${NC}"
    echo "   - Users with MD5 passwords will need to reset their passwords"
    echo "   - The system will automatically guide them through the process"
    echo "   - See MIGRATION_MD5_PLAN.md for details"
    echo ""
    echo "2. ${BLUE}Verify Data${NC}"
    echo "   - Login to the application"
    echo "   - Check that lists are visible"
    echo "   - Verify avatars are displayed"
    echo "   - Test gift reservations"
    echo ""
    echo "3. ${BLUE}Admin Access${NC}"
    echo "   - Login as admin (user ID 1)"
    echo "   - Verify admin panel access"
    echo "   - Check family management"
    echo ""
    echo "4. ${BLUE}Backup${NC}"
    echo "   - Backup file saved at:"
    echo "     $BACKUP_FILE"
    echo "   - Keep this backup safe!"
    echo ""
    echo "For troubleshooting, see: Migrations/MIGRATION_GUIDE.md"
}

# ============================================================================
# Main Script
# ============================================================================

main() {
    print_header "Nawel Database Migration"

    # Parse arguments
    case "${1:-}" in
        --backup-only)
            check_prerequisites
            create_backup
            exit 0
            ;;
        --verify-only)
            run_verification
            exit 0
            ;;
        --help)
            show_help
            exit 0
            ;;
        "")
            # Continue with full migration
            ;;
        *)
            print_error "Unknown option: $1"
            echo ""
            show_help
            exit 1
            ;;
    esac

    # Run full migration
    check_prerequisites
    create_backup
    run_migration
    copy_avatars
    run_verification
    print_next_steps

    print_success "All done! 🎄🎁"
}

# Run main function
main "$@"
