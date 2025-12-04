# Installation et Configuration Docker + MySQL pour Nawel

Ce guide vous permettra de mettre en place l'environnement Docker avec MySQL pour tester l'application en local.

## Prérequis

### 1. Installer Docker Desktop

1. Téléchargez Docker Desktop pour Windows depuis : https://www.docker.com/products/docker-desktop/
2. Exécutez l'installateur
3. Redémarrez votre ordinateur si nécessaire
4. Lancez Docker Desktop
5. Vérifiez l'installation en ouvrant un terminal PowerShell et en tapant :
   ```bash
   docker --version
   docker-compose --version
   ```

## Configuration de l'environnement

### 2. Fichier docker-compose.yml

Le fichier `docker-compose.yml` est déjà présent à la racine du projet. Il configure :
- Une base de données MySQL 8.0
- Un réseau Docker pour la communication

### 3. Fichier .env (à créer)

Créez un fichier `.env` à la racine du projet avec le contenu suivant :

```env
# Database Configuration
MYSQL_ROOT_PASSWORD=root_password_secure_123
MYSQL_DATABASE=nawel_db
MYSQL_USER=nawel_user
MYSQL_PASSWORD=nawel_password_secure_456

# Connection String for the API
DATABASE_CONNECTION_STRING=Server=localhost;Port=3306;Database=nawel_db;User=nawel_user;Password=nawel_password_secure_456;
```

**⚠️ Important** : Ajoutez `.env` à votre `.gitignore` pour ne pas commiter les mots de passe !

### 4. Mise à jour du .gitignore

Ajoutez ces lignes à votre fichier `.gitignore` :

```
# Environment variables
.env

# Database files
*.db
*.db-shm
*.db-wal
```

## Démarrage de MySQL avec Docker

### 5. Lancer MySQL

Ouvrez un terminal à la racine du projet et exécutez :

```bash
docker-compose up -d
```

Cette commande va :
- Télécharger l'image MySQL si nécessaire
- Créer et démarrer le conteneur MySQL
- Exposer le port 3306 sur votre machine locale

### 6. Vérifier que MySQL tourne

```bash
docker-compose ps
```

Vous devriez voir un conteneur `nawel-mysql-1` avec le statut "Up".

### 7. Vérifier les logs MySQL (optionnel)

```bash
docker-compose logs mysql
```

## Configuration de l'API pour utiliser MySQL

### 8. Modifier appsettings.Development.json

Le fichier `backend/Nawel.Api/appsettings.Development.json` contient déjà la configuration pour basculer entre SQLite et MySQL.

Pour utiliser MySQL, modifiez la section `ConnectionStrings` :

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=nawel_db;User=nawel_user;Password=nawel_password_secure_456;"
  },
  "UseMySQL": true,
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  }
}
```

**Note** : Assurez-vous que `"UseMySQL": true` est bien présent.

### 9. Installer les packages NuGet nécessaires (si pas déjà fait)

Si ce n'est pas déjà fait, installez le package MySQL pour Entity Framework :

```bash
cd backend/Nawel.Api
dotnet add package Pomelo.EntityFrameworkCore.MySql
```

### 10. Appliquer les migrations sur MySQL

```bash
cd backend/Nawel.Api
dotnet ef database update
```

Cette commande va créer toutes les tables dans MySQL.

## Démarrage de l'application

### 11. Lancer le Backend

```bash
cd backend/Nawel.Api
dotnet run
```

Le backend devrait se connecter à MySQL et initialiser les données de seed (utilisateurs par défaut).

### 12. Lancer le Frontend

Ouvrez un autre terminal :

```bash
cd frontend/nawel-app
npm run dev
```

### 13. Tester l'application

Ouvrez votre navigateur sur : http://localhost:5173

Connectez-vous avec un des comptes de test :
- Login : `sylvain` / Password : `password123`
- Login : `claire` / Password : `password123`
- Login : `marie` / Password : `password123`

## Commandes Docker utiles

### Arrêter MySQL
```bash
docker-compose stop
```

### Redémarrer MySQL
```bash
docker-compose start
```

### Arrêter et supprimer les conteneurs
```bash
docker-compose down
```

### Arrêter et supprimer les conteneurs + les volumes (⚠️ SUPPRIME TOUTES LES DONNÉES)
```bash
docker-compose down -v
```

### Se connecter au conteneur MySQL (pour debug)
```bash
docker exec -it nawel-mysql-1 mysql -u nawel_user -p
# Entrez le mot de passe : nawel_password_secure_456
```

### Voir les logs en temps réel
```bash
docker-compose logs -f mysql
```

## Accès à la base de données MySQL

### Avec un client MySQL (optionnel)

Vous pouvez utiliser MySQL Workbench, DBeaver, ou tout autre client MySQL :

**Paramètres de connexion :**
- Host : `localhost`
- Port : `3306`
- Database : `nawel_db`
- User : `nawel_user`
- Password : `nawel_password_secure_456`

## Sauvegarde et restauration

### Sauvegarder la base de données

```bash
docker exec nawel-mysql-1 mysqldump -u nawel_user -pnawel_password_secure_456 nawel_db > backup_$(date +%Y%m%d_%H%M%S).sql
```

### Restaurer une sauvegarde

```bash
docker exec -i nawel-mysql-1 mysql -u nawel_user -pnawel_password_secure_456 nawel_db < backup_YYYYMMDD_HHMMSS.sql
```

## Résolution de problèmes

### Le port 3306 est déjà utilisé

Si vous avez déjà MySQL installé localement, il utilise probablement le port 3306.

**Solution 1** : Arrêtez votre MySQL local temporairement

**Solution 2** : Modifiez le port dans `docker-compose.yml` :
```yaml
ports:
  - "3307:3306"  # Utilisez le port 3307 sur votre machine
```

Puis modifiez la chaîne de connexion dans `appsettings.Development.json` :
```json
"DefaultConnection": "Server=localhost;Port=3307;Database=nawel_db;User=nawel_user;Password=nawel_password_secure_456;"
```

### Docker Desktop ne démarre pas

- Vérifiez que la virtualisation est activée dans votre BIOS
- Assurez-vous que WSL 2 est installé (Docker Desktop l'installera automatiquement)
- Redémarrez votre ordinateur

### L'API ne se connecte pas à MySQL

1. Vérifiez que le conteneur MySQL tourne : `docker-compose ps`
2. Vérifiez les logs : `docker-compose logs mysql`
3. Vérifiez que `"UseMySQL": true` est bien dans `appsettings.Development.json`
4. Vérifiez que les migrations sont appliquées : `dotnet ef database update`

### Réinitialiser complètement la base de données

```bash
# Arrêter et supprimer tout
docker-compose down -v

# Redémarrer
docker-compose up -d

# Attendre quelques secondes que MySQL démarre
sleep 10

# Appliquer les migrations
cd backend/Nawel.Api
dotnet ef database update
```

## Basculer entre SQLite et MySQL

### Pour utiliser SQLite (développement rapide)

Dans `appsettings.Development.json` :
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=nawel.db"
  },
  "UseMySQL": false
}
```

### Pour utiliser MySQL (test en conditions réelles)

Dans `appsettings.Development.json` :
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=nawel_db;User=nawel_user;Password=nawel_password_secure_456;"
  },
  "UseMySQL": true
}
```

## Prochaines étapes

Une fois que tout fonctionne en local avec MySQL, vous pourrez :
1. Déployer l'application sur un serveur avec Docker
2. Utiliser Docker Compose pour déployer le backend + MySQL ensemble
3. Configurer un reverse proxy (nginx) pour exposer l'API

---

**Besoin d'aide ?** Vérifiez les logs de Docker et de l'API pour identifier les problèmes.
