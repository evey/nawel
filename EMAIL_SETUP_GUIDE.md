# Guide de Configuration des Emails pour Nawel

Le système d'envoi d'emails a été implémenté dans l'application Nawel. Ce guide explique comment le tester en développement et le configurer en production.

## 📧 Fonctionnalités Email

L'application envoie des emails dans les situations suivantes :

1. **Cadeau réservé** : Notification à **tous les utilisateurs** (sauf le propriétaire de la liste) quand quelqu'un réserve un cadeau
2. **Participation à un cadeau groupé** : Notification à **tous les utilisateurs** (sauf le propriétaire de la liste) quand quelqu'un participe à un cadeau groupé
3. **Liste modifiée** : Notification à **tous les utilisateurs** (sauf celui qui modifie) quand quelqu'un ajoute ou modifie un cadeau dans sa liste

**Important** : Seuls les utilisateurs qui ont activé les notifications correspondantes dans leur profil recevront les emails.

## 🔧 Configuration Actuelle

### Développement
Les emails sont **désactivés par défaut** en développement (`Email:Enabled = false` dans appsettings.json).

Quand désactivés, les emails sont simulés et vous verrez dans les logs :
```
Email sending is disabled. Would have sent to user@example.com: 🎁 Cadeau réservé : ...
```

### Production
Les emails seront activés automatiquement en production via `appsettings.Production.json`.

## 🧪 Option 1 : Tester avec Mailpit (Recommandé pour le développement)

**Mailpit** est un serveur SMTP local qui capture tous les emails et les affiche dans une interface web.

### Installation de Mailpit

#### Sur Windows :
```powershell
# Télécharger depuis https://github.com/axllent/mailpit/releases
# Ou via Chocolatey :
choco install mailpit

# Ou via Scoop :
scoop install mailpit
```

#### Sur Linux/Mac :
```bash
# Via Homebrew (Mac) :
brew install mailpit

# Via apt (Linux) :
sudo apt install mailpit

# Ou télécharger le binaire depuis :
# https://github.com/axllent/mailpit/releases
```

### Démarrer Mailpit

```bash
mailpit
```

Par défaut :
- **SMTP** : `localhost:1025`
- **Interface web** : http://localhost:8025

### Configuration de l'application

Modifiez `backend/Nawel.Api/appsettings.json` :

```json
{
  "Email": {
    "Enabled": true,              // ← Activer les emails
    "SmtpHost": "localhost",
    "SmtpPort": 1025,
    "SmtpUsername": "",
    "SmtpPassword": "",
    "FromEmail": "no-reply@nawel.com",
    "FromName": "Nawel - Listes de Noël",
    "UseSsl": false               // ← Important : false pour Mailpit
  }
}
```

### Tester

1. Redémarrez le backend si nécessaire
2. Connectez-vous à l'application
3. Effectuez une action (réserver un cadeau, ajouter un cadeau, etc.)
4. Ouvrez http://localhost:8025 pour voir les emails capturés

## 🧪 Option 2 : Tester avec MailHog (Alternative)

MailHog est une alternative similaire à Mailpit.

### Installation via Docker :
```bash
docker run -d -p 1025:1025 -p 8025:8025 mailhog/mailhog
```

Configuration identique à Mailpit (même ports).

## 🚀 Configuration Production

Pour la production, vous devrez configurer un vrai serveur SMTP.

### Option 1 : Gmail SMTP

⚠️ **Note** : Nécessite un mot de passe d'application (pas votre mot de passe Gmail normal)

```json
{
  "Email": {
    "Enabled": true,
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": 587,
    "SmtpUsername": "votre-email@gmail.com",
    "SmtpPassword": "votre-mot-de-passe-application",
    "FromEmail": "votre-email@gmail.com",
    "FromName": "Nawel - Listes de Noël",
    "UseSsl": true
  }
}
```

**Créer un mot de passe d'application Gmail** :
1. Allez sur https://myaccount.google.com/security
2. Activez la validation en deux étapes si ce n'est pas déjà fait
3. Allez dans "Mots de passe des applications"
4. Créez un nouveau mot de passe pour "Nawel"

**Limites** : Gmail limite à ~500 emails/jour pour les comptes gratuits

### Option 2 : SendGrid (Recommandé pour la production)

SendGrid offre 100 emails/jour gratuitement.

```json
{
  "Email": {
    "Enabled": true,
    "SmtpHost": "smtp.sendgrid.net",
    "SmtpPort": 587,
    "SmtpUsername": "apikey",
    "SmtpPassword": "votre-api-key-sendgrid",
    "FromEmail": "no-reply@votre-domaine.com",
    "FromName": "Nawel - Listes de Noël",
    "UseSsl": true
  }
}
```

**Configuration SendGrid** :
1. Créez un compte sur https://sendgrid.com
2. Créez une clé API dans Settings > API Keys
3. Vérifiez votre adresse email d'envoi (Sender Authentication)

### Option 3 : Mailgun

```json
{
  "Email": {
    "Enabled": true,
    "SmtpHost": "smtp.mailgun.org",
    "SmtpPort": 587,
    "SmtpUsername": "postmaster@votre-domaine.mailgun.org",
    "SmtpPassword": "votre-mot-de-passe-mailgun",
    "FromEmail": "no-reply@votre-domaine.com",
    "FromName": "Nawel - Listes de Noël",
    "UseSsl": true
  }
}
```

### Option 4 : SMTP de votre hébergeur

La plupart des hébergeurs (OVH, Gandi, etc.) fournissent un serveur SMTP. Consultez leur documentation.

## 🔒 Sécurité en Production

⚠️ **IMPORTANT** : Ne jamais commiter les mots de passe SMTP dans le code source !

### Utiliser les variables d'environnement

Au lieu de mettre les identifiants dans `appsettings.Production.json`, utilisez des variables d'environnement :

```json
{
  "Email": {
    "Enabled": true,
    "SmtpHost": "smtp.sendgrid.net",
    "SmtpPort": 587,
    "SmtpUsername": "apikey",
    "SmtpPassword": "${EMAIL_SMTP_PASSWORD}",
    "FromEmail": "${EMAIL_FROM}",
    "FromName": "Nawel - Listes de Noël",
    "UseSsl": true
  }
}
```

Sur votre serveur, définissez les variables :
```bash
export EMAIL_SMTP_PASSWORD="votre-api-key"
export EMAIL_FROM="no-reply@votre-domaine.com"
```

## 📊 Préférences utilisateur

Les utilisateurs peuvent activer/désactiver les notifications dans leur profil :

- **NotifyListEdit** : Recevoir un email quand **quelqu'un d'autre** modifie sa liste
- **NotifyGiftTaken** : Recevoir un email quand quelqu'un réserve/participe à un cadeau (d'un autre utilisateur)

Ces préférences sont déjà implémentées dans l'interface utilisateur (page Profil).

**Note** : Vous ne recevrez jamais d'emails concernant vos propres actions (vos propres modifications de liste ou réservations sur votre liste).

## 🎨 Templates d'emails

Les emails sont au format HTML avec le thème de Noël (vert et or). Les templates sont dans `backend/Nawel.Api/Services/Email/EmailService.cs`.

Caractéristiques :
- Design responsive
- Thème de Noël avec dégradé vert
- Emojis festifs 🎄 🎁 🎅
- Support du pré-formatage pour les commentaires multi-lignes

## 🐛 Dépannage

### Les emails ne sont pas envoyés

1. Vérifiez que `Email:Enabled` est à `true`
2. Vérifiez que l'utilisateur a activé les notifications dans son profil
3. Vérifiez que l'utilisateur a une adresse email valide
4. Vérifiez les logs du backend pour les erreurs

### Erreur SMTP

```
Failed to send email to user@example.com
```

Vérifiez :
- Les identifiants SMTP (host, port, username, password)
- Le paramètre `UseSsl` (true pour les services externes, false pour Mailpit)
- Que le serveur SMTP est accessible depuis votre réseau

### Mailpit ne démarre pas

```bash
# Vérifiez si le port 1025 ou 8025 est déjà utilisé
netstat -an | findstr 1025
netstat -an | findstr 8025

# Changez les ports si nécessaire :
mailpit --smtp-bind-addr 127.0.0.1:1026 --ui-bind-addr 127.0.0.1:8026
```

## 📝 Notes Importantes

1. **En développement** : Les emails sont désactivés par défaut pour ne pas spammer pendant les tests
2. **Mailpit est local** : Il ne peut pas envoyer de vrais emails, il les capture seulement
3. **Production** : N'oubliez pas d'activer les emails et de configurer un vrai service SMTP
4. **Limites** : Respectez les limites de votre service SMTP pour éviter d'être bloqué
5. **Spam** : Assurez-vous de configurer SPF/DKIM/DMARC pour éviter que vos emails finissent en spam

## ✅ Checklist avant mise en production

- [ ] Configurer un service SMTP de production (SendGrid, Mailgun, etc.)
- [ ] Vérifier l'adresse email d'envoi (Sender Authentication)
- [ ] Tester l'envoi d'emails depuis le serveur de production
- [ ] Configurer les variables d'environnement pour les secrets
- [ ] Vérifier que `Email:Enabled` est à `true` dans appsettings.Production.json
- [ ] Tester les notifications avec un vrai compte utilisateur
- [ ] Configurer SPF/DKIM/DMARC pour éviter le spam
