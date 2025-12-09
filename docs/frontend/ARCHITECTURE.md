# Architecture Frontend - Nawel App

## Vue d'Ensemble

L'application frontend Nawel est construite avec **React 19.2** et **TypeScript 5.9**, suivant une architecture modulaire avec séparation claire entre pages, composants réutilisables, logique métier et styles.

### Stack Technique

| Composant | Technologie | Version | Utilisation |
|-----------|-------------|---------|-------------|
| Framework | React | 19.2.0 | UI library avec hooks |
| Langage | TypeScript | 5.9.3 | Type safety |
| Build Tool | Vite | 7.2.3 | Dev server + bundler |
| UI Library | Material-UI (MUI) | 7.3.4 | Composants préfabriqués |
| Routing | React Router | 7.10.0 | Navigation SPA |
| HTTP Client | Axios | 1.13.0 | Requêtes API |
| CSS Preprocessor | LESS | 4.4.2 | Styles avec modules |
| Charts | Recharts | 2.15.1 | Graphiques admin |
| Testing | Vitest | 4.0.15 | Tests unitaires |
| Testing Library | @testing-library/react | 16.2.0 | Tests composants |

## Architecture Globale

```
┌─────────────────────────────────────────────────┐
│              Browser / User                      │
└─────────────────┬───────────────────────────────┘
                  │
┌─────────────────▼───────────────────────────────┐
│          React Application (SPA)                 │
│                                                  │
│  ┌──────────────────────────────────────────┐  │
│  │         Routing (React Router)            │  │
│  │  - ProtectedRoute wrapper                 │  │
│  │  - Public: /login                         │  │
│  │  - Protected: /, /my-list, /cart, etc.   │  │
│  └─────────────────┬────────────────────────┘  │
│                    │                            │
│  ┌─────────────────▼────────────────────────┐  │
│  │           Pages Layer                     │  │
│  │  - Home, MyList, UserList, Cart           │  │
│  │  - Profile, Admin, Login                  │  │
│  └─────────────────┬────────────────────────┘  │
│                    │                            │
│  ┌─────────────────▼────────────────────────┐  │
│  │        Components Layer                   │  │
│  │  - Layout: NavigationBar, Avatar          │  │
│  │  - Gifts: GiftFormDialog, GiftListItem    │  │
│  │  - Profile: ProfileForm, AvatarUpload     │  │
│  │  - Admin: AdminDashboard, AdminUsers      │  │
│  └─────────────────┬────────────────────────┘  │
│                    │                            │
│  ┌─────────────────▼────────────────────────┐  │
│  │       State Management                    │  │
│  │  - AuthContext (global)                   │  │
│  │  - Local useState (page/component)        │  │
│  │  - localStorage (persistence)             │  │
│  └─────────────────┬────────────────────────┘  │
│                    │                            │
│  ┌─────────────────▼────────────────────────┐  │
│  │         Services Layer                    │  │
│  │  - api.ts: Axios client + interceptors    │  │
│  │  - API groups: auth, users, gifts, etc.   │  │
│  └─────────────────┬────────────────────────┘  │
│                    │                            │
│  ┌─────────────────▼────────────────────────┐  │
│  │        Utilities & Helpers                │  │
│  │  - giftHelpers.ts: Formatting, validation │  │
│  │  - logger.ts: Development logging         │  │
│  └──────────────────────────────────────────┘  │
│                                                  │
└──────────────────┬───────────────────────────────┘
                   │ HTTP REST
┌──────────────────▼───────────────────────────────┐
│         ASP.NET Core API (Backend)               │
│              Port 5000                           │
└──────────────────────────────────────────────────┘
```

## Structure des Répertoires

```
frontend/nawel-app/
├── public/                  # Assets statiques
│   └── avatar.png          # Avatar par défaut
├── src/
│   ├── components/         # Composants réutilisables (18 composants)
│   │   ├── gifts/         # Composants liés aux cadeaux (3)
│   │   ├── profile/       # Composants profil utilisateur (3)
│   │   ├── admin/         # Composants administration (3)
│   │   ├── userlist/      # Composants liste utilisateur (2)
│   │   └── *.tsx          # Composants layout (7)
│   ├── contexts/          # Contextes React
│   │   └── AuthContext.tsx # Authentification globale
│   ├── css/              # Styles LESS modules (23 fichiers)
│   │   ├── pages/        # Styles pages
│   │   ├── components/   # Styles composants
│   │   └── common.module.less
│   ├── hooks/            # Custom hooks
│   │   └── useGifts.ts  # Hook gestion cadeaux (non utilisé)
│   ├── pages/            # Pages principales (7)
│   │   ├── Home.tsx
│   │   ├── MyList.tsx
│   │   ├── UserList.tsx
│   │   ├── Cart.tsx
│   │   ├── Profile.tsx
│   │   ├── Admin.tsx
│   │   └── Login.tsx
│   ├── services/         # Services API
│   │   └── api.ts       # Client Axios + tous les endpoints
│   ├── test/            # Configuration tests
│   │   ├── setup.ts
│   │   └── vitest-setup.d.ts
│   ├── types/           # Définitions TypeScript (6 fichiers)
│   │   ├── user.ts
│   │   ├── gift.ts
│   │   ├── list.ts
│   │   ├── family.ts
│   │   ├── api.ts
│   │   └── index.ts
│   ├── utils/           # Utilitaires
│   │   ├── giftHelpers.ts
│   │   └── logger.ts
│   ├── App.tsx          # Composant racine + routing
│   ├── main.tsx         # Point d'entrée + theme MUI
│   └── vite-env.d.ts    # Types Vite
├── .env                 # Variables d'environnement
├── index.html           # HTML entry point
├── package.json         # Dépendances npm
├── tsconfig.json        # Configuration TypeScript
├── vite.config.ts       # Configuration Vite
└── vitest.config.ts     # Configuration tests
```

## Couche Pages

Les pages représentent les écrans principaux de l'application. Chaque page correspond à une route.

### Responsabilités

- Orchestration des composants
- Gestion de l'état local de la page
- Appels API via `services/api.ts`
- Gestion des effets (useEffect)
- Routing et navigation

### Structure Type d'une Page

```tsx
import React, { useState, useEffect } from 'react';
import { useAuth } from '../contexts/AuthContext';
import { giftsAPI } from '../services/api';
import { Gift } from '../types';
import GiftListItem from '../components/gifts/GiftListItem';
import styles from '../css/pages/MyList.module.less';

const MyListPage: React.FC = () => {
  // State management
  const { user } = useAuth();
  const [gifts, setGifts] = useState<Gift[]>([]);
  const [loading, setLoading] = useState(true);

  // Data fetching
  useEffect(() => {
    fetchGifts();
  }, []);

  const fetchGifts = async () => {
    try {
      setLoading(true);
      const data = await giftsAPI.getMyGifts(2025);
      setGifts(data);
    } catch (error) {
      console.error('Error:', error);
    } finally {
      setLoading(false);
    }
  };

  // Render
  return (
    <div className={styles.container}>
      {loading ? (
        <CircularProgress />
      ) : (
        gifts.map(gift => (
          <GiftListItem key={gift.id} gift={gift} />
        ))
      )}
    </div>
  );
};

export default MyListPage;
```

### Liste des Pages

| Page | Route | Accès | Description |
|------|-------|-------|-------------|
| Login | `/login` | Public | Authentification avec support migration MD5 |
| Home | `/` | Protected | Dashboard familial avec listes |
| MyList | `/my-list` | Protected | Gestion de sa liste personnelle |
| UserList | `/list/:userId` | Protected | Consultation liste d'un autre utilisateur |
| Cart | `/cart` | Protected | Panier des cadeaux réservés |
| Profile | `/profile` | Protected | Profil utilisateur |
| Admin | `/admin` | Admin only | Panneau d'administration |

## Couche Composants

Les composants sont réutilisables et peuvent être imbriqués dans les pages ou d'autres composants.

### Types de Composants

#### 1. Composants Layout (7 composants)

Gèrent la structure globale et la navigation :

- **NavigationBar.tsx** : Barre de navigation responsive avec menu mobile
- **ChristmasLayout.tsx** : Wrapper avec thème de Noël
- **Avatar.tsx** : Avatar utilisateur avec fallback initiales
- **ManagingChildBanner.tsx** : Alerte quand un parent gère un enfant
- **ProtectedRoute.tsx** : HOC pour protection des routes

#### 2. Composants Gifts (3 composants)

Gestion des cadeaux :

- **GiftFormDialog.tsx** : Modal création/édition avec extraction URL
- **GiftListItem.tsx** : Item de liste (ma liste, sans réservation)
- **ImportDialog.tsx** : Dialogue d'import depuis année précédente

#### 3. Composants UserList (2 composants)

Liste des cadeaux d'un autre utilisateur :

- **UserGiftListItem.tsx** : Item avec boutons réserver/participer
- **ReserveDialog.tsx** : Dialogue de confirmation de réservation

#### 4. Composants Profile (3 composants)

Gestion du profil :

- **ProfileForm.tsx** : Formulaire informations personnelles
- **AvatarUpload.tsx** : Upload/sélection d'avatar
- **PasswordChangeForm.tsx** : Changement de mot de passe

#### 5. Composants Admin (3 composants)

Administration :

- **AdminDashboard.tsx** : Statistiques avec graphiques
- **AdminUsers.tsx** : Gestion des utilisateurs
- **AdminFamilies.tsx** : Gestion des familles

### Conventions de Composants

**Props typées** :
```tsx
interface GiftListItemProps {
  gift: Gift;
  onEdit?: (gift: Gift) => void;
  onDelete?: (giftId: number) => void;
}

const GiftListItem: React.FC<GiftListItemProps> = ({ gift, onEdit, onDelete }) => {
  // ...
};
```

**Memo pour optimisation** :
```tsx
const GiftFormDialog = React.memo(({ open, gift, onClose, onSave }: Props) => {
  // Évite re-renders inutiles
});
```

**Callbacks memoizés** :
```tsx
const handleSubmit = useCallback(() => {
  // Action
}, [dependencies]);
```

## Couche State Management

### AuthContext (Global State)

Le contexte d'authentification gère l'état global de l'utilisateur.

**État géré** :
```typescript
interface AuthContextType {
  user: User | null;
  token: string | null;
  loading: boolean;
  managingChild: ManagingChild | null;
  isAuthenticated: boolean;
  login: (credentials: LoginCredentials) => Promise<void>;
  logout: () => void;
  updateUser: (userData: Partial<User>) => void;
  startManagingChild: (child: ManagingChild) => void;
  stopManagingChild: () => void;
}
```

**Persistence** : localStorage
- `nawel_token` : JWT token
- `nawel_user` : User object (sérialisé JSON)
- `nawel_managingChild` : Child info (sérialisé JSON)

**Usage dans les composants** :
```tsx
import { useAuth } from '../contexts/AuthContext';

const MyComponent = () => {
  const { user, isAuthenticated, logout } = useAuth();

  if (!isAuthenticated) return <Navigate to="/login" />;

  return <div>Bonjour {user?.firstName}</div>;
};
```

### Local State (useState)

Chaque page/composant gère son propre état local :

```tsx
const [gifts, setGifts] = useState<Gift[]>([]);
const [loading, setLoading] = useState(false);
const [error, setError] = useState<string | null>(null);
const [dialogOpen, setDialogOpen] = useState(false);
```

## Couche Services (API)

### Organisation

Le fichier `services/api.ts` (154 lignes) centralise toutes les requêtes API :

```typescript
// Configuration Axios
const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000/api';

const api = axios.create({
  baseURL: API_URL,
  headers: { 'Content-Type': 'application/json' }
});

// Intercepteur Request : Ajoute le token JWT
api.interceptors.request.use(config => {
  const token = localStorage.getItem('nawel_token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Intercepteur Response : Gère les erreurs 401
api.interceptors.response.use(
  response => response,
  error => {
    if (error.response?.status === 401) {
      localStorage.clear();
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);
```

### Groupes API

| Groupe | Endpoints | Exemples |
|--------|-----------|----------|
| **authAPI** | 5 | login, validateToken, requestPasswordReset |
| **usersAPI** | 6 | getMe, updateMe, uploadAvatar |
| **listsAPI** | 2 | getAll, getMine |
| **giftsAPI** | 12 | getMyGifts, createGift, reserveGift |
| **productsAPI** | 1 | extractInfo |
| **adminAPI** | 9 | getStats, createUser, deleteUser |

**Exemple d'utilisation** :
```tsx
import { giftsAPI } from '../services/api';

const MyComponent = () => {
  const handleReserve = async (giftId: number) => {
    try {
      await giftsAPI.reserveGift(giftId, { comment: 'Avec plaisir!' });
      alert('Cadeau réservé !');
    } catch (error) {
      console.error('Erreur:', error);
    }
  };
};
```

## Routing & Navigation

### Configuration (App.tsx)

```tsx
<Router>
  <ChristmasLayout>
    <NavigationBar />
    {managingChild && <ManagingChildBanner />}

    <Routes>
      <Route path="/login" element={<Login />} />

      <Route path="/" element={
        <ProtectedRoute>
          <Home />
        </ProtectedRoute>
      } />

      <Route path="/my-list" element={
        <ProtectedRoute>
          <MyList />
        </ProtectedRoute>
      } />

      {/* ... autres routes protégées */}

      <Route path="*" element={<Navigate to="/" />} />
    </Routes>
  </ChristmasLayout>
</Router>
```

### ProtectedRoute

Composant HOC pour protéger les routes :

```tsx
const ProtectedRoute: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const { isAuthenticated, loading } = useAuth();

  if (loading) {
    return <CircularProgress />;
  }

  if (!isAuthenticated) {
    return <Navigate to="/login" />;
  }

  return <>{children}</>;
};
```

## Styling

### Approche : CSS Modules + LESS

Chaque composant/page a son propre fichier `.module.less` :

```less
// MyList.module.less
.container {
  padding: 2rem;
  max-width: 1200px;
  margin: 0 auto;
}

.giftCard {
  background: white;
  border-radius: 12px;
  padding: 1rem;

  &:hover {
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
  }
}
```

**Usage** :
```tsx
import styles from '../css/pages/MyList.module.less';

const MyList = () => (
  <div className={styles.container}>
    <div className={styles.giftCard}>...</div>
  </div>
);
```

### Thème Material-UI

Configuration dans `main.tsx` :

```tsx
const theme = createTheme({
  palette: {
    primary: {
      main: '#2d5f3f', // Vert forêt (thème Noël)
    },
    secondary: {
      main: '#b8860b', // Or/doré
    },
    background: {
      default: '#f8f6f3', // Beige/crème
    },
  },
  typography: {
    fontFamily: '"Roboto", "Helvetica", "Arial", sans-serif',
  },
  components: {
    MuiButton: {
      styleOverrides: {
        root: {
          borderRadius: 8,
          textTransform: 'none',
        },
      },
    },
    MuiCard: {
      styleOverrides: {
        root: {
          borderRadius: 12,
          boxShadow: '0 2px 8px rgba(0, 0, 0, 0.1)',
        },
      },
    },
  },
});
```

## Types TypeScript

### Organisation

Les types sont organisés par domaine dans `src/types/` :

**user.ts** :
```typescript
export interface User {
  id: number;
  login: string;
  email: string | null;
  firstName: string | null;
  lastName: string | null;
  avatar: string | null;
  pseudo: string | null;
  isChildren: boolean;
  isAdmin: boolean;
  familyId: number;
  familyName: string | null;
  notifyListEdit: boolean;
  notifyGiftTaken: boolean;
  displayPopup: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface LoginCredentials {
  login: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  user: User;
}
```

**gift.ts** :
```typescript
export interface Gift {
  id: number;
  name: string;
  description: string | null;
  link: string | null;
  cost: number | null;
  currency: string;
  image: string | null;
  year: number;
  listId: number;
  takenBy: number | null;
  takenByUser?: User;
  available: boolean;
  comment: string | null;
  isGroupGift: boolean;
  participants?: GiftParticipation[];
  createdAt: string;
  updatedAt: string;
}
```

### Type Safety

TypeScript est strictement configuré dans `tsconfig.json` :

```json
{
  "compilerOptions": {
    "strict": true,
    "noUnusedLocals": true,
    "noUnusedParameters": true,
    "noImplicitReturns": true,
    "noFallthroughCasesInSwitch": true
  }
}
```

## Patterns & Bonnes Pratiques

### 1. Composition de Composants

```tsx
<ChristmasLayout>
  <NavigationBar />
  <ManagingChildBanner />
  <PageContent />
</ChristmasLayout>
```

### 2. Hooks Personnalisés (non utilisé actuellement)

```tsx
// hooks/useGifts.ts
export const useGifts = (year: number) => {
  const [gifts, setGifts] = useState<Gift[]>([]);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    fetchGifts(year);
  }, [year]);

  return { gifts, loading, /* ... */ };
};
```

### 3. Memoization

```tsx
// Évite re-renders inutiles
const GiftFormDialog = React.memo(({ gift, onSave }: Props) => {
  // ...
});

// Callbacks memoizés
const handleSubmit = useCallback(() => {
  onSave(formData);
}, [formData, onSave]);
```

### 4. Error Boundaries (à implémenter)

```tsx
class ErrorBoundary extends React.Component {
  componentDidCatch(error, errorInfo) {
    console.error('Error:', error, errorInfo);
  }

  render() {
    return this.props.children;
  }
}
```

## Flux de Données

### Flux d'Authentification

```
1. User entre credentials → Login.tsx
2. Login.tsx appelle authAPI.login()
3. AuthContext stocke user + token
4. localStorage sauvegarde
5. Redirect vers Home
6. NavigationBar affiche user connecté
```

### Flux de Réservation de Cadeau

```
1. User clique "Réserver" → UserGiftListItem.tsx
2. ReserveDialog s'ouvre pour confirmation
3. User confirme → giftsAPI.reserveGift(id, comment)
4. Backend traite + envoie email (debounced)
5. Frontend met à jour l'état local
6. UI affiche "Réservé"
```

### Flux de Gestion Enfant

```
1. Parent clique "Gérer" sur enfant → Home.tsx
2. AuthContext.startManagingChild(childInfo)
3. localStorage sauvegarde managingChild
4. ManagingChildBanner s'affiche
5. MyList.tsx charge la liste de l'enfant
6. Parent peut éditer la liste
7. Parent clique "Revenir" → stopManagingChild()
```

## Performance

### Optimisations Implémentées

1. **React.memo** : Composants lourds (GiftFormDialog, ProfileForm, etc.)
2. **useCallback** : Callbacks passés aux enfants
3. **Lazy Loading** : Non implémenté (à faire)
4. **Code Splitting** : Non implémenté (à faire)

### Optimisations Futures

```tsx
// Lazy loading des pages
const AdminPage = lazy(() => import('./pages/Admin'));
const ProfilePage = lazy(() => import('./pages/Profile'));

// Utilisation
<Suspense fallback={<CircularProgress />}>
  <AdminPage />
</Suspense>
```

## Build & Déploiement

### Configuration Vite

```typescript
// vite.config.ts
export default defineConfig({
  plugins: [react()],
  server: {
    port: 3000,
    proxy: {
      '/api': {
        target: 'http://localhost:5000',
        changeOrigin: true,
      },
    },
  },
  build: {
    outDir: 'dist',
    sourcemap: false,
    rollupOptions: {
      output: {
        manualChunks: {
          'react-vendor': ['react', 'react-dom', 'react-router-dom'],
          'mui-vendor': ['@mui/material', '@mui/icons-material'],
        },
      },
    },
  },
});
```

### Variables d'Environnement

`.env` :
```bash
VITE_API_URL=http://localhost:5000/api
```

`.env.production` :
```bash
VITE_API_URL=https://api.nawel.com/api
```

### Build Production

```bash
npm run build  # Génère dist/
npm run preview  # Preview du build localement
```

## Tests

### Configuration Vitest

```typescript
// vitest.config.ts
export default defineConfig({
  plugins: [react()],
  test: {
    globals: true,
    environment: 'jsdom',
    setupFiles: './src/test/setup.ts',
    coverage: {
      provider: 'v8',
      reporter: ['text', 'html'],
    },
  },
});
```

### Types de Tests

**Tests unitaires** :
```tsx
// Avatar.test.tsx
describe('Avatar Component', () => {
  it('renders user initials when no avatar', () => {
    render(<Avatar user={mockUser} size="medium" />);
    expect(screen.getByText('TU')).toBeInTheDocument();
  });
});
```

**Tests d'intégration** :
```tsx
// AuthContext.test.tsx
describe('AuthContext', () => {
  it('login flow works correctly', async () => {
    const { result } = renderHook(() => useAuth(), {
      wrapper: AuthProvider,
    });

    await act(async () => {
      await result.current.login({ login: 'admin', password: 'admin' });
    });

    expect(result.current.isAuthenticated).toBe(true);
  });
});
```

### Couverture Actuelle

- **Avatar.tsx** : ✅ Tests complets
- **AuthContext.tsx** : ✅ Tests complets
- **giftHelpers.ts** : ✅ Tests complets
- **Autres** : ❌ À implémenter

## Sécurité

### Protection XSS

- **React** : Échappement automatique des valeurs
- **DOMPurify** : Non utilisé (à implémenter pour HTML externe)

### Protection CSRF

- **Stateless JWT** : Pas de cookies = pas de CSRF

### Validation des Données

```tsx
// Validation côté client
const isValidUrl = (url: string): boolean => {
  try {
    new URL(url);
    return true;
  } catch {
    return false;
  }
};

// Validation backend via API
const handleSubmit = async () => {
  try {
    await giftsAPI.createGift(formData);
  } catch (error) {
    if (error.response?.status === 400) {
      setErrors(error.response.data.errors);
    }
  }
};
```

### Gestion des Secrets

- **Token JWT** : localStorage (vulnérable XSS mais acceptable pour SPA)
- **API URL** : Variable d'environnement (`VITE_API_URL`)
- **Pas de secrets côté client** : Tout est côté backend

## Accessibilité

### Bonnes Pratiques

- **Material-UI** : Composants accessibles par défaut
- **Aria labels** : À améliorer
- **Clavier** : Navigation via Tab (MUI gère)
- **Contraste** : Thème vérifié WCAG AA

### Améliorations Futures

```tsx
// Ajouter aria-label explicites
<IconButton aria-label="Supprimer le cadeau">
  <DeleteIcon />
</IconButton>

// Focus management
<Dialog
  open={open}
  onClose={handleClose}
  aria-labelledby="dialog-title"
  aria-describedby="dialog-description"
>
```

## Debugging

### React DevTools

- Extension Chrome/Firefox pour inspecter composants
- Voir state, props, hooks

### Logger Personnalisé

```typescript
// utils/logger.ts
const isDev = import.meta.env.MODE === 'development';

export const logger = {
  debug: (...args: any[]) => isDev && console.log('[DEBUG]', ...args),
  info: (...args: any[]) => isDev && console.info('[INFO]', ...args),
  warn: (...args: any[]) => isDev && console.warn('[WARN]', ...args),
  error: (...args: any[]) => console.error('[ERROR]', ...args),
};
```

## Références

- [React Documentation](https://react.dev/)
- [TypeScript Documentation](https://www.typescriptlang.org/docs/)
- [Material-UI Documentation](https://mui.com/material-ui/)
- [Vite Documentation](https://vitejs.dev/)
- [React Router Documentation](https://reactrouter.com/)
- [Vitest Documentation](https://vitest.dev/)
