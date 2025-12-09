# State Management - Nawel App

## Vue d'Ensemble

L'application Nawel utilise une stratégie de state management hybride combinant :

1. **Context API (React)** : State global via `AuthContext`
2. **Local State (useState)** : State local dans pages et composants
3. **localStorage** : Persistence des données d'authentification

Cette approche simple et efficace convient parfaitement à la taille de l'application sans nécessiter de bibliothèque externe comme Redux.

---

## Architecture du State

```
┌─────────────────────────────────────────────────┐
│              AuthContext (Global)                │
│  - user: User | null                             │
│  - token: string | null                          │
│  - loading: boolean                              │
│  - managingChild: ManagingChild | null           │
│  ↕                                               │
│  localStorage                                    │
│  - nawel_token                                   │
│  - nawel_user                                    │
│  - nawel_managingChild                           │
└─────────────────┬───────────────────────────────┘
                  │
                  ├─────────> Home Page
                  │            - families: Family[]
                  │            - loading: boolean
                  │
                  ├─────────> MyList Page
                  │            - gifts: Gift[]
                  │            - selectedYear: number
                  │            - dialogOpen: boolean
                  │
                  ├─────────> UserList Page
                  │            - owner: User | null
                  │            - gifts: Gift[]
                  │            - selectedYear: number
                  │
                  ├─────────> Cart Page
                  │            - reservedGifts: Gift[]
                  │            - totals: { [currency]: number }
                  │
                  ├─────────> Profile Page
                  │            - profile: UpdateProfileData
                  │            - success: boolean
                  │
                  └─────────> Admin Page
                               - stats: AdminStats | null
                               - users: User[]
                               - families: Family[]
```

---

## AuthContext (Global State)

### Fichier

`src/contexts/AuthContext.tsx`

### Description

Le contexte d'authentification gère l'état global de l'utilisateur connecté, son token JWT, et le mode "Managing Child" (parent gérant la liste d'un enfant).

### Interface

```typescript
interface AuthContextType {
  // State
  user: User | null;                          // Utilisateur connecté
  token: string | null;                       // JWT token
  loading: boolean;                           // Initial auth check
  managingChild: ManagingChild | null;        // Enfant actuellement géré
  isAuthenticated: boolean;                   // Dérivé de token

  // Actions
  login: (credentials: LoginCredentials) => Promise<void>;
  logout: () => void;
  updateUser: (userData: Partial<User>) => void;
  startManagingChild: (child: ManagingChild) => void;
  stopManagingChild: () => void;
}

interface ManagingChild {
  userId: number;
  userName: string;
  avatarUrl: string;
}
```

### Provider

```typescript
export const AuthProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [user, setUser] = useState<User | null>(null);
  const [token, setToken] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);
  const [managingChild, setManagingChild] = useState<ManagingChild | null>(null);

  // Initialisation au mount
  useEffect(() => {
    const initAuth = () => {
      const storedToken = localStorage.getItem('nawel_token');
      const storedUser = localStorage.getItem('nawel_user');
      const storedManagingChild = localStorage.getItem('nawel_managingChild');

      if (storedToken && storedUser) {
        setToken(storedToken);
        setUser(JSON.parse(storedUser));
      }

      if (storedManagingChild) {
        setManagingChild(JSON.parse(storedManagingChild));
      }

      setLoading(false);
    };

    initAuth();
  }, []);

  // ... méthodes (voir ci-dessous)

  const value = {
    user,
    token,
    loading,
    managingChild,
    isAuthenticated: !!token,
    login,
    logout,
    updateUser,
    startManagingChild,
    stopManagingChild
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
};
```

### Méthodes

#### 1. login()

Authentifie un utilisateur et stocke ses informations.

```typescript
const login = async (credentials: LoginCredentials) => {
  try {
    const response = await authAPI.login(credentials);
    const { token, user } = response.data;

    // Stockage mémoire
    setToken(token);
    setUser(user);

    // Persistence
    localStorage.setItem('nawel_token', token);
    localStorage.setItem('nawel_user', JSON.stringify(user));
  } catch (error) {
    // Gestion des erreurs (MD5 legacy, credentials invalides)
    throw error;
  }
};
```

**Usage** :
```typescript
const { login } = useAuth();

const handleLogin = async () => {
  try {
    await login({ login: 'admin', password: 'admin' });
    navigate('/');
  } catch (error) {
    if (error.response?.data?.code === 'LEGACY_PASSWORD') {
      setShowLegacyPasswordModal(true);
    } else {
      setError('Identifiants incorrects');
    }
  }
};
```

---

#### 2. logout()

Déconnecte l'utilisateur et nettoie tout le state.

```typescript
const logout = () => {
  // Nettoyage mémoire
  setUser(null);
  setToken(null);
  setManagingChild(null);

  // Nettoyage persistence
  localStorage.removeItem('nawel_token');
  localStorage.removeItem('nawel_user');
  localStorage.removeItem('nawel_managingChild');
};
```

**Usage** :
```typescript
const { logout } = useAuth();

const handleLogout = () => {
  logout();
  navigate('/login');
};
```

---

#### 3. updateUser()

Met à jour les informations de l'utilisateur (après modification profil, upload avatar).

```typescript
const updateUser = (userData: Partial<User>) => {
  if (!user) return;

  const updatedUser = { ...user, ...userData };

  // Update mémoire
  setUser(updatedUser);

  // Update persistence
  localStorage.setItem('nawel_user', JSON.stringify(updatedUser));
};
```

**Usage** :
```typescript
const { updateUser } = useAuth();

const handleSaveProfile = async (profileData: UpdateProfileData) => {
  await usersAPI.updateMe(profileData);

  // Mettre à jour le state global
  updateUser({
    firstName: profileData.firstName,
    lastName: profileData.lastName,
    email: profileData.email,
    // ...
  });
};
```

---

#### 4. startManagingChild()

Active le mode "Managing Child" (parent gérant la liste d'un enfant).

```typescript
const startManagingChild = (child: ManagingChild) => {
  setManagingChild(child);
  localStorage.setItem('nawel_managingChild', JSON.stringify(child));
};
```

**Usage** :
```typescript
const { startManagingChild } = useAuth();

const handleManageChild = (childUser: UserListInfo) => {
  startManagingChild({
    userId: childUser.id,
    userName: `${childUser.firstName} ${childUser.lastName}`,
    avatarUrl: childUser.avatar || ''
  });

  navigate('/my-list');
};
```

**Effet** :
- `ManagingChildBanner` s'affiche
- `MyList` charge les cadeaux de l'enfant au lieu des siens

---

#### 5. stopManagingChild()

Désactive le mode "Managing Child".

```typescript
const stopManagingChild = () => {
  setManagingChild(null);
  localStorage.removeItem('nawel_managingChild');
};
```

**Usage** :
```typescript
const { stopManagingChild } = useAuth();

const handleStopManaging = () => {
  stopManagingChild();
  navigate('/my-list');  // Retour à sa propre liste
};
```

---

### Hook useAuth()

Le custom hook `useAuth()` permet d'accéder au contexte facilement :

```typescript
export const useAuth = (): AuthContextType => {
  const context = useContext(AuthContext);

  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider');
  }

  return context;
};
```

**Usage dans un composant** :
```typescript
import { useAuth } from '../contexts/AuthContext';

const MyComponent = () => {
  const { user, isAuthenticated, login, logout } = useAuth();

  if (!isAuthenticated) {
    return <Navigate to="/login" />;
  }

  return (
    <div>
      <h1>Bonjour {user?.firstName}</h1>
      <Button onClick={logout}>Déconnexion</Button>
    </div>
  );
};
```

---

### Persistence (localStorage)

Le `AuthContext` utilise `localStorage` pour la persistence des données d'authentification :

| Clé | Contenu | Type |
|-----|---------|------|
| `nawel_token` | JWT token | string |
| `nawel_user` | User object complet | JSON string |
| `nawel_managingChild` | Child info (si actif) | JSON string |

**Lifecycle** :

1. **Mount de l'application** :
   ```typescript
   useEffect(() => {
     const storedToken = localStorage.getItem('nawel_token');
     const storedUser = localStorage.getItem('nawel_user');

     if (storedToken && storedUser) {
       setToken(storedToken);
       setUser(JSON.parse(storedUser));
     }

     setLoading(false);
   }, []);
   ```

2. **Login** : Stockage token + user

3. **Logout** : Suppression de toutes les clés

4. **Update Profile** : Mise à jour de `nawel_user`

5. **Token expiré (401)** : Suppression automatique (interceptor Axios)

---

## Local State (useState)

Chaque page et composant gère son propre état local pour les données spécifiques.

### Exemple : MyList Page

```typescript
const MyListPage: React.FC = () => {
  // State local
  const [gifts, setGifts] = useState<Gift[]>([]);
  const [selectedYear, setSelectedYear] = useState<number>(new Date().getFullYear());
  const [availableYears, setAvailableYears] = useState<number[]>([]);
  const [loading, setLoading] = useState(false);
  const [giftDialogOpen, setGiftDialogOpen] = useState(false);
  const [editingGift, setEditingGift] = useState<Gift | undefined>();

  // State global
  const { user, managingChild } = useAuth();

  // Fetching
  useEffect(() => {
    fetchGifts();
  }, [selectedYear]);

  const fetchGifts = async () => {
    setLoading(true);
    try {
      const data = await giftsAPI.getMyGifts(selectedYear);
      setGifts(data);
    } catch (error) {
      console.error('Error:', error);
    } finally {
      setLoading(false);
    }
  };

  // ...
};
```

### Pattern Commun

```typescript
// 1. State pour les données
const [items, setItems] = useState<Item[]>([]);

// 2. State pour le loading
const [loading, setLoading] = useState(false);

// 3. State pour les erreurs (optionnel)
const [error, setError] = useState<string | null>(null);

// 4. State pour les dialogues/modals
const [dialogOpen, setDialogOpen] = useState(false);

// 5. State pour les formulaires
const [formData, setFormData] = useState<FormData>({
  field1: '',
  field2: ''
});
```

---

## Derived State

L'état dérivé (calculé à partir d'autres états) utilise `useMemo` ou simplement des variables :

### Exemple : isAuthenticated

```typescript
// Dans AuthContext
const isAuthenticated = !!token;  // Dérivé de token
```

### Exemple : Totaux du Panier

```typescript
// Dans Cart Page
const [reservedGifts, setReservedGifts] = useState<Gift[]>([]);

// Dérivé : Calcul des totaux par devise
const totals = useMemo(() => {
  const totalsMap: { [currency: string]: number } = {};

  reservedGifts.forEach(gift => {
    if (gift.cost) {
      const currency = gift.currency || 'EUR';
      totalsMap[currency] = (totalsMap[currency] || 0) + gift.cost;
    }
  });

  return totalsMap;
}, [reservedGifts]);
```

---

## Effects (useEffect)

Les effets gèrent les side-effects comme les appels API, subscriptions, etc.

### Pattern : Data Fetching

```typescript
useEffect(() => {
  const fetchData = async () => {
    setLoading(true);
    try {
      const data = await api.getData();
      setData(data);
    } catch (error) {
      setError(error.message);
    } finally {
      setLoading(false);
    }
  };

  fetchData();
}, [dependency]);  // Re-fetch si dependency change
```

### Pattern : Cleanup

```typescript
useEffect(() => {
  const timer = setTimeout(() => {
    // Action
  }, 2000);

  // Cleanup
  return () => clearTimeout(timer);
}, []);
```

### Pattern : Sync localStorage

```typescript
useEffect(() => {
  localStorage.setItem('key', JSON.stringify(value));
}, [value]);
```

---

## State Updates

### Pattern : Object Update

```typescript
const [formData, setFormData] = useState({ name: '', email: '' });

// Mise à jour partielle
setFormData(prev => ({
  ...prev,
  name: 'New Name'
}));
```

### Pattern : Array Update

```typescript
const [items, setItems] = useState<Item[]>([]);

// Ajouter
setItems(prev => [...prev, newItem]);

// Mettre à jour
setItems(prev => prev.map(item =>
  item.id === id ? { ...item, updated: true } : item
));

// Supprimer
setItems(prev => prev.filter(item => item.id !== id));
```

---

## Callbacks (useCallback)

Les callbacks passés aux composants enfants doivent être memoizés pour éviter re-renders :

```typescript
const handleSave = useCallback((data: GiftData) => {
  // Action
  saveGift(data);
}, [/* dependencies */]);

<ChildComponent onSave={handleSave} />
```

---

## Tests

### AuthContext Tests

**Fichier** : `src/contexts/AuthContext.test.tsx`

```typescript
describe('AuthContext', () => {
  it('provides authentication state', () => {
    const { result } = renderHook(() => useAuth(), {
      wrapper: AuthProvider
    });

    expect(result.current.isAuthenticated).toBe(false);
    expect(result.current.user).toBeNull();
  });

  it('login sets user and token', async () => {
    const { result } = renderHook(() => useAuth(), {
      wrapper: AuthProvider
    });

    await act(async () => {
      await result.current.login({ login: 'admin', password: 'admin' });
    });

    expect(result.current.isAuthenticated).toBe(true);
    expect(result.current.user).not.toBeNull();
  });

  it('logout clears user and token', () => {
    const { result } = renderHook(() => useAuth(), {
      wrapper: AuthProvider
    });

    act(() => {
      result.current.logout();
    });

    expect(result.current.isAuthenticated).toBe(false);
    expect(result.current.user).toBeNull();
  });

  it('manages child mode correctly', () => {
    const { result } = renderHook(() => useAuth(), {
      wrapper: AuthProvider
    });

    act(() => {
      result.current.startManagingChild({
        userId: 2,
        userName: 'Child',
        avatarUrl: ''
      });
    });

    expect(result.current.managingChild).not.toBeNull();
    expect(result.current.managingChild?.userId).toBe(2);

    act(() => {
      result.current.stopManagingChild();
    });

    expect(result.current.managingChild).toBeNull();
  });
});
```

---

## Bonnes Pratiques

### 1. Éviter les Props Drilling

Utiliser Context plutôt que passer props sur plusieurs niveaux :

```typescript
// ❌ Bad : Props drilling
<Parent>
  <Child user={user}>
    <GrandChild user={user}>
      <GreatGrandChild user={user} />
    </GrandChild>
  </Child>
</Parent>

// ✅ Good : Context
<AuthProvider>
  <Parent>
    <Child>
      <GrandChild>
        <GreatGrandChild />  {/* Utilise useAuth() */}
      </GrandChild>
    </Child>
  </Parent>
</AuthProvider>
```

### 2. State Local vs Global

**État local** : Données spécifiques à un composant/page
- Formulaires
- Dialogues ouvertes/fermées
- Loading states
- Données de liste (gifts, users, etc.)

**État global** : Données partagées entre plusieurs pages
- User connecté
- Token JWT
- Préférences globales
- Managing child mode

### 3. Éviter les Re-Renders Inutiles

```typescript
// ✅ Memoize callbacks
const handleClick = useCallback(() => {
  // Action
}, [dependencies]);

// ✅ Memoize derived state
const filteredItems = useMemo(() => {
  return items.filter(item => item.visible);
}, [items]);

// ✅ Memoize components
const MemoizedChild = React.memo(ChildComponent);
```

### 4. Initialisation Asynchrone

```typescript
useEffect(() => {
  let mounted = true;

  const fetchData = async () => {
    const data = await api.getData();
    if (mounted) {
      setData(data);
    }
  };

  fetchData();

  return () => {
    mounted = false;
  };
}, []);
```

### 5. Gestion des Erreurs

```typescript
const [error, setError] = useState<string | null>(null);

const handleAction = async () => {
  setError(null);
  try {
    await api.call();
  } catch (err: any) {
    setError(err.response?.data?.message || 'Une erreur est survenue');
  }
};

// Affichage
{error && <Alert severity="error">{error}</Alert>}
```

---

## Alternatives Non Utilisées

### Redux

**Pourquoi pas utilisé** :
- Application de petite/moyenne taille
- State global limité (auth uniquement)
- Context API suffisant
- Réduirait la simplicité du code

**Quand l'envisager** :
- Application > 50 pages
- State global complexe et partagé
- Nombreuses actions asynchrones interdépendantes

### Zustand / Jotai / Recoil

**Pourquoi pas utilisé** :
- Même raison que Redux (overkill)
- Context API fait le job

### React Query / SWR

**Pourquoi pas utilisé** :
- Pas de cache sophistiqué requis
- Pas de synchronisation temps réel
- Fetching manuel avec Axios suffisant

**Quand l'envisager** :
- Besoin de cache automatique
- Invalidation de cache complexe
- Optimistic updates
- Synchronisation multi-onglets

---

## Diagramme de Flux

### Flux d'Authentification

```
┌─────────────┐
│ Login Page  │
└──────┬──────┘
       │ User entre credentials
       ↓
┌──────────────────┐
│ AuthContext      │
│ login()          │
└──────┬───────────┘
       │ authAPI.login()
       ↓
┌──────────────────┐
│ Backend API      │
└──────┬───────────┘
       │ Returns {token, user}
       ↓
┌──────────────────┐
│ AuthContext      │
│ - setToken()     │
│ - setUser()      │
│ - localStorage   │
└──────┬───────────┘
       │
       ↓
┌──────────────────┐
│ Navigate to /    │
└──────────────────┘
```

### Flux de Gestion Enfant

```
┌─────────────┐
│ Home Page   │
└──────┬──────┘
       │ Parent click "Gérer" sur enfant
       ↓
┌──────────────────────────┐
│ AuthContext              │
│ startManagingChild()     │
│ - setManagingChild()     │
│ - localStorage           │
└──────┬───────────────────┘
       │
       ↓
┌──────────────────────────┐
│ ManagingChildBanner      │
│ (s'affiche)              │
└──────────────────────────┘
       │
       ↓
┌──────────────────────────┐
│ MyList Page              │
│ - Détecte managingChild  │
│ - Fetch gifts enfant     │
│ - Actions sur liste      │
└──────┬───────────────────┘
       │ Parent click "Revenir"
       ↓
┌──────────────────────────┐
│ AuthContext              │
│ stopManagingChild()      │
│ - setManagingChild(null) │
│ - localStorage.remove    │
└──────┬───────────────────┘
       │
       ↓
┌──────────────────────────┐
│ MyList Page              │
│ - Fetch own gifts        │
└──────────────────────────┘
```

---

## Récapitulatif

| Type de State | Scope | Persistence | Utilisation |
|---------------|-------|-------------|-------------|
| **AuthContext** | Global | localStorage | User, token, managingChild |
| **Local State** | Page/Component | Non | Données spécifiques, UI state |
| **Derived State** | Local | Non | Calculé à partir d'autres states |
| **Form State** | Component | Non | Valeurs de formulaires |

---

## Références

- [React Context Documentation](https://react.dev/learn/passing-data-deeply-with-context)
- [useState Hook](https://react.dev/reference/react/useState)
- [useEffect Hook](https://react.dev/reference/react/useEffect)
- [useCallback Hook](https://react.dev/reference/react/useCallback)
- [useMemo Hook](https://react.dev/reference/react/useMemo)
