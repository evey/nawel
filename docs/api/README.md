# Documentation API - Swagger/OpenAPI

## Accès à Swagger UI

Une fois l'application backend lancée, Swagger UI est accessible à l'adresse suivante :

```
http://localhost:5000/swagger
```

Swagger UI fournit une documentation interactive et auto-générée de toute l'API Nawel, avec la possibilité de tester directement les endpoints depuis l'interface.

## Fonctionnalités

### 1. Documentation Complète

Tous les endpoints sont documentés avec :
- **Descriptions** des opérations
- **Paramètres** requis et optionnels
- **Codes de réponse HTTP** avec leurs significations
- **Schémas de données** (DTOs) pour les requêtes et réponses
- **Exemples** de payloads

### 2. Authentification JWT

La plupart des endpoints nécessitent une authentification JWT. Pour tester ces endpoints dans Swagger UI :

1. **Obtenir un token** :
   - Utilisez l'endpoint `POST /api/Auth/login`
   - Fournissez vos identifiants (login + password)
   - Copiez le token JWT retourné dans la réponse

2. **Configurer l'authentification** (à implémenter dans une version future) :
   - Cliquez sur le bouton "Authorize" en haut à droite
   - Entrez `Bearer {votre-token}` dans le champ
   - Validez

Pour l'instant, vous devrez ajouter manuellement le header `Authorization: Bearer {token}` dans vos tests.

### 3. Test des Endpoints

Pour chaque endpoint, vous pouvez :
- Cliquer sur "Try it out"
- Remplir les paramètres requis
- Cliquer sur "Execute"
- Voir la réponse complète (code HTTP, body, headers)

## Organisation de l'API

L'API est organisée en 6 contrôleurs principaux :

### AuthController
**Authentification et gestion des mots de passe**
- `POST /api/Auth/login` - Connexion utilisateur
- `POST /api/Auth/reset-password-request` - Demande de réinitialisation
- `POST /api/Auth/reset-password` - Réinitialisation du mot de passe
- `POST /api/Auth/request-migration-reset` - Migration MD5 → BCrypt
- `GET /api/Auth/validate-token` - Validation de token JWT

### AdminController
**Administration (réservé aux administrateurs)**
- `GET /api/Admin/stats` - Statistiques globales
- `GET /api/Admin/users` - Liste des utilisateurs
- `POST /api/Admin/users` - Créer un utilisateur
- `PUT /api/Admin/users/{id}` - Modifier un utilisateur
- `DELETE /api/Admin/users/{id}` - Supprimer un utilisateur
- `GET /api/Admin/families` - Liste des familles
- `POST /api/Admin/families` - Créer une famille
- `PUT /api/Admin/families/{id}` - Modifier une famille
- `DELETE /api/Admin/families/{id}` - Supprimer une famille

### UsersController
**Gestion des profils utilisateurs**
- `GET /api/Users/me` - Profil de l'utilisateur connecté
- `GET /api/Users/{id}` - Profil d'un utilisateur spécifique
- `PUT /api/Users/me` - Modifier son profil
- `POST /api/Users/me/change-password` - Changer son mot de passe
- `POST /api/Users/me/avatar` - Upload d'avatar
- `DELETE /api/Users/me/avatar` - Supprimer son avatar

### ListsController
**Gestion des listes de cadeaux**
- `GET /api/Lists` - Toutes les listes (par famille)
- `GET /api/Lists/mine` - Ma liste de cadeaux

### GiftsController
**Gestion des cadeaux (CRUD, réservations, imports)**
- `GET /api/Gifts/years` - Années disponibles
- `POST /api/Gifts/import-from-year/{year}` - Import depuis une année passée
- `GET /api/Gifts/my-list` - Mes cadeaux (année spécifique)
- `GET /api/Gifts/manage-child/{childId}` - Cadeaux d'un enfant (parent)
- `GET /api/Gifts/{userId}` - Cadeaux d'un utilisateur
- `POST /api/Gifts/manage-child/{childId}` - Créer cadeau pour enfant
- `POST /api/Gifts` - Créer un cadeau
- `PUT /api/Gifts/{id}` - Modifier un cadeau
- `DELETE /api/Gifts/{id}` - Supprimer un cadeau
- `POST /api/Gifts/{id}/reserve` - Réserver un cadeau
- `DELETE /api/Gifts/{id}/reserve` - Annuler une réservation

### ProductsController
**Extraction d'informations produits**
- `POST /api/Products/extract-info` - Extraire métadonnées depuis une URL

## Formats de Données

### Codes de Réponse HTTP

L'API utilise les codes HTTP standards :

| Code | Signification | Exemple d'usage |
|------|---------------|-----------------|
| **200 OK** | Succès | Données retournées avec succès |
| **201 Created** | Ressource créée | Cadeau créé avec succès |
| **204 No Content** | Succès sans contenu | Suppression réussie |
| **400 Bad Request** | Requête invalide | Paramètres manquants ou incorrects |
| **401 Unauthorized** | Non authentifié | Token JWT manquant ou invalide |
| **403 Forbidden** | Accès interdit | Tentative d'accès sans permissions |
| **404 Not Found** | Ressource introuvable | Cadeau ou utilisateur inexistant |
| **500 Internal Server Error** | Erreur serveur | Erreur technique côté backend |

### Format JSON

Toutes les requêtes et réponses utilisent JSON avec la convention **camelCase** :

```json
{
  "id": 42,
  "firstName": "John",
  "lastName": "Doe",
  "email": "john@example.com",
  "createdAt": "2025-01-01T00:00:00Z"
}
```

## Exemples d'Utilisation

### Exemple 1 : Authentification

**Requête** :
```http
POST /api/Auth/login HTTP/1.1
Content-Type: application/json

{
  "login": "john_doe",
  "password": "mon_mot_de_passe"
}
```

**Réponse** :
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": {
    "id": 42,
    "login": "john_doe",
    "firstName": "John",
    "lastName": "Doe",
    "email": "john@example.com",
    "avatar": "uploads/avatars/user_42_abc123.jpg",
    "isAdmin": false,
    "isChildren": false,
    "familyId": 5,
    "familyName": "Famille Doe"
  }
}
```

### Exemple 2 : Créer un Cadeau

**Requête** :
```http
POST /api/Gifts HTTP/1.1
Authorization: Bearer {votre_token_jwt}
Content-Type: application/json

{
  "name": "Nintendo Switch",
  "description": "Console de jeu portable",
  "url": "https://www.exemple.com/nintendo-switch",
  "price": 299.99,
  "imageUrl": "https://www.exemple.com/image.jpg",
  "isGroupGift": false
}
```

**Réponse** :
```json
{
  "id": 123,
  "name": "Nintendo Switch",
  "description": "Console de jeu portable",
  "url": "https://www.exemple.com/nintendo-switch",
  "price": 299.99,
  "imageUrl": "https://www.exemple.com/image.jpg",
  "year": 2025,
  "isTaken": false,
  "isGroupGift": false,
  "participantCount": 0
}
```

### Exemple 3 : Réserver un Cadeau

**Requête** :
```http
POST /api/Gifts/123/reserve HTTP/1.1
Authorization: Bearer {votre_token_jwt}
Content-Type: application/json

{
  "comment": "Je participe avec plaisir !"
}
```

**Réponse** :
```json
{
  "message": "Gift reserved successfully"
}
```

## Génération du XML de Documentation

Le fichier XML de documentation est automatiquement généré lors du build du projet grâce à la configuration suivante dans `Nawel.Api.csproj` :

```xml
<PropertyGroup>
  <GenerateDocumentationFile>true</GenerateDocumentationFile>
  <NoWarn>$(NoWarn);1591</NoWarn>
</PropertyGroup>
```

Le fichier `Nawel.Api.xml` est créé dans le répertoire de sortie (`bin/Debug/net9.0/`) et est automatiquement chargé par Swagger au démarrage de l'application.

## Conseils d'Utilisation

### Pour les Développeurs

- **Testez systématiquement** vos endpoints dans Swagger UI après chaque modification
- **Maintenez à jour** les annotations XML (`/// <summary>`, `/// <param>`, etc.) dans les contrôleurs
- **Vérifiez** que tous les DTOs ont des propriétés bien documentées
- **Utilisez** les attributs `[ProducesResponseType]` pour documenter tous les codes de réponse possibles

### Pour les Testeurs / QA

- **Explorez** tous les endpoints dans Swagger UI pour comprendre l'API
- **Utilisez** l'onglet "Schemas" en bas de la page pour voir tous les modèles de données
- **Testez** les cas d'erreur (401, 403, 404) en plus des cas de succès
- **Documentez** les bugs trouvés avec les payloads exacts utilisés

### Pour les Intégrateurs Frontend

- **Référez-vous** à Swagger comme source de vérité pour les contrats d'API
- **Copiez** les schémas JSON pour créer vos types TypeScript
- **Testez** vos intégrations avec de vraies requêtes depuis Swagger UI
- **Signalez** toute incohérence entre Swagger et le comportement réel de l'API

## Limitations Actuelles

1. **Pas de support Bearer Token UI** : L'authentification JWT doit être ajoutée manuellement dans les headers pour l'instant
2. **Pas de versionning d'API** : Une seule version (v1) est disponible
3. **Pas de webhooks documentés** : Seulement les endpoints REST

## Améliorations Futures

- [ ] Ajouter le support complet de l'authentification JWT dans Swagger UI
- [ ] Implémenter des exemples de requêtes/réponses plus détaillés
- [ ] Ajouter la documentation des webhooks si implémentés
- [ ] Générer automatiquement des clients API (TypeScript, C#) depuis Swagger

## Références

- [Documentation Swashbuckle](https://github.com/domaindrivendev/Swashbuckle.AspNetCore)
- [OpenAPI Specification](https://swagger.io/specification/)
- [ASP.NET Core Web API Documentation](https://learn.microsoft.com/en-us/aspnet/core/tutorials/web-api-help-pages-using-swagger)
