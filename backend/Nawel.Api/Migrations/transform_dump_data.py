#!/usr/bin/env python3
# =============================================================================
# Script Python pour transformer les données du dump SQL
# =============================================================================
# Ce script extrait les INSERT de gifts et gift_participation du vieux dump
# et les transforme pour la nouvelle structure
# =============================================================================

import re
import sys

OLD_DUMP_PATH = r"D:\Perso\nawel\old\nironico_nawel_update.sql"
OUTPUT_PATH = r"D:\Perso\nawel\backend\Nawel.Api\Migrations\import_gifts_data.sql"

def main():
    print("Lecture du dump original...")

    try:
        with open(OLD_DUMP_PATH, 'r', encoding='utf-8', errors='ignore') as f:
            lines = f.readlines()
    except Exception as e:
        print(f"Erreur lors de la lecture du fichier: {e}")
        sys.exit(1)

    output_lines = []
    output_lines.append("-- =============================================================================")
    output_lines.append("-- Import des données gifts et gift_participation")
    output_lines.append("-- Généré automatiquement à partir de l'ancien dump")
    output_lines.append("-- =============================================================================")
    output_lines.append("")
    output_lines.append("SET FOREIGN_KEY_CHECKS = 0;")
    output_lines.append("")

    print("Transformation des données gifts...")

    # Traiter les gifts ligne par ligne
    in_gifts_insert = False
    gifts_buffer = []

    for i, line in enumerate(lines):
        # Détecter le début d'un INSERT gifts
        if "INSERT INTO `gifts`" in line and "VALUES" in line:
            in_gifts_insert = True
            gifts_buffer = []
            continue

        # Si on est dans un INSERT gifts
        if in_gifts_insert:
            # Ajouter la ligne au buffer
            gifts_buffer.append(line.rstrip())

            # Détecter la fin de l'INSERT (ligne se terminant par ;)
            if line.rstrip().endswith(';'):
                in_gifts_insert = False
                # Traiter le buffer
                process_gifts_insert(gifts_buffer, output_lines)

    print("Transformation des données gift_participation...")

    # Traiter gift_participation
    in_participation_insert = False
    participation_buffer = []

    for i, line in enumerate(lines):
        if "INSERT INTO `gift_participation`" in line and "VALUES" in line:
            in_participation_insert = True
            participation_buffer = []
            continue

        if in_participation_insert:
            participation_buffer.append(line.rstrip())

            if line.rstrip().endswith(';'):
                in_participation_insert = False
                process_participation_insert(participation_buffer, output_lines)

    output_lines.append("SET FOREIGN_KEY_CHECKS = 1;")
    output_lines.append("")
    output_lines.append("SELECT 'Import des cadeaux terminé' as status;")

    print("Écriture du fichier de sortie...")

    try:
        with open(OUTPUT_PATH, 'w', encoding='utf-8') as f:
            f.write('\n'.join(output_lines))
        print(f"Terminé! Fichier généré: {OUTPUT_PATH}")
        print("Vous pouvez maintenant exécuter ce fichier après import_old_data.sql")
    except Exception as e:
        print(f"Erreur lors de l'écriture du fichier: {e}")
        sys.exit(1)


def process_gifts_insert(buffer, output_lines):
    """Traite un bloc d'INSERT gifts"""
    # Joindre toutes les lignes
    full_text = ' '.join(buffer)

    # Extraire toutes les valeurs entre parenthèses
    # Pattern: chercher (...)
    values = extract_value_groups(full_text)

    if not values:
        return

    output_lines.append("INSERT INTO gifts (id, list_id, name, description, image, link, cost, currency, available, taken_by, is_group_gift, comment, year, created_at, updated_at)")
    output_lines.append("VALUES")

    transformed = []
    for value in values:
        # Trouver la position des 3 derniers champs: taken_by, comment, year
        # On doit insérer is_group_gift après taken_by

        # Compter les virgules depuis la fin
        # Structure: ..., taken_by, comment, year)
        # On veut: ..., taken_by, 0, comment, year, NOW(), NOW())

        parts = split_sql_values(value)

        if len(parts) == 12:  # Vérifier qu'on a 12 colonnes
            # Insérer is_group_gift (0) après taken_by (position 9)
            parts.insert(10, '0')
            # Ajouter created_at et updated_at
            parts.append('NOW()')
            parts.append('NOW()')

            transformed_value = '(' + ', '.join(parts) + ')'
            transformed.append(transformed_value)

    # Joindre avec des virgules
    for i, val in enumerate(transformed):
        if i < len(transformed) - 1:
            output_lines.append(val + ',')
        else:
            output_lines.append(val + ';')

    output_lines.append("")


def process_participation_insert(buffer, output_lines):
    """Traite un bloc d'INSERT gift_participation"""
    full_text = ' '.join(buffer)

    values = extract_value_groups(full_text)

    if not values:
        return

    output_lines.append("INSERT INTO gift_participation (id, gift_id, user_id, is_active, created_at)")
    output_lines.append("VALUES")

    transformed = []
    for value in values:
        # Enlever les parenthèses et ajouter , NOW()
        value_content = value.strip('()')
        transformed_value = '(' + value_content + ', NOW())'
        transformed.append(transformed_value)

    for i, val in enumerate(transformed):
        if i < len(transformed) - 1:
            output_lines.append(val + ',')
        else:
            output_lines.append(val + ';')

    output_lines.append("")


def extract_value_groups(text):
    """Extrait tous les groupes (valeur1, valeur2, ...) d'une chaîne SQL"""
    values = []
    depth = 0
    current = ''
    in_string = False
    quote_char = None
    escape_next = False

    for char in text:
        if escape_next:
            current += char
            escape_next = False
            continue

        if char == '\\':
            current += char
            escape_next = True
            continue

        if char in ('"', "'") and not in_string:
            in_string = True
            quote_char = char
            current += char
        elif char == quote_char and in_string:
            in_string = False
            quote_char = None
            current += char
        elif char == '(' and not in_string:
            depth += 1
            if depth == 1:
                current = ''
            else:
                current += char
        elif char == ')' and not in_string:
            depth -= 1
            if depth == 0:
                if current.strip():
                    values.append(current.strip())
                current = ''
            else:
                current += char
        else:
            if depth > 0:
                current += char

    return values


def split_sql_values(value_string):
    """
    Découpe une chaîne de valeurs SQL en tenant compte des guillemets et échappements.
    Exemple: "1, 2, 'test', NULL" -> ['1', '2', "'test'", 'NULL']
    """
    values = []
    current_value = ''
    in_string = False
    escape_next = False
    quote_char = None

    for char in value_string:
        if escape_next:
            current_value += char
            escape_next = False
        elif char == '\\':
            current_value += char
            escape_next = True
        elif char in ('"', "'") and not in_string:
            in_string = True
            quote_char = char
            current_value += char
        elif char == quote_char and in_string:
            current_value += char
            in_string = False
            quote_char = None
        elif char == ',' and not in_string:
            values.append(current_value.strip())
            current_value = ''
        else:
            current_value += char

    # Ajouter la dernière valeur
    if current_value.strip():
        values.append(current_value.strip())

    return values


if __name__ == "__main__":
    main()
