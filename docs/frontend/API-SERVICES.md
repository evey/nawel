# API Services - Nawel App

## Vue d'Ensemble

Toutes les requêtes HTTP vers le backend sont centralisées dans un seul fichier : `src/services/api.ts` (154 lignes).

Ce fichier fournit :
- Un client Axios configuré avec interceptors
- 6 groupes d'API organisés par fonctionnalité
- ~40 endpoints typés avec TypeScript
- Gestion automatique des erreurs et du token JWT

---

## Configuration Axios

### Base Configuration

```typescript
import axios, { AxiosInstance } from 'axios';

// URL de l'API depuis variable d'environnement
const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000/api';

// Instance Axios configurée
const api: AxiosInstance = axios.create({
  baseURL: API_URL,
  headers: {
    'Content-Type': 'application/json'
  },
  timeout: 30000  // 30 secondes
});
```

### Variables d'Environnement

**.env (Development)** :
```bash
VITE_API_URL=http://localhost:5000/api
```

**.env.production (Production)** :
```bash
VITE_API_URL=https://api.nawel.com/api
```

---

## Interceptors

### Request Interceptor

Ajoute automatiquement le token JWT à chaque requête.

```typescript
api.interceptors.request.use(
  (config) => {
    // Récupérer le token depuis localStorage
    const token = localStorage.getItem('nawel_token');

    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }

    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);
```

**Effet** :
- Toutes les requêtes authentifiées incluent `Authorization: Bearer <token>`
- Pas besoin de passer le token manuellement à chaque appel

---

### Response Interceptor

Gère les erreurs 401 (Unauthorized) en déconnectant automatiquement l'utilisateur.

```typescript
api.interceptors.response.use(
  (response) => {
    // Retourne la réponse telle quelle si succès
    return response;
  },
  (error) => {
    // Si 401 Unauthorized → Token expiré ou invalide
    if (error.response?.status === 401) {
      // Nettoyage du localStorage
      localStorage.removeItem('nawel_token');
      localStorage.removeItem('nawel_user');
      localStorage.removeItem('nawel_managingChild');

      // Redirect vers login
      window.location.href = '/login';
    }

    return Promise.reject(error);
  }
);
```

**Effet** :
- Token expiré → Déconnexion automatique + redirect vers `/login`
- Évite les erreurs silencieuses ou les états incohérents

---

## Groupes d'API

L'API est organisée en 6 groupes logiques :

| Groupe | Endpoints | Description |
|--------|-----------|-------------|
| **authAPI** | 5 | Authentification, réinitialisation MDP |
| **usersAPI** | 6 | Gestion du profil utilisateur |
| **listsAPI** | 2 | Récupération des listes de cadeaux |
| **giftsAPI** | 12 | CRUD cadeaux + réservations |
| **productsAPI** | 1 | Extraction infos produit (OpenGraph) |
| **adminAPI** | 9 | Administration (stats, users, families) |

**Total** : 35 endpoints

---

## 1. authAPI (Authentification)

### Endpoints

#### POST /auth/login

Authentifie un utilisateur.

```typescript
authAPI.login(credentials: LoginCredentials): Promise<LoginResponse>
```

**Request** :
```typescript
interface LoginCredentials {
  login: string;
  password: string;
}
```

**Response** :
```typescript
interface LoginResponse {
  token: string;      // JWT token
  user: User;         // User object complet
}
```

**Usage** :
```typescript
try {
  const response = await authAPI.login({
    login: 'admin',
    password: 'admin123'
  });

  const { token, user } = response.data;
  // Stocker dans AuthContext
} catch (error) {
  if (error.response?.data?.code === 'LEGACY_PASSWORD') {
    // Mot de passe MD5 → Migration requise
  } else {
    // Identifiants incorrects
  }
}
```

---

#### POST /auth/validate-token

Valide un token JWT (utilisé au startup).

```typescript
authAPI.validateToken(): Promise<User>
```

**Response** : User object si token valide

**Usage** :
```typescript
const token = localStorage.getItem('nawel_token');
if (token) {
  try {
    const user = await authAPI.validateToken();
    // Token valide, restaurer la session
  } catch (error) {
    // Token invalide, déconnecter
  }
}
```

---

#### POST /auth/request-password-reset

Demande un email de réinitialisation de mot de passe.

```typescript
authAPI.requestPasswordReset(data: { email: string }): Promise<void>
```

**Usage** :
```typescript
await authAPI.requestPasswordReset({ email: 'user@example.com' });
// Email envoyé avec lien de réinitialisation
```

---

#### POST /auth/reset-password

Réinitialise le mot de passe avec un token.

```typescript
authAPI.resetPassword(data: { token: string; newPassword: string }): Promise<void>
```

**Usage** :
```typescript
await authAPI.resetPassword({
  token: tokenFromUrl,
  newPassword: 'newSecurePassword123'
});
```

---

#### POST /auth/request-migration-reset

Demande un reset pour migration MD5 → BCrypt.

```typescript
authAPI.requestMigrationReset(data: { login: string }): Promise<void>
```

**Usage** :
```typescript
await authAPI.requestMigrationReset({ login: 'admin' });
// Email envoyé avec instructions de migration
```

---

## 2. usersAPI (Profil Utilisateur)

### Endpoints

#### GET /users/me

Récupère le profil de l'utilisateur connecté.

```typescript
usersAPI.getMe(): Promise<User>
```

**Usage** :
```typescript
const user = await usersAPI.getMe();
```

---

#### GET /users/:id

Récupère un utilisateur par ID.

```typescript
usersAPI.getById(id: number): Promise<User>
```

**Usage** :
```typescript
const owner = await usersAPI.getById(5);
// Utilisé dans UserList pour afficher le propriétaire
```

---

#### PUT /users/me

Met à jour le profil de l'utilisateur connecté.

```typescript
usersAPI.updateMe(data: UpdateProfileData): Promise<User>
```

**Request** :
```typescript
interface UpdateProfileData {
  firstName?: string;
  lastName?: string;
  email?: string;
  pseudo?: string;
  notifyListEdit?: boolean;
  notifyGiftTaken?: boolean;
  displayPopup?: boolean;
}
```

**Usage** :
```typescript
await usersAPI.updateMe({
  firstName: 'John',
  lastName: 'Doe',
  notifyGiftTaken: true
});
```

---

#### POST /users/me/change-password

Change le mot de passe.

```typescript
usersAPI.changePassword(data: ChangePasswordData): Promise<void>
```

**Request** :
```typescript
interface ChangePasswordData {
  oldPassword: string;
  newPassword: string;
}
```

**Usage** :
```typescript
await usersAPI.changePassword({
  oldPassword: 'oldPass123',
  newPassword: 'newSecurePass456'
});
```

---

#### POST /users/me/avatar

Upload un avatar (multipart/form-data).

```typescript
usersAPI.uploadAvatar(file: File): Promise<{ avatarUrl: string }>
```

**Usage** :
```typescript
const formData = new FormData();
formData.append('avatar', file);

const response = await usersAPI.uploadAvatar(file);
// response.data.avatarUrl = "uploads/avatars/user_2_guid.jpg"
```

**Configuration Axios pour FormData** :
```typescript
const uploadAvatar = (file: File) => {
  const formData = new FormData();
  formData.append('avatar', file);

  return api.post('/users/me/avatar', formData, {
    headers: { 'Content-Type': 'multipart/form-data' }
  });
};
```

---

#### DELETE /users/me/avatar

Supprime l'avatar.

```typescript
usersAPI.deleteAvatar(): Promise<void>
```

**Usage** :
```typescript
await usersAPI.deleteAvatar();
// Avatar réinitialisé à la valeur par défaut
```

---

## 3. listsAPI (Listes de Cadeaux)

### Endpoints

#### GET /lists/all

Récupère toutes les familles avec leurs membres.

```typescript
listsAPI.getAll(): Promise<AllListsResponse>
```

**Response** :
```typescript
interface AllListsResponse {
  families: FamilyList[];
}

interface FamilyList {
  id: number;
  name: string;
  users: UserListInfo[];
}

interface UserListInfo {
  id: number;
  firstName: string;
  lastName: string;
  avatar: string | null;
  isAdmin: boolean;
  isChildren: boolean;
  familyId: number;
}
```

**Usage** :
```typescript
const response = await listsAPI.getAll();
const families = response.data.families;

// Affichage dans Home Page
families.forEach(family => {
  console.log(`Famille ${family.name} :`);
  family.users.forEach(user => {
    console.log(`- ${user.firstName} ${user.lastName}`);
  });
});
```

---

#### GET /lists/mine

Récupère la liste de l'utilisateur connecté.

```typescript
listsAPI.getMine(): Promise<GiftList>
```

**Response** :
```typescript
interface GiftList {
  id: number;
  name: string;
  userId: number;
  createdAt: string;
  updatedAt: string;
}
```

**Usage** :
```typescript
const myList = await listsAPI.getMine();
```

---

## 4. giftsAPI (Gestion des Cadeaux)

### Endpoints

#### GET /gifts/my-list

Récupère les cadeaux de l'utilisateur connecté pour une année.

```typescript
giftsAPI.getMyGifts(year?: number): Promise<Gift[]>
```

**Usage** :
```typescript
const gifts = await giftsAPI.getMyGifts(2025);
```

---

#### GET /gifts/manage-child/:childId

Récupère les cadeaux d'un enfant (mode managing child).

```typescript
giftsAPI.getChildGifts(childId: number, year?: number): Promise<Gift[]>
```

**Usage** :
```typescript
const childGifts = await giftsAPI.getChildGifts(5, 2025);
```

---

#### GET /gifts/user/:userId

Récupère les cadeaux d'un utilisateur (pour consultation).

```typescript
giftsAPI.getUserGifts(userId: number, year?: number): Promise<Gift[]>
```

**Usage** :
```typescript
const userGifts = await giftsAPI.getUserGifts(5, 2025);
// Utilisé dans UserList Page
```

---

#### GET /gifts/available-years OR /gifts/available-years/:userId

Récupère les années disponibles (avec cadeaux).

```typescript
giftsAPI.getAvailableYears(userId?: number): Promise<number[]>
```

**Usage** :
```typescript
// Mes années
const myYears = await giftsAPI.getAvailableYears();

// Années d'un autre utilisateur
const userYears = await giftsAPI.getAvailableYears(5);
```

---

#### GET /gifts/cart

Récupère le panier (cadeaux réservés).

```typescript
giftsAPI.getCart(year?: number): Promise<Gift[]>
```

**Usage** :
```typescript
const reservedGifts = await giftsAPI.getCart(2025);
// Inclut cadeaux classiques (takenBy = me) et participations groupées
```

---

#### POST /gifts

Crée un nouveau cadeau.

```typescript
giftsAPI.createGift(data: CreateGiftData): Promise<Gift>
```

**Request** :
```typescript
interface CreateGiftData {
  name: string;
  description?: string;
  link?: string;
  cost?: number;
  currency?: string;
  image?: string;
  isGroupGift?: boolean;
}
```

**Usage** :
```typescript
const newGift = await giftsAPI.createGift({
  name: 'Nintendo Switch',
  description: 'Console de jeu portable',
  cost: 299.99,
  currency: 'EUR',
  isGroupGift: false
});
```

---

#### PUT /gifts/:id

Met à jour un cadeau.

```typescript
giftsAPI.updateGift(id: number, data: UpdateGiftData): Promise<Gift>
```

**Request** :
```typescript
interface UpdateGiftData {
  name?: string;
  description?: string;
  link?: string;
  cost?: number;
  currency?: string;
  image?: string;
  isGroupGift?: boolean;
}
```

**Usage** :
```typescript
await giftsAPI.updateGift(123, {
  cost: 279.99,
  description: 'Console avec jeu inclus'
});
```

---

#### DELETE /gifts/:id

Supprime un cadeau.

```typescript
giftsAPI.deleteGift(id: number): Promise<void>
```

**Usage** :
```typescript
await giftsAPI.deleteGift(123);
```

---

#### POST /gifts/import

Importe des cadeaux depuis une année précédente.

```typescript
giftsAPI.importGifts(fromYear: number, toYear: number): Promise<{ count: number }>
```

**Usage** :
```typescript
const result = await giftsAPI.importGifts(2024, 2025);
console.log(`${result.data.count} cadeaux importés`);
```

---

#### POST /gifts/:id/reserve

Réserve un cadeau (ou participe à un cadeau groupé).

```typescript
giftsAPI.reserveGift(id: number, data?: ReserveGiftData): Promise<void>
```

**Request** :
```typescript
interface ReserveGiftData {
  comment?: string;
}
```

**Usage** :
```typescript
await giftsAPI.reserveGift(123, { comment: 'Avec plaisir !' });
```

---

#### POST /gifts/:id/unreserve

Annule une réservation.

```typescript
giftsAPI.unreserveGift(id: number): Promise<void>
```

**Usage** :
```typescript
await giftsAPI.unreserveGift(123);
```

---

## 5. productsAPI (Extraction Produit)

### Endpoint

#### POST /products/extract-info

Extrait les métadonnées d'un produit depuis une URL (OpenGraph).

```typescript
productsAPI.extractInfo(url: string): Promise<ProductInfo>
```

**Request** :
```typescript
interface ExtractInfoRequest {
  url: string;
}
```

**Response** :
```typescript
interface ProductInfo {
  name: string | null;
  description: string | null;
  price: number | null;
  currency: string | null;
  imageUrl: string | null;
}
```

**Usage** :
```typescript
const productInfo = await productsAPI.extractInfo('https://www.amazon.fr/...');

// Auto-remplissage du formulaire
setFormData({
  name: productInfo.data.name || '',
  description: productInfo.data.description || '',
  cost: productInfo.data.price || null,
  currency: productInfo.data.currency || 'EUR',
  image: productInfo.data.imageUrl || ''
});
```

---

## 6. adminAPI (Administration)

### Endpoints

#### GET /admin/stats

Récupère les statistiques admin.

```typescript
adminAPI.getStats(): Promise<AdminStats>
```

**Response** :
```typescript
interface AdminStats {
  totalRequests: number;
  successRate: number;
  activeUsers: number;
  monthlyRequests: Array<{
    month: string;
    total: number;
    success: number;
  }>;
}
```

**Usage** :
```typescript
const stats = await adminAPI.getStats();
// Affichage dans AdminDashboard avec graphique
```

---

#### GET /admin/users

Liste tous les utilisateurs.

```typescript
adminAPI.getUsers(): Promise<User[]>
```

---

#### POST /admin/users

Crée un utilisateur.

```typescript
adminAPI.createUser(data: CreateUserData): Promise<User>
```

**Request** :
```typescript
interface CreateUserData {
  login: string;
  password: string;
  firstName?: string;
  lastName?: string;
  email?: string;
  familyId: number;
  isAdmin?: boolean;
  isChildren?: boolean;
}
```

---

#### PUT /admin/users/:id

Met à jour un utilisateur.

```typescript
adminAPI.updateUser(id: number, data: UpdateUserAdminDto): Promise<User>
```

---

#### DELETE /admin/users/:id

Supprime un utilisateur.

```typescript
adminAPI.deleteUser(id: number): Promise<void>
```

---

#### GET /admin/families

Liste toutes les familles.

```typescript
adminAPI.getFamilies(): Promise<Family[]>
```

---

#### POST /admin/families

Crée une famille.

```typescript
adminAPI.createFamily(data: CreateFamilyData): Promise<Family>
```

**Request** :
```typescript
interface CreateFamilyData {
  name: string;
}
```

---

#### PUT /admin/families/:id

Met à jour une famille.

```typescript
adminAPI.updateFamily(id: number, data: UpdateFamilyData): Promise<Family>
```

---

#### DELETE /admin/families/:id

Supprime une famille.

```typescript
adminAPI.deleteFamily(id: number): Promise<void>
```

---

## Gestion des Erreurs

### Try-Catch Pattern

```typescript
const handleAction = async () => {
  setLoading(true);
  setError(null);

  try {
    const result = await giftsAPI.createGift(formData);
    setSuccess(true);
    return result.data;
  } catch (error: any) {
    const errorMessage = error.response?.data?.message || 'Une erreur est survenue';
    setError(errorMessage);
    console.error('API Error:', error);
  } finally {
    setLoading(false);
  }
};
```

### Erreurs Spécifiques

#### 400 Bad Request

```typescript
catch (error: any) {
  if (error.response?.status === 400) {
    // Validation error
    const errors = error.response.data.errors;
    setValidationErrors(errors);
  }
}
```

#### 401 Unauthorized

Géré automatiquement par l'interceptor → Redirect `/login`

#### 403 Forbidden

```typescript
catch (error: any) {
  if (error.response?.status === 403) {
    alert('Vous n\'avez pas les permissions pour cette action');
  }
}
```

#### 404 Not Found

```typescript
catch (error: any) {
  if (error.response?.status === 404) {
    alert('Ressource introuvable');
  }
}
```

#### 429 Too Many Requests

```typescript
catch (error: any) {
  if (error.response?.status === 429) {
    alert('Trop de requêtes. Veuillez réessayer plus tard.');
  }
}
```

#### 500 Internal Server Error

```typescript
catch (error: any) {
  if (error.response?.status === 500) {
    alert('Erreur serveur. Veuillez contacter l\'administrateur.');
  }
}
```

---

## Types TypeScript

Tous les types sont définis dans `src/types/` :

### Importation

```typescript
import type {
  User,
  LoginCredentials,
  LoginResponse,
  Gift,
  CreateGiftData,
  ProductInfo
} from '../types';
```

### Typage des Réponses Axios

```typescript
import { AxiosResponse } from 'axios';

const login = (credentials: LoginCredentials): Promise<AxiosResponse<LoginResponse>> => {
  return api.post<LoginResponse>('/auth/login', credentials);
};
```

---

## Utilisation dans les Composants

### Pattern Standard

```typescript
import { giftsAPI } from '../services/api';

const MyComponent: React.FC = () => {
  const [gifts, setGifts] = useState<Gift[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    fetchGifts();
  }, []);

  const fetchGifts = async () => {
    setLoading(true);
    setError(null);

    try {
      const response = await giftsAPI.getMyGifts(2025);
      setGifts(response.data);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Erreur de chargement');
    } finally {
      setLoading(false);
    }
  };

  if (loading) return <CircularProgress />;
  if (error) return <Alert severity="error">{error}</Alert>;

  return (
    <div>
      {gifts.map(gift => (
        <GiftCard key={gift.id} gift={gift} />
      ))}
    </div>
  );
};
```

---

## Testing des API Calls

### Mocking avec Vitest

```typescript
import { vi } from 'vitest';
import { giftsAPI } from '../services/api';

vi.mock('../services/api', () => ({
  giftsAPI: {
    getMyGifts: vi.fn()
  }
}));

describe('MyList Component', () => {
  it('fetches and displays gifts', async () => {
    const mockGifts: Gift[] = [
      { id: 1, name: 'Gift 1', /* ... */ },
      { id: 2, name: 'Gift 2', /* ... */ }
    ];

    vi.mocked(giftsAPI.getMyGifts).mockResolvedValue({
      data: mockGifts
    } as any);

    render(<MyList />);

    await waitFor(() => {
      expect(screen.getByText('Gift 1')).toBeInTheDocument();
      expect(screen.getByText('Gift 2')).toBeInTheDocument();
    });
  });
});
```

---

## Performance & Optimisation

### Éviter les Re-Fetches Inutiles

```typescript
const [gifts, setGifts] = useState<Gift[]>([]);
const [lastFetchYear, setLastFetchYear] = useState<number | null>(null);

useEffect(() => {
  if (selectedYear === lastFetchYear) return;  // Skip si même année

  fetchGifts();
  setLastFetchYear(selectedYear);
}, [selectedYear]);
```

### Debouncing des Requêtes

```typescript
import { debounce } from 'lodash';

const debouncedSearch = useCallback(
  debounce(async (query: string) => {
    const results = await api.get('/search', { params: { q: query } });
    setResults(results.data);
  }, 300),
  []
);
```

### Cancel Tokens (si nécessaire)

```typescript
useEffect(() => {
  const source = axios.CancelToken.source();

  const fetchData = async () => {
    try {
      const response = await api.get('/data', {
        cancelToken: source.token
      });
      setData(response.data);
    } catch (error) {
      if (!axios.isCancel(error)) {
        console.error(error);
      }
    }
  };

  fetchData();

  return () => {
    source.cancel('Component unmounted');
  };
}, []);
```

---

## Récapitulatif

| Groupe API | Endpoints | Authentification Requise | Rôle Admin Requis |
|------------|-----------|--------------------------|-------------------|
| authAPI | 5 | Non (sauf validateToken) | Non |
| usersAPI | 6 | Oui | Non |
| listsAPI | 2 | Oui | Non |
| giftsAPI | 12 | Oui | Non |
| productsAPI | 1 | Oui | Non |
| adminAPI | 9 | Oui | Oui |

---

## Références

- [Axios Documentation](https://axios-http.com/docs/intro)
- [Axios Interceptors](https://axios-http.com/docs/interceptors)
- [TypeScript with Axios](https://axios-http.com/docs/typescript)
- [Backend API Documentation](http://localhost:5000/swagger) - Swagger UI
