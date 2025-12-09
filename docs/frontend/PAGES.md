# Documentation des Pages - Nawel App

## Vue d'Ensemble

L'application Nawel contient **7 pages principales**, dont 6 pages protégées nécessitant une authentification et 1 page publique (Login).

| Page | Route | Accès | Fichier |
|------|-------|-------|---------|
| Login | `/login` | Public | src/pages/Login.tsx |
| Home | `/` | Protected | src/pages/Home.tsx |
| MyList | `/my-list` | Protected | src/pages/MyList.tsx |
| UserList | `/list/:userId` | Protected | src/pages/UserList.tsx |
| Cart | `/cart` | Protected | src/pages/Cart.tsx |
| Profile | `/profile` | Protected | src/pages/Profile.tsx |
| Admin | `/admin` | Admin only | src/pages/Admin.tsx |

---

## 1. Page Login

### Route
- **Path** : `/login`
- **Accès** : Public
- **Fichier** : `src/pages/Login.tsx`

### Description

Page d'authentification permettant aux utilisateurs de se connecter à l'application avec leur login et mot de passe.

### Fonctionnalités

1. **Formulaire de Login**
   - Champs : Login (username), Password
   - Validation : Champs requis
   - Bouton "Se connecter"

2. **Gestion des Mots de Passe MD5 (Legacy)**
   - Détection automatique des mots de passe MD5
   - Modal d'avertissement avec option de réinitialisation
   - Appel API pour demander un email de migration

3. **Gestion des Erreurs**
   - Identifiants incorrects : Message d'erreur
   - Mot de passe MD5 : Modal spécifique
   - Erreurs réseau : Message générique

4. **Redirection Post-Login**
   - Utilisateur authentifié → Redirect vers `/` (Home)

### State Management

```typescript
const [formData, setFormData] = useState({
  login: '',
  password: ''
});
const [error, setError] = useState<string | null>(null);
const [loading, setLoading] = useState(false);
const [showLegacyPasswordModal, setShowLegacyPasswordModal] = useState(false);
```

### API Calls

| Endpoint | Méthode | Description |
|----------|---------|-------------|
| `/api/Auth/login` | POST | Authentification |
| `/api/Auth/request-migration-reset` | POST | Demande reset pour migration MD5 |

### Composants Utilisés

- `TextField` (MUI) : Champs de formulaire
- `Button` (MUI) : Bouton de soumission
- `Dialog` (MUI) : Modal migration MD5
- `Alert` (MUI) : Messages d'erreur

### Flux Utilisateur

```
1. User accède à /login
2. User entre login + password
3. Click "Se connecter"
   ├─ Si credentials valides → AuthContext.login() → Redirect /
   ├─ Si password MD5 → Modal "Migration requise"
   │   └─ User entre login → Email de reset envoyé
   └─ Si erreur → Message d'erreur affiché
```

### Exemple de Code

```tsx
const handleSubmit = async (e: React.FormEvent) => {
  e.preventDefault();
  setError(null);
  setLoading(true);

  try {
    await login(formData);
    navigate('/');
  } catch (err: any) {
    if (err.response?.data?.code === 'LEGACY_PASSWORD') {
      setShowLegacyPasswordModal(true);
    } else {
      setError('Identifiants incorrects');
    }
  } finally {
    setLoading(false);
  }
};
```

---

## 2. Page Home

### Route
- **Path** : `/`
- **Accès** : Protected
- **Fichier** : `src/pages/Home.tsx`

### Description

Dashboard principal affichant toutes les familles avec leurs membres et permettant d'accéder aux listes de cadeaux.

### Fonctionnalités

1. **Affichage des Familles**
   - Liste groupée par famille
   - Affichage des membres de chaque famille
   - Avatar de chaque utilisateur

2. **Navigation vers les Listes**
   - Click sur un utilisateur → Redirect vers `/list/:userId`
   - Click sur "Ma liste" → Redirect vers `/my-list`

3. **Gestion des Comptes Enfants (Parents)**
   - Badge "Compte enfant" visible sur les enfants
   - Bouton "Gérer" pour les parents
   - Mode "Managing Child" activé → Banner affiché

4. **Badge Admin**
   - Badge "Admin" visible sur les administrateurs

### State Management

```typescript
const [families, setFamilies] = useState<FamilyList[]>([]);
const [loading, setLoading] = useState(true);
const [error, setError] = useState<string | null>(null);
const { user, managingChild, startManagingChild } = useAuth();
```

### API Calls

| Endpoint | Méthode | Description |
|----------|---------|-------------|
| `/api/Lists/all` | GET | Récupère toutes les familles et leurs membres |

### Composants Utilisés

- `Card` (MUI) : Cards pour chaque famille
- `Avatar` (custom) : Avatars utilisateurs
- `Chip` (MUI) : Badges "Enfant", "Admin"
- `Button` (MUI) : Boutons "Gérer", "Voir la liste"
- `ManagingChildBanner` (custom) : Banner en mode gestion enfant

### Flux Utilisateur

```
1. User accède à /
2. Fetch de toutes les familles → listsAPI.getAll()
3. Affichage des familles avec membres
4. User clique sur un membre
   ├─ Si c'est lui-même → Redirect /my-list
   ├─ Si c'est un autre → Redirect /list/:userId
   └─ Si c'est un enfant et user est parent
       └─ Click "Gérer" → startManagingChild() → Redirect /my-list
```

### Logique Spécifique

**Vérification si user peut gérer un enfant** :
```typescript
const canManageChild = (child: UserListInfo) => {
  return user &&
         child.isChildren &&
         user.familyId === child.familyId &&
         user.id !== child.id;
};
```

---

## 3. Page MyList

### Route
- **Path** : `/my-list`
- **Accès** : Protected
- **Fichier** : `src/pages/MyList.tsx`

### Description

Page de gestion de sa propre liste de cadeaux (ou liste d'un enfant si en mode "Managing Child").

### Fonctionnalités

1. **Affichage des Cadeaux**
   - Liste filtrée par année
   - Sélecteur d'année (dropdown)
   - Affichage en grille de cartes

2. **Création de Cadeau**
   - Bouton "Ajouter un cadeau"
   - Modal `GiftFormDialog`
   - Extraction automatique depuis URL (OpenGraph)

3. **Édition de Cadeau**
   - Click sur "Modifier" → Modal pré-rempli
   - Modification de tous les champs

4. **Suppression de Cadeau**
   - Click sur "Supprimer" → Confirmation
   - Suppression immédiate

5. **Import de Cadeaux**
   - Bouton "Importer"
   - Modal `ImportDialog` : Sélection année source
   - Import des cadeaux non réservés de l'année sélectionnée

6. **Filtrage par Année**
   - Dropdown avec années disponibles
   - Année par défaut : Année courante
   - Rafraîchissement automatique à changement d'année

### State Management

```typescript
const [gifts, setGifts] = useState<Gift[]>([]);
const [selectedYear, setSelectedYear] = useState<number>(new Date().getFullYear());
const [availableYears, setAvailableYears] = useState<number[]>([]);
const [loading, setLoading] = useState(false);
const [giftDialogOpen, setGiftDialogOpen] = useState(false);
const [editingGift, setEditingGift] = useState<Gift | undefined>();
const [importDialogOpen, setImportDialogOpen] = useState(false);
const { user, managingChild } = useAuth();
```

### API Calls

| Endpoint | Méthode | Description |
|----------|---------|-------------|
| `/api/Gifts/my-list` | GET | Récupère les cadeaux de l'année (mode normal) |
| `/api/Gifts/manage-child/:id` | GET | Récupère les cadeaux d'un enfant (mode gestion) |
| `/api/Gifts` | POST | Crée un nouveau cadeau |
| `/api/Gifts/:id` | PUT | Met à jour un cadeau |
| `/api/Gifts/:id` | DELETE | Supprime un cadeau |
| `/api/Gifts/available-years` | GET | Récupère les années disponibles |
| `/api/Gifts/import` | POST | Importe des cadeaux d'une année |
| `/api/Products/extract-info` | POST | Extrait infos produit depuis URL |

### Composants Utilisés

- `GiftListItem` (custom) : Item de la liste (avec actions edit/delete)
- `GiftFormDialog` (custom) : Modal de création/édition
- `ImportDialog` (custom) : Modal d'import
- `Select` (MUI) : Sélecteur d'année
- `Button` (MUI) : Boutons d'action
- `CircularProgress` (MUI) : Loader
- `ManagingChildBanner` (custom) : Banner si gestion enfant active

### Flux Utilisateur

```
1. User accède à /my-list
2. Vérification du mode
   ├─ Mode normal → Fetch giftsAPI.getMyGifts(year)
   └─ Mode managing child → Fetch giftsAPI.getChildGifts(childId, year)
3. Affichage de la liste de cadeaux
4. Actions possibles :
   ├─ Click "Ajouter" → GiftFormDialog (création)
   ├─ Click "Modifier" sur un cadeau → GiftFormDialog (édition)
   ├─ Click "Supprimer" → Confirmation → DELETE
   ├─ Click "Importer" → ImportDialog → POST import
   └─ Changement d'année → Re-fetch avec nouvelle année
```

### Extraction Automatique de Produit

```typescript
const handleExtractProductInfo = async (url: string): Promise<ProductInfo | null> => {
  try {
    const productInfo = await productsAPI.extractInfo(url);
    return productInfo;
  } catch (error) {
    console.error('Erreur extraction:', error);
    return null;
  }
};
```

**Champs auto-remplis** :
- Nom du produit
- Description
- Prix
- Image
- Devise

---

## 4. Page UserList

### Route
- **Path** : `/list/:userId`
- **Accès** : Protected
- **Fichier** : `src/pages/UserList.tsx`

### Description

Page de consultation de la liste de cadeaux d'un autre utilisateur avec possibilité de réservation.

### Fonctionnalités

1. **Affichage des Cadeaux**
   - Liste filtrée par année
   - Sélecteur d'année
   - Statut de chaque cadeau (disponible, réservé, groupe)

2. **Réservation de Cadeau**
   - Bouton "Réserver" sur cadeaux disponibles
   - Modal `ReserveDialog` avec champ commentaire
   - Réservation immédiate

3. **Annulation de Réservation**
   - Bouton "Annuler" sur cadeaux réservés par moi
   - Confirmation → Annulation

4. **Participation Cadeau Groupé**
   - Bouton "Participer" sur cadeaux groupés
   - Modal avec commentaire
   - Affichage des participants existants

5. **Filtrage par Année**
   - Dropdown avec années disponibles
   - Historique des listes

6. **Informations Propriétaire**
   - Avatar et nom du propriétaire de la liste
   - Retour à Home via bouton

### State Management

```typescript
const [owner, setOwner] = useState<User | null>(null);
const [gifts, setGifts] = useState<Gift[]>([]);
const [selectedYear, setSelectedYear] = useState<number>(new Date().getFullYear());
const [availableYears, setAvailableYears] = useState<number[]>([]);
const [loading, setLoading] = useState(false);
const [reserveDialogOpen, setReserveDialogOpen] = useState(false);
const [selectedGift, setSelectedGift] = useState<Gift | null>(null);
const { user } = useAuth();
```

### API Calls

| Endpoint | Méthode | Description |
|----------|---------|-------------|
| `/api/Users/:id` | GET | Récupère info du propriétaire |
| `/api/Gifts/user/:userId` | GET | Récupère les cadeaux de l'année |
| `/api/Gifts/:id/reserve` | POST | Réserve un cadeau (ou participe) |
| `/api/Gifts/:id/unreserve` | POST | Annule une réservation |
| `/api/Gifts/available-years/:userId` | GET | Années disponibles |

### Composants Utilisés

- `UserGiftListItem` (custom) : Item avec boutons réserver/participer
- `ReserveDialog` (custom) : Modal de réservation
- `Avatar` (custom) : Avatar du propriétaire
- `Select` (MUI) : Sélecteur d'année
- `Button` (MUI) : Bouton retour
- `Chip` (MUI) : Statuts des cadeaux

### Flux Utilisateur

```
1. User accède à /list/5 (userId=5)
2. Fetch usersAPI.getById(5) → Owner
3. Fetch giftsAPI.getUserGifts(5, year) → Gifts
4. Affichage de la liste
5. Actions possibles :
   ├─ Cadeau disponible (non groupé)
   │   └─ Click "Réserver" → ReserveDialog → POST reserve
   ├─ Cadeau réservé par moi
   │   └─ Click "Annuler" → POST unreserve
   ├─ Cadeau groupé
   │   ├─ Click "Participer" → ReserveDialog → POST reserve
   │   └─ Affichage liste participants
   └─ Changement d'année → Re-fetch
```

### Logique de Statut

```typescript
const getGiftStatus = (gift: Gift, currentUserId: number) => {
  if (gift.isGroupGift) {
    const isParticipating = gift.participants?.some(p => p.userId === currentUserId);
    return {
      label: isParticipating ? 'Vous participez' : 'Cadeau groupé',
      color: isParticipating ? 'success' : 'info',
      canReserve: !isParticipating,
      canUnreserve: isParticipating
    };
  }

  if (!gift.available && gift.takenBy === currentUserId) {
    return {
      label: 'Réservé par vous',
      color: 'success',
      canReserve: false,
      canUnreserve: true
    };
  }

  if (!gift.available) {
    return {
      label: 'Déjà réservé',
      color: 'default',
      canReserve: false,
      canUnreserve: false
    };
  }

  return {
    label: 'Disponible',
    color: 'primary',
    canReserve: true,
    canUnreserve: false
  };
};
```

---

## 5. Page Cart

### Route
- **Path** : `/cart`
- **Accès** : Protected
- **Fichier** : `src/pages/Cart.tsx`

### Description

Page "panier" affichant tous les cadeaux réservés par l'utilisateur avec calcul des totaux.

### Fonctionnalités

1. **Affichage des Réservations**
   - Liste de tous les cadeaux réservés
   - Groupés par propriétaire
   - Affichage du coût par cadeau

2. **Gestion Cadeaux Classiques**
   - Affichage des cadeaux réservés (takenBy = currentUser)
   - Propriétaire du cadeau
   - Commentaire de réservation

3. **Gestion Cadeaux Groupés**
   - Affichage des participations
   - Commentaire de participation
   - Liste des autres participants

4. **Calcul des Totaux**
   - Total par devise (EUR, USD, etc.)
   - Affichage en bas de page

5. **Annulation de Réservation**
   - Bouton "Annuler" sur chaque cadeau
   - Confirmation → Suppression du panier

6. **Filtre par Année**
   - Sélecteur d'année
   - Par défaut : Année courante

### State Management

```typescript
const [reservedGifts, setReservedGifts] = useState<Gift[]>([]);
const [selectedYear, setSelectedYear] = useState<number>(new Date().getFullYear());
const [loading, setLoading] = useState(false);
const [totals, setTotals] = useState<{ [currency: string]: number }>({});
const { user } = useAuth();
```

### API Calls

| Endpoint | Méthode | Description |
|----------|---------|-------------|
| `/api/Gifts/cart` | GET | Récupère le panier (année courante) |
| `/api/Gifts/:id/unreserve` | POST | Annule une réservation |

### Composants Utilisés

- `Card` (MUI) : Cards pour chaque cadeau
- `Typography` (MUI) : Textes
- `Button` (MUI) : Bouton annuler
- `Chip` (MUI) : Badge "Groupe"
- `Avatar` (custom) : Avatar du propriétaire

### Flux Utilisateur

```
1. User accède à /cart
2. Fetch giftsAPI.getCart(year)
3. Calcul des totaux par devise
4. Affichage des cadeaux groupés par propriétaire
5. Actions possibles :
   ├─ Click "Annuler" → Confirmation → POST unreserve → Refresh
   └─ Changement d'année → Re-fetch
```

### Calcul des Totaux

```typescript
useEffect(() => {
  const calculateTotals = () => {
    const totalsMap: { [currency: string]: number } = {};

    reservedGifts.forEach(gift => {
      if (gift.cost) {
        const currency = gift.currency || 'EUR';
        totalsMap[currency] = (totalsMap[currency] || 0) + gift.cost;
      }
    });

    setTotals(totalsMap);
  };

  calculateTotals();
}, [reservedGifts]);
```

---

## 6. Page Profile

### Route
- **Path** : `/profile`
- **Accès** : Protected
- **Fichier** : `src/pages/Profile.tsx`

### Description

Page de gestion du profil utilisateur avec modification des informations personnelles, avatar et mot de passe.

### Fonctionnalités

1. **Informations Personnelles**
   - Formulaire avec tous les champs modifiables
   - FirstName, LastName, Email, Pseudo
   - Préférences de notifications (email)
   - Sauvegarde automatique

2. **Gestion de l'Avatar**
   - Upload d'un fichier image
   - Formats supportés : JPEG, PNG, GIF, WebP
   - Taille max : 5MB
   - Preview avant upload
   - Suppression de l'avatar

3. **Changement de Mot de Passe**
   - Modal dédiée
   - Champs : Ancien mot de passe, Nouveau, Confirmation
   - Validation : Min 6 caractères, correspondance

4. **Affichage des Préférences**
   - Checkboxes pour notifications :
     - `notifyListEdit` : Notification si liste modifiée
     - `notifyGiftTaken` : Notification si cadeau réservé
     - `displayPopup` : Afficher popups info

### State Management

```typescript
const [profile, setProfile] = useState<UpdateProfileData>({
  firstName: '',
  lastName: '',
  email: '',
  pseudo: '',
  notifyListEdit: true,
  notifyGiftTaken: true,
  displayPopup: true
});
const [loading, setLoading] = useState(false);
const [success, setSuccess] = useState(false);
const [passwordDialogOpen, setPasswordDialogOpen] = useState(false);
const { user, updateUser } = useAuth();
```

### API Calls

| Endpoint | Méthode | Description |
|----------|---------|-------------|
| `/api/Users/me` | PUT | Met à jour le profil |
| `/api/Users/me/change-password` | POST | Change le mot de passe |
| `/api/Users/me/avatar` | POST | Upload un avatar |
| `/api/Users/me/avatar` | DELETE | Supprime l'avatar |

### Composants Utilisés

- `ProfileForm` (custom) : Formulaire principal
- `AvatarUpload` (custom) : Upload/suppression avatar
- `PasswordChangeForm` (custom) : Modal changement mdp
- `TextField` (MUI) : Champs de formulaire
- `Checkbox` (MUI) : Préférences
- `Button` (MUI) : Boutons d'action
- `Alert` (MUI) : Messages succès/erreur

### Flux Utilisateur

```
1. User accède à /profile
2. Chargement des données depuis AuthContext.user
3. Pré-remplissage du formulaire
4. Actions possibles :
   ├─ Modification des champs → Click "Sauvegarder"
   │   └─ PUT /api/Users/me → updateUser() → Success message
   ├─ Click "Changer de mot de passe"
   │   └─ PasswordChangeForm modal → POST change-password
   ├─ Upload avatar → POST /api/Users/me/avatar
   └─ Supprimer avatar → DELETE /api/Users/me/avatar
```

### Validation Avatar

```typescript
const validateFile = (file: File): string | null => {
  const validTypes = ['image/jpeg', 'image/png', 'image/gif', 'image/webp'];
  const maxSize = 5 * 1024 * 1024; // 5MB

  if (!validTypes.includes(file.type)) {
    return 'Format non supporté. Utilisez JPEG, PNG, GIF ou WebP.';
  }

  if (file.size > maxSize) {
    return 'Fichier trop volumineux. Maximum 5MB.';
  }

  return null;
};
```

---

## 7. Page Admin

### Route
- **Path** : `/admin`
- **Accès** : Admin only (isAdmin = true)
- **Fichier** : `src/pages/Admin.tsx`

### Description

Panneau d'administration réservé aux utilisateurs avec le rôle admin, permettant de gérer les utilisateurs, familles et consulter des statistiques.

### Fonctionnalités

1. **Tabs de Navigation**
   - 3 onglets : Dashboard, Utilisateurs, Familles
   - Navigation via `Tabs` Material-UI

2. **Tab Dashboard**
   - Statistiques API OpenGraph
   - Graphique linéaire (Recharts) : Requêtes par mois
   - Cards avec métriques :
     - Total requêtes
     - Taux de succès
     - Utilisateurs actifs

3. **Tab Utilisateurs**
   - Table avec tous les utilisateurs
   - Colonnes : Avatar, Nom, Email, Famille, Rôle, Actions
   - Actions :
     - Créer un utilisateur
     - Modifier un utilisateur
     - Supprimer un utilisateur
     - Assigner rôle admin

4. **Tab Familles**
   - Table avec toutes les familles
   - Colonnes : Nom, Nombre de membres, Actions
   - Actions :
     - Créer une famille
     - Modifier une famille
     - Supprimer une famille

### State Management

```typescript
const [currentTab, setCurrentTab] = useState(0);
const [stats, setStats] = useState<AdminStats | null>(null);
const [users, setUsers] = useState<User[]>([]);
const [families, setFamilies] = useState<Family[]>([]);
const [loading, setLoading] = useState(false);
const [userDialogOpen, setUserDialogOpen] = useState(false);
const [familyDialogOpen, setFamilyDialogOpen] = useState(false);
```

### API Calls

| Endpoint | Méthode | Description |
|----------|---------|-------------|
| `/api/Admin/stats` | GET | Récupère les statistiques |
| `/api/Admin/users` | GET | Liste tous les utilisateurs |
| `/api/Admin/users` | POST | Crée un utilisateur |
| `/api/Admin/users/:id` | PUT | Met à jour un utilisateur |
| `/api/Admin/users/:id` | DELETE | Supprime un utilisateur |
| `/api/Admin/families` | GET | Liste toutes les familles |
| `/api/Admin/families` | POST | Crée une famille |
| `/api/Admin/families/:id` | PUT | Met à jour une famille |
| `/api/Admin/families/:id` | DELETE | Supprime une famille |

### Composants Utilisés

- `AdminDashboard` (custom) : Tab 1 avec stats
- `AdminUsers` (custom) : Tab 2 gestion users
- `AdminFamilies` (custom) : Tab 3 gestion families
- `Tabs`, `Tab` (MUI) : Navigation tabs
- `Table`, `TableBody`, `TableCell`, `TableRow` (MUI) : Tables
- `Dialog` (MUI) : Modals création/édition
- `LineChart` (Recharts) : Graphique

### Flux Utilisateur

```
1. User (admin) accède à /admin
2. ProtectedRoute vérifie isAdmin → OK
3. Par défaut : Tab Dashboard
4. Fetch adminAPI.getStats() → Affichage graphique
5. Navigation entre tabs :
   ├─ Tab Utilisateurs → Fetch adminAPI.getUsers()
   │   ├─ Click "Créer" → Dialog → POST
   │   ├─ Click "Modifier" → Dialog → PUT
   │   └─ Click "Supprimer" → Confirmation → DELETE
   └─ Tab Familles → Fetch adminAPI.getFamilies()
       ├─ Click "Créer" → Dialog → POST
       └─ Click "Supprimer" → Confirmation → DELETE
```

### Protection Admin

```tsx
// Dans App.tsx
<Route path="/admin" element={
  <ProtectedRoute>
    {user?.isAdmin ? <Admin /> : <Navigate to="/" />}
  </ProtectedRoute>
} />
```

### Graphique Stats (Dashboard)

```tsx
<LineChart width={600} height={300} data={stats.monthlyRequests}>
  <CartesianGrid strokeDasharray="3 3" />
  <XAxis dataKey="month" />
  <YAxis />
  <Tooltip />
  <Legend />
  <Line type="monotone" dataKey="total" stroke="#2d5f3f" name="Requêtes" />
  <Line type="monotone" dataKey="success" stroke="#82ca9d" name="Succès" />
</LineChart>
```

---

## Protection des Routes

### ProtectedRoute Component

Toutes les pages sauf Login sont protégées par le composant `ProtectedRoute` :

```tsx
const ProtectedRoute: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const { isAuthenticated, loading } = useAuth();

  if (loading) {
    return (
      <Box display="flex" justifyContent="center" alignItems="center" minHeight="100vh">
        <CircularProgress />
      </Box>
    );
  }

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  return <>{children}</>;
};
```

**Logique** :
1. `loading = true` → Affiche spinner (vérification token en cours)
2. `loading = false` + `isAuthenticated = false` → Redirect vers `/login`
3. `loading = false` + `isAuthenticated = true` → Affiche la page

### Protection Admin

La page Admin a une double protection :

```tsx
<Route path="/admin" element={
  <ProtectedRoute>
    {user?.isAdmin ? <Admin /> : <Navigate to="/" />}
  </ProtectedRoute>
} />
```

1. `ProtectedRoute` : Vérifie authentification
2. Condition `user?.isAdmin` : Vérifie rôle admin

---

## Navigation entre Pages

### Liens de Navigation

| Depuis | Vers | Action |
|--------|------|--------|
| Login | Home | Après authentification réussie |
| Home | MyList | Click sur "Ma liste" ou sur soi-même |
| Home | UserList | Click sur un autre utilisateur |
| MyList | Home | Click "Retour" dans NavigationBar |
| UserList | Home | Click "Retour" |
| NavigationBar | Cart | Click icône panier |
| NavigationBar | Profile | Click icône profil |
| NavigationBar | Admin | Click "Admin" (si isAdmin) |
| Any | Login | Déconnexion ou token expiré |

### Programmatic Navigation

```tsx
import { useNavigate } from 'react-router-dom';

const MyComponent = () => {
  const navigate = useNavigate();

  const handleClick = () => {
    navigate('/my-list');
  };

  return <Button onClick={handleClick}>Ma liste</Button>;
};
```

---

## Récapitulatif des Fonctionnalités par Page

| Page | Lecture | Création | Modification | Suppression | Réservation |
|------|---------|----------|--------------|-------------|-------------|
| Login | - | - | - | - | - |
| Home | ✅ Familles/Users | - | - | - | - |
| MyList | ✅ Mes cadeaux | ✅ Cadeau | ✅ Cadeau | ✅ Cadeau | - |
| UserList | ✅ Cadeaux user | - | - | - | ✅ Réservation |
| Cart | ✅ Mes réservations | - | - | - | ❌ Annulation |
| Profile | ✅ Mon profil | - | ✅ Profil | ❌ Avatar | - |
| Admin | ✅ Stats/Users/Families | ✅ User/Family | ✅ User/Family | ✅ User/Family | - |

---

## Références

- [React Router Documentation](https://reactrouter.com/)
- [Material-UI Components](https://mui.com/material-ui/all-components/)
- [React Hooks Reference](https://react.dev/reference/react)
