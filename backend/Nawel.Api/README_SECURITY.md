# Configuration de Sécurité - Nawel API

## Variables d'Environnement Requises

Pour des raisons de sécurité, **TOUTES les valeurs sensibles doivent être configurées via des variables d'environnement en production**.

### Configuration JWT (OBLIGATOIRE)

```bash
export JWT_SECRET="votre-secret-securise-minimum-32-caracteres-aleatoires"
export JWT_ISSUER="NawelApi"
export JWT_AUDIENCE="NawelApp"
export JWT_EXPIRATION_MINUTES="60"
```

**⚠️ IMPORTANT**: Le JWT_SECRET doit contenir **au moins 32 caractères** et être **aléatoire**. Utilisez un générateur de mots de passe sécurisé.

Exemple de génération d'un secret sécurisé :
```bash
# Linux/Mac
openssl rand -base64 48

# PowerShell
[Convert]::ToBase64String((1..48 | ForEach-Object { Get-Random -Maximum 256 }))
```

### Base de Données

```bash
export DB_CONNECTION_STRING="Server=localhost;Port=3306;Database=nawel;User=your_user;Password=your_password;"
```

### OpenGraph API (Optionnel)

Pour l'extraction automatique d'informations produit :
```bash
export OPENGRAPH_API_KEY="votre-cle-api-opengraph"
```

Obtenez une clé gratuite sur https://www.opengraph.io/ (1000 requêtes/mois)

### Email SMTP (Optionnel)

```bash
export EMAIL_ENABLED="true"
export SMTP_HOST="smtp.example.com"
export SMTP_PORT="587"
export SMTP_USERNAME="your-email@example.com"
export SMTP_PASSWORD="your-smtp-password"
export SMTP_FROM_EMAIL="no-reply@nawel.com"
export SMTP_FROM_NAME="Nawel - Listes de Noël"
export SMTP_USE_SSL="true"
```

## Déploiement sur VPS

### Méthode 1 : Fichier .env

1. Créez un fichier `.env` dans le répertoire de l'API :
```bash
cd /path/to/backend/Nawel.Api
cp .env.example .env
```

2. Éditez le fichier `.env` avec vos valeurs réelles :
```bash
nano .env
```

3. Chargez les variables avant de lancer l'application :
```bash
set -a
source .env
set +a
dotnet run
```

### Méthode 2 : Service systemd

Créez un service systemd avec les variables d'environnement :

```ini
# /etc/systemd/system/nawel-api.service
[Unit]
Description=Nawel API
After=network.target

[Service]
WorkingDirectory=/var/www/nawel-api
ExecStart=/usr/bin/dotnet /var/www/nawel-api/Nawel.Api.dll
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=nawel-api
User=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=JWT_SECRET=votre-secret-ici-minimum-32-caracteres
Environment=OPENGRAPH_API_KEY=votre-cle-api
Environment=DB_CONNECTION_STRING=Server=localhost;Port=3306;Database=nawel;User=nawel;Password=secret;

[Install]
WantedBy=multi-user.target
```

### Méthode 3 : Docker Compose

```yaml
# docker-compose.yml
version: '3.8'
services:
  backend:
    build: ./backend
    environment:
      - JWT_SECRET=${JWT_SECRET}
      - OPENGRAPH_API_KEY=${OPENGRAPH_API_KEY}
      - DB_CONNECTION_STRING=Server=mysql;Port=3306;Database=nawel;User=root;Password=${MYSQL_ROOT_PASSWORD};
    env_file:
      - .env
```

## Vérification de la Configuration

L'application vérifie automatiquement au démarrage que :
1. Le JWT_SECRET est configuré
2. Le JWT_SECRET contient au moins 32 caractères
3. Si la validation échoue, l'application refusera de démarrer avec une erreur explicite

## Ordre de Priorité

Les valeurs sont lues dans cet ordre (priorité décroissante) :
1. **Variables d'environnement** (recommandé pour production)
2. **appsettings.json** (valeurs par défaut pour développement uniquement)

## Sécurité

- ❌ **NE JAMAIS** commiter de fichiers `.env` ou `appsettings.Production.json` dans Git
- ❌ **NE JAMAIS** utiliser les valeurs par défaut de `appsettings.json` en production
- ✅ Utiliser des secrets forts et aléatoires
- ✅ Changer régulièrement les secrets en production
- ✅ Limiter l'accès aux fichiers de configuration (chmod 600)

## Test de la Configuration

Pour tester si votre configuration est correcte :

```bash
# Vérifier que JWT_SECRET est défini
echo $JWT_SECRET

# Lancer l'application
dotnet run

# Si le secret n'est pas configuré ou trop court, vous verrez :
# InvalidOperationException: JWT Secret must be at least 32 characters long
```

## Support

Pour plus d'informations sur la configuration, consultez le fichier `.env.example`.
