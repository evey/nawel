# Plan d'Action - Migration Transparente MD5 → BCrypt

## 📋 Analyse de la Situation

### Problème Actuel
- ✅ Détection des mots de passe MD5 fonctionnelle (`AuthService.cs:31`)
- ❌ Retourne simplement "Invalid credentials" - pas d'info utilisateur
- ❌ Aucun moyen pour l'utilisateur de débloquer son compte
- ❌ Après migration des données, **TOUS les utilisateurs seront bloqués**

### Code Existant à Utiliser
- ✅ Flow de reset password complet déjà en place
- ✅ `GenerateResetTokenAsync(email)` fonctionnel
- ✅ `ResetPasswordAsync(token, newPassword)` fonctionnel
- ✅ EmailService configuré (SMTP)

---

## 🎯 Objectif
Permettre aux utilisateurs avec mot de passe MD5 de le migrer vers BCrypt **de manière transparente et guidée**, sans friction.

---

## 📝 Plan d'Action Détaillé

### **Phase 1 : Backend - Détection et Communication** ⚙️

#### 1.1 Créer une Exception Spécifique
**Fichier**: `backend/Nawel.Api/Exceptions/LegacyPasswordException.cs`
```csharp
public class LegacyPasswordException : Exception
{
    public string Email { get; }
    public string Login { get; }

    public LegacyPasswordException(string login, string email)
        : base("Legacy MD5 password detected. Password reset required.")
    {
        Login = login;
        Email = email;
    }
}
```

#### 1.2 Modifier AuthService
**Fichier**: `backend/Nawel.Api/Services/Auth/AuthService.cs`
**Action**: Au lieu de retourner `null`, throw `LegacyPasswordException`

**Avant** (ligne 31-38):
```csharp
if (user.Password.Length == 32 && !user.Password.StartsWith("$2"))
{
    _logger.LogWarning(...);
    return null; // ❌
}
```

**Après**:
```csharp
if (user.Password.Length == 32 && !user.Password.StartsWith("$2"))
{
    _logger.LogWarning(...);
    throw new LegacyPasswordException(user.Login, user.Email ?? "");
}
```

#### 1.3 Modifier AuthController
**Fichier**: `backend/Nawel.Api/Controllers/AuthController.cs`
**Action**: Catch `LegacyPasswordException` et retourner une réponse spécifique

```csharp
[HttpPost("login")]
public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
{
    try
    {
        var user = await _authService.AuthenticateAsync(request.Login, request.Password);
        // ... suite
    }
    catch (LegacyPasswordException ex)
    {
        return Unauthorized(new
        {
            code = "LEGACY_PASSWORD",
            message = "Votre mot de passe doit être réinitialisé pour des raisons de sécurité",
            email = ex.Email,
            requiresReset = true
        });
    }
    // ... autres catches
}
```

#### 1.4 Ajouter un Endpoint de Migration Automatique
**Fichier**: `backend/Nawel.Api/Controllers/AuthController.cs`
**Nouveau endpoint**:

```csharp
[HttpPost("request-migration-reset")]
public async Task<ActionResult> RequestMigrationReset([FromBody] RequestMigrationResetDto request)
{
    try
    {
        // Vérifier que l'utilisateur existe et a bien un mot de passe MD5
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Login == request.Login);

        if (user == null || string.IsNullOrEmpty(user.Email))
        {
            // Ne pas révéler si l'utilisateur existe
            return Ok(new { message = "Si votre compte nécessite une migration, un email a été envoyé" });
        }

        // Vérifier que c'est bien un mot de passe MD5
        if (user.Password.Length == 32 && !user.Password.StartsWith("$2"))
        {
            // Générer le token de reset
            var token = await _authService.GenerateResetTokenAsync(user.Email);

            // Envoyer l'email de migration
            await _emailService.SendMigrationResetEmailAsync(user.Email, user.FirstName ?? user.Login, token);

            _logger.LogInformation("Migration reset requested for user {Login} (ID: {UserId})", user.Login, user.Id);
        }

        return Ok(new { message = "Si votre compte nécessite une migration, un email a été envoyé" });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error during migration reset request for login {Login}", request.Login);
        return StatusCode(500, new { message = "Une erreur est survenue" });
    }
}
```

#### 1.5 Créer le DTO
**Fichier**: `backend/Nawel.Api/DTOs/RequestMigrationResetDto.cs`
```csharp
public class RequestMigrationResetDto
{
    [Required]
    public string Login { get; set; } = string.Empty;
}
```

#### 1.6 Ajouter une Méthode EmailService
**Fichier**: `backend/Nawel.Api/Services/Email/EmailService.cs`

```csharp
public async Task SendMigrationResetEmailAsync(string toEmail, string userName, string resetToken)
{
    var subject = "🔐 Mise à jour de sécurité - Réinitialisation de mot de passe requise";

    var body = $@"
        <h2>Bonjour {userName},</h2>

        <p>Pour améliorer la sécurité de votre compte Nawel, nous avons mis à niveau notre système de sécurité.</p>

        <p><strong>Votre mot de passe doit être réinitialisé.</strong></p>

        <p>Cliquez sur le lien ci-dessous pour créer un nouveau mot de passe sécurisé :</p>

        <p>
            <a href='http://localhost:5173/reset-password?token={resetToken}'
               style='background-color: #4CAF50; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>
                Réinitialiser mon mot de passe
            </a>
        </p>

        <p><em>Ce lien est valide pendant 24 heures.</em></p>

        <hr>
        <p style='font-size: 12px; color: #666;'>
            Si vous n'avez pas demandé cette réinitialisation, ignorez cet email.<br>
            Votre mot de passe actuel reste inchangé jusqu'à ce que vous en créiez un nouveau.
        </p>
    ";

    await SendEmailAsync(toEmail, subject, body);
}
```

---

### **Phase 2 : Frontend - UX Transparente** 🎨

#### 2.1 Modifier la Page Login
**Fichier**: `frontend/nawel-app/src/pages/Login.tsx`

**Ajouter un state**:
```typescript
const [legacyPasswordDetected, setLegacyPasswordDetected] = useState(false);
const [userEmail, setUserEmail] = useState('');
const [migrationEmailSent, setMigrationEmailSent] = useState(false);
```

**Modifier le handleSubmit**:
```typescript
const handleSubmit = async (e: React.FormEvent) => {
  e.preventDefault();
  setError('');
  setLegacyPasswordDetected(false);

  try {
    const result = await login({ login: loginValue, password });

    if (result.success) {
      navigate('/');
    } else {
      setError(result.error || 'Identifiants invalides');
    }
  } catch (err: any) {
    // Détecter l'erreur spécifique MD5
    if (err.response?.data?.code === 'LEGACY_PASSWORD') {
      setLegacyPasswordDetected(true);
      setUserEmail(err.response.data.email);
      setError('');
    } else {
      setError(err.response?.data?.message || 'Erreur de connexion');
    }
  }
};
```

**Ajouter une fonction pour la migration**:
```typescript
const handleRequestMigration = async () => {
  try {
    const response = await authAPI.requestMigrationReset({ login: loginValue });
    setMigrationEmailSent(true);
  } catch (err) {
    setError('Erreur lors de l\'envoi de l\'email');
  }
};
```

#### 2.2 Ajouter l'UI pour la Migration
**Dans le JSX de Login.tsx**:

```tsx
{legacyPasswordDetected && !migrationEmailSent && (
  <Alert severity="warning" sx={{ mt: 2 }}>
    <AlertTitle>Mise à jour de sécurité requise</AlertTitle>
    <Typography variant="body2" sx={{ mb: 2 }}>
      Pour améliorer la sécurité de votre compte, votre mot de passe doit être réinitialisé.
    </Typography>
    <Button
      variant="contained"
      color="primary"
      onClick={handleRequestMigration}
      fullWidth
    >
      Recevoir un email de réinitialisation
    </Button>
    {userEmail && (
      <Typography variant="caption" sx={{ mt: 1, display: 'block' }}>
        L'email sera envoyé à : {userEmail}
      </Typography>
    )}
  </Alert>
)}

{migrationEmailSent && (
  <Alert severity="success" sx={{ mt: 2 }}>
    <AlertTitle>Email envoyé !</AlertTitle>
    <Typography variant="body2">
      Consultez votre boîte mail ({userEmail}) pour réinitialiser votre mot de passe.
    </Typography>
  </Alert>
)}
```

#### 2.3 Ajouter la Méthode API
**Fichier**: `frontend/nawel-app/src/services/api.ts`

```typescript
export const authAPI = {
  // ... méthodes existantes

  requestMigrationReset: (data: { login: string }) =>
    api.post('/auth/request-migration-reset', data),
};
```

---

### **Phase 3 : Tests** 🧪

#### 3.1 Tests Backend
**Fichier**: `backend/Nawel.Api.Tests/Integration/MD5MigrationFlowTests.cs`

```csharp
[Fact]
public async Task Login_WithMD5Password_ReturnsLegacyPasswordError()
{
    // Arrange - User with MD5 password

    // Act - Try to login

    // Assert - Should get LEGACY_PASSWORD code
}

[Fact]
public async Task RequestMigrationReset_WithMD5User_SendsEmail()
{
    // Test the migration reset endpoint
}
```

#### 3.2 Tests Frontend
**Fichier**: `frontend/nawel-app/src/pages/Login.test.tsx`

```typescript
it('should show migration UI when legacy password detected', async () => {
  // Mock API to return LEGACY_PASSWORD error
  // Render Login
  // Fill form and submit
  // Expect migration UI to appear
});
```

---

## 🎬 Flow Utilisateur Final

1. **Utilisateur tente de se connecter** avec login/password MD5
2. **Frontend détecte** l'erreur `LEGACY_PASSWORD`
3. **Affichage message clair** : "Mise à jour de sécurité requise"
4. **Bouton "Recevoir email"** → Call API `request-migration-reset`
5. **Backend génère token** et envoie email avec template spécifique
6. **Utilisateur clique** sur le lien dans l'email
7. **Page reset-password** s'ouvre avec le token pré-rempli
8. **Utilisateur entre** nouveau mot de passe
9. **Backend hash en BCrypt** et sauvegarde
10. **Connexion fonctionne** ✅

---

## ✅ Checklist d'Implémentation

### Backend
- [ ] Créer `LegacyPasswordException.cs`
- [ ] Modifier `AuthService.AuthenticateAsync()` pour throw exception
- [ ] Modifier `AuthController.Login()` pour catch exception
- [ ] Créer `RequestMigrationResetDto.cs`
- [ ] Ajouter endpoint `POST /auth/request-migration-reset`
- [ ] Ajouter `SendMigrationResetEmailAsync()` dans EmailService
- [ ] Tests unitaires AuthService
- [ ] Tests d'intégration migration flow

### Frontend
- [ ] Ajouter states dans Login.tsx
- [ ] Modifier handleSubmit pour détecter LEGACY_PASSWORD
- [ ] Ajouter handleRequestMigration
- [ ] Ajouter UI Alert/Button migration
- [ ] Ajouter méthode API `requestMigrationReset`
- [ ] Tests composant Login

### Documentation
- [ ] Documenter le processus de migration
- [ ] Ajouter section dans README
- [ ] Script de migration de données (si nécessaire)

---

## 🚀 Ordre d'Exécution Recommandé

1. **Backend Exception & Service** (30 min)
2. **Backend Controller & Endpoint** (30 min)
3. **Backend Email Template** (15 min)
4. **Frontend Detection & UI** (45 min)
5. **Tests Backend** (30 min)
6. **Tests Frontend** (20 min)
7. **Test E2E Manuel** (20 min)

**Total estimé : ~3h30**

---

## 📌 Notes Importantes

- ⚠️ **Sécurité** : Ne jamais révéler si un login existe ou non
- ⚠️ **Email** : S'assurer que SMTP est configuré en production
- ⚠️ **Token** : Les tokens de reset expirent après 24h
- ⚠️ **Logs** : Garder les logs pour tracer les migrations
- ✅ **UX** : Le processus doit être clair et rassurant
