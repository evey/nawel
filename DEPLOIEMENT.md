# Guide de Déploiement - Nawel

## 📦 Préparation du Déploiement

Ce document résume les étapes pour déployer le nouveau système Nawel avec migration des anciennes données.

## ✅ Checklist Pré-Déploiement

### 1. Vérification du Code
- [x] Frontend converti en TypeScript
- [x] Backend avec sécurité renforcée (BCrypt, JWT, rate limiting)
- [x] Tests unitaires et d'intégration en place
- [x] Guides utilisateur intégrés dans l'application (/help)
- [x] Scripts de migration préparés

### 2. Configuration de l'Environnement

**Backend (`backend/Nawel.Api/appsettings.json`)**:
```json
{
  "Jwt": {
    "SecretKey": "[GÉNÉRER UNE CLÉ SÉCURISÉE EN PRODUCTION]",
    "Issuer": "NawelApi",
    "Audience": "NawelApp",
    "ExpirationMinutes": 10080
  },
  "Email": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "SenderEmail": "[VOTRE EMAIL]",
    "SenderPassword": "[MOT DE PASSE APP]",
    "SenderName": "Nawel - Listes de Noël"
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=nawel;User=nawel_user;Password=[PASSWORD];"
  }
}
```

**Frontend** : Les variables d'environnement sont gérées via Vite

### 3. Migration des Données

#### Option A: Utiliser le Script PowerShell (Windows) - **RECOMMANDÉ**

```powershell
# 1. Se placer dans le bon répertoire
cd D:\Perso\nawel\backend\Nawel.Api

# 2. S'assurer que sqlite3 est installé
# Télécharger depuis: https://www.sqlite.org/download.html
# Ajouter au PATH ou placer dans le dossier courant

# 3. Exécuter le script de migration
.\Migrations\migrate_old_to_new.ps1

# Options disponibles:
# .\Migrations\migrate_old_to_new.ps1 -BackupOnly    # Créer uniquement un backup
# .\Migrations\migrate_old_to_new.ps1 -VerifyOnly   # Vérifier les données migrées
# .\Migrations\migrate_old_to_new.ps1 -Help         # Afficher l'aide
```

#### Option B: Utiliser le Script Bash (Linux/Mac)

```bash
# 1. Se placer dans le bon répertoire
cd /path/to/nawel/backend/Nawel.Api

# 2. Donner les permissions d'exécution
chmod +x Migrations/migrate_old_to_new.sh

# 3. Exécuter le script de migration
./Migrations/migrate_old_to_new.sh

# Options disponibles:
# ./Migrations/migrate_old_to_new.sh --backup-only
# ./Migrations/migrate_old_to_new.sh --verify-only
# ./Migrations/migrate_old_to_new.sh --help
```

#### Option C: Migration Manuelle

Consulter le guide détaillé: `backend/Nawel.Api/Migrations/MIGRATION_GUIDE.md`

### 4. Ce que fait le Script de Migration

1. **Sauvegarde automatique** de la base actuelle
2. **Migration des tables**:
   - Familles (2-3 familles)
   - Utilisateurs (~15 utilisateurs avec mots de passe MD5 préservés)
   - Listes (~15 listes)
   - Cadeaux (~3000+ cadeaux sur plusieurs années)
   - Participations aux cadeaux groupés (~100+)
3. **Détection automatique** des cadeaux groupés
4. **Copie des fichiers avatar** depuis `old/uploads/avatars/`
5. **Mise à jour** des séquences d'auto-incrémentation
6. **Vérifications** post-migration

## 🔐 Gestion des Mots de Passe MD5

**Important**: Tous les utilisateurs avec mots de passe MD5 devront les réinitialiser à leur première connexion.

### Flow Automatique:

1. **Utilisateur tente de se connecter** avec son ancien mot de passe
2. **Système détecte MD5** (hash de 32 caractères)
3. **Interface spéciale** s'affiche:
   - Message: "Mise à jour de sécurité requise"
   - Bouton: "Recevoir un email de réinitialisation"
4. **Email envoyé** avec lien sécurisé (valide 24h)
5. **Utilisateur réinitialise** son mot de passe
6. **Nouveau hash BCrypt** créé
7. **Connexion normale** fonctionne

**Documentation complète**: `MIGRATION_MD5_PLAN.md`

## 🚀 Étapes de Déploiement

### 1. Préparation

```bash
# Cloner le repository sur le serveur
git clone [URL_DU_REPO] nawel
cd nawel

# Ou mettre à jour
git pull origin master
```

### 2. Backend

```bash
cd backend/Nawel.Api

# Restaurer les packages
dotnet restore

# Compiler
dotnet build --configuration Release

# Configuration
cp appsettings.json appsettings.Production.json
# Éditer appsettings.Production.json avec les valeurs de production

# Migrer les données
.\Migrations\migrate_old_to_new.ps1  # Windows
# OU
./Migrations/migrate_old_to_new.sh   # Linux/Mac

# Lancer l'application
dotnet run --configuration Release
# OU utiliser un service systemd, IIS, nginx, etc.
```

### 3. Frontend

```bash
cd frontend/nawel-app

# Installer les dépendances
npm install

# Build de production
npm run build

# Le dossier dist/ contient les fichiers statiques à servir
# Copier vers votre serveur web (nginx, Apache, etc.)
```

### 4. Configuration Serveur Web

**Nginx (exemple)**:

```nginx
server {
    listen 80;
    server_name nawel.example.com;

    # Frontend
    location / {
        root /var/www/nawel/frontend/dist;
        try_files $uri $uri/ /index.html;
    }

    # Backend API
    location /api/ {
        proxy_pass http://localhost:5000/;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection 'upgrade';
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
    }

    # Uploads (avatars)
    location /uploads/ {
        alias /var/www/nawel/backend/Nawel.Api/uploads/;
    }
}
```

## 🧪 Tests Post-Déploiement

### 1. Authentification
- [ ] Login avec utilisateur MD5 → Affiche interface de reset
- [ ] Réception de l'email de réinitialisation
- [ ] Reset du mot de passe fonctionne
- [ ] Login avec nouveau mot de passe réussit

### 2. Données
- [ ] Les listes s'affichent correctement
- [ ] Les cadeaux sont visibles par année (2016-2025)
- [ ] Les avatars s'affichent
- [ ] Les cadeaux groupés montrent leurs participants

### 3. Fonctionnalités
- [ ] Réservation d'un cadeau classique
- [ ] Participation à un cadeau groupé
- [ ] Ajout d'un nouveau cadeau
- [ ] Extraction automatique depuis URL (Amazon, etc.)
- [ ] Panier affiche les cadeaux réservés
- [ ] Profil utilisateur modifiable
- [ ] Page d'aide accessible (/help)

### 4. Admin
- [ ] Accès au panel admin (user ID 1)
- [ ] Gestion des familles
- [ ] Gestion des utilisateurs

### 5. Performance
- [ ] Temps de chargement < 2s
- [ ] Navigation fluide entre les pages
- [ ] Pas d'erreurs dans la console

## 📊 Statistiques Attendues Post-Migration

D'après le dump SQL fourni:
- **Familles**: 2-3
- **Utilisateurs**: ~15
- **Listes**: ~15
- **Cadeaux**: ~3000+ (années 2016-2025)
- **Participations**: ~100+
- **Avatars**: ~10-15 fichiers

## 🆘 Dépannage

### Problème: SQLite3 non trouvé (Windows)

**Solution**:
1. Télécharger SQLite depuis https://www.sqlite.org/download.html
2. Extraire `sqlite3.exe`
3. Option A: Ajouter au PATH système
4. Option B: Copier dans `backend\Nawel.Api\`

### Problème: Tous les utilisateurs ne peuvent pas se connecter

**Cause**: Mots de passe MD5
**Solution**: C'est normal ! Suivre le processus de réinitialisation par email

### Problème: Emails non reçus

**Vérifications**:
1. Configuration SMTP correcte dans `appsettings.json`
2. Mot de passe d'application Gmail configuré (si Gmail)
3. Vérifier les logs: `backend/Nawel.Api/logs/`
4. Tester l'envoi d'email manuellement

### Problème: Avatars ne s'affichent pas

**Solution**:
1. Vérifier que les fichiers sont dans `backend/Nawel.Api/uploads/avatars/`
2. Permissions: `chmod 755 uploads/avatars/*` (Linux)
3. Vérifier la configuration du serveur web (nginx, etc.)

### Problème: Erreur lors de la migration

**Solution**:
1. Restaurer le backup: `Copy-Item nawel.db.backup_[DATE] nawel.db -Force`
2. Vérifier les prérequis
3. Consulter `backend/Nawel.Api/Migrations/MIGRATION_GUIDE.md`

## 📂 Structure des Fichiers Importants

```
nawel/
├── backend/
│   └── Nawel.Api/
│       ├── appsettings.json              # Configuration (à adapter pour prod)
│       ├── nawel.db                      # Base de données SQLite
│       ├── uploads/avatars/              # Fichiers avatar
│       └── Migrations/
│           ├── MIGRATION_GUIDE.md        # Guide détaillé
│           ├── migrate_old_to_new.ps1    # Script PowerShell (Windows)
│           ├── migrate_old_to_new.sh     # Script Bash (Linux/Mac)
│           └── 006_migrate_from_old_database.sql
├── frontend/
│   └── nawel-app/
│       ├── dist/                         # Build de production (après npm run build)
│       └── public/
│           └── guides/                   # Guides utilisateur (intégrés)
│               ├── GETTING-STARTED.md
│               └── FEATURES.md
├── old/
│   ├── nironico_nawel.sql               # Base de données source
│   └── uploads/avatars/                 # Avatars source
├── DEPLOIEMENT.md                       # Ce fichier
└── MIGRATION_MD5_PLAN.md                # Plan de migration des mots de passe
```

## 📞 Support

En cas de problème:
1. Consulter les logs: `backend/Nawel.Api/logs/`
2. Vérifier la documentation dans `backend/Nawel.Api/Migrations/MIGRATION_GUIDE.md`
3. Tester en environnement de développement d'abord

## ✅ Checklist Finale Déploiement

- [ ] Backup de l'ancienne base effectué
- [ ] Configuration production renseignée
- [ ] Migration des données réussie
- [ ] Fichiers avatar copiés
- [ ] Backend démarré et accessible
- [ ] Frontend build et déployé
- [ ] Tests d'authentification passés
- [ ] Tests fonctionnels passés
- [ ] Admin peut se connecter
- [ ] Email de réinitialisation fonctionne
- [ ] Documentation utilisateur accessible

---

**Bon déploiement ! 🎄🎁**
