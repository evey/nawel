# Instructions pour l'import des anciennes données

## Vue d'ensemble

Ce dossier contient les scripts nécessaires pour importer les anciennes données de la base SQL vers la nouvelle structure créée par Entity Framework Core.

## Fichiers

- `import_old_data.sql` : Script SQL principal pour importer family, user et lists
- `transform_dump_data.py` : Script Python pour transformer les données de gifts et gift_participation
- `import_gifts_data.sql` : Généré par le script Python, contient les données transformées des cadeaux

## Procédure d'import

### Étape 1 : Démarrer l'environnement Docker

```bash
cd D:\Perso\nawel
docker-compose up -d
```

### Étape 2 : Drop la base de données existante

```bash
docker exec -it nawel-mysql mysql -u root -p
```

Puis dans MySQL:
```sql
DROP DATABASE IF EXISTS nawel_db;
CREATE DATABASE nawel_db CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
exit;
```

### Étape 3 : Laisser Entity Framework créer la structure

Démarrer le backend pour qu'Entity Framework crée automatiquement les tables avec la bonne structure:

```bash
cd backend\Nawel.Api
dotnet ef database update
```

Ou simplement démarrer l'application:
```bash
dotnet run
```

L'application va créer automatiquement toutes les tables avec la migration InitialCreate.

### Étape 4 : Générer le fichier d'import des cadeaux

Exécuter le script Python pour transformer les données de gifts et gift_participation:

```bash
cd backend\Nawel.Api\Migrations
python transform_dump_data.py
```

Cela va créer le fichier `import_gifts_data.sql` avec les données transformées.

### Étape 5 : Importer les données

Copier les fichiers SQL dans le conteneur Docker:

```bash
docker cp backend\Nawel.Api\Migrations\import_old_data.sql nawel-mysql:/tmp/
docker cp backend\Nawel.Api\Migrations\import_gifts_data.sql nawel-mysql:/tmp/
```

Exécuter les scripts SQL:

```bash
docker exec -it nawel-mysql mysql -u root -proot_password nawel_db < /tmp/import_old_data.sql
docker exec -it nawel-mysql mysql -u root -proot_password nawel_db < /tmp/import_gifts_data.sql
```

Ou en se connectant à MySQL:

```bash
docker exec -it nawel-mysql mysql -u root -p nawel_db
```

Puis:
```sql
source /tmp/import_old_data.sql;
source /tmp/import_gifts_data.sql;
```

### Étape 6 : Vérifier l'import

```sql
SELECT COUNT(*) FROM family;
SELECT COUNT(*) FROM user;
SELECT COUNT(*) FROM lists;
SELECT COUNT(*) FROM gifts;
SELECT COUNT(*) FROM gift_participation;
```

## Détails des transformations

### Table `family`
- **Ajouté**: `created_at` (NOW())

### Table `user`
- **Ajouté**: `is_admin` (1 pour user id=1, 0 pour les autres)
- **Ajouté**: `created_at` (NOW())
- **Ajouté**: `updated_at` (NOW())

### Table `lists`
- **Supprimé**: `filename` (n'existe plus dans la nouvelle structure)
- **Ajouté**: `created_at` (NOW())
- **Ajouté**: `updated_at` (NOW())

### Table `gifts`
- **Ajouté**: `is_group_gift` (0 par défaut)
- **Ajouté**: `created_at` (NOW())
- **Ajouté**: `updated_at` (NOW())

### Table `gift_participation`
- **Ajouté**: `created_at` (NOW())

## Notes importantes

1. **Ordre d'exécution**: Il est crucial d'exécuter `import_old_data.sql` AVANT `import_gifts_data.sql` car les cadeaux dépendent des utilisateurs et des listes.

2. **Contraintes de clés étrangères**: Les scripts désactivent temporairement les contraintes (`SET FOREIGN_KEY_CHECKS = 0`) pour éviter les problèmes d'ordre d'insertion, puis les réactivent à la fin.

3. **Encodage**: Assurez-vous que les fichiers sont en UTF-8 pour éviter les problèmes d'accents et de caractères spéciaux.

4. **Table opengraph_request**: Cette table est nouvelle et ne contient aucune donnée dans l'ancien dump. Elle sera créée vide par Entity Framework.

## Dépannage

### Erreur "Table already exists"
Si vous obtenez cette erreur, c'est que la base n'a pas été correctement droppée. Recommencez à l'Étape 2.

### Erreur "Foreign key constraint fails"
Vérifiez que vous avez bien exécuté `import_old_data.sql` avant `import_gifts_data.sql`.

### Python n'est pas installé
Vous pouvez utiliser le script PowerShell à la place (`transform_dump_data.ps1`), mais le script Python est recommandé pour sa robustesse.

### Les caractères spéciaux sont mal affichés
Vérifiez l'encodage des fichiers. Utilisez UTF-8 sans BOM.
