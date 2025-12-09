# Guide de Tests Frontend - Nawel App

## Vue d'Ensemble

L'application Nawel utilise une stack de tests moderne et performante :

- **Vitest 4.0.15** : Framework de tests rapide et compatible Vite
- **React Testing Library 16.3.0** : Tests centrés sur le comportement utilisateur
- **@testing-library/user-event 14.6.1** : Simulation d'interactions utilisateur réalistes
- **@testing-library/jest-dom 6.9.1** : Matchers supplémentaires pour les assertions DOM
- **jsdom 27.3.0** : Simulation d'environnement DOM pour Node.js

**Philosophie de test** : Les tests se concentrent sur **ce que l'utilisateur voit et fait**, pas sur les détails d'implémentation.

---

## Table des Matières

1. [Configuration](#configuration)
2. [Structure des Tests](#structure-des-tests)
3. [Écrire des Tests](#écrire-des-tests)
4. [Stratégies de Mocking](#stratégies-de-mocking)
5. [Tests de Composants](#tests-de-composants)
6. [Tests de Contexts](#tests-de-contexts)
7. [Tests de Pages](#tests-de-pages)
8. [Assertions Courantes](#assertions-courantes)
9. [Utilitaires de Test](#utilitaires-de-test)
10. [Exécution des Tests](#exécution-des-tests)
11. [Couverture de Code](#couverture-de-code)
12. [Bonnes Pratiques](#bonnes-pratiques)
13. [Patterns Avancés](#patterns-avancés)
14. [Dépannage](#dépannage)

---

## Configuration

### vitest.config.ts

```typescript
import { defineConfig } from 'vitest/config';
import react from '@vitejs/plugin-react';
import path from 'path';

export default defineConfig({
  plugins: [react()],
  test: {
    globals: true,                    // Pas besoin d'importer describe, it, expect
    environment: 'jsdom',             // Environnement DOM simulé
    setupFiles: './src/test/setup.ts', // Setup global
    coverage: {
      provider: 'v8',
      reporter: ['text', 'json', 'html'],
      exclude: [
        'node_modules/',
        'src/test/',
        '**/*.d.ts',
        '**/*.config.*',
        '**/mockData',
        'dist/',
      ],
    },
  },
  resolve: {
    alias: {
      '@': path.resolve(__dirname, './src'),  // Alias @ pour imports
    },
  },
});
```

### src/test/setup.ts

Fichier de configuration exécuté avant tous les tests :

```typescript
import { expect, afterEach, vi } from 'vitest';
import { cleanup } from '@testing-library/react';
import * as matchers from '@testing-library/jest-dom/matchers';

// Extend Vitest's expect with jest-dom matchers
expect.extend(matchers);

// Cleanup after each test case
afterEach(() => {
  cleanup();  // Unmount tous les composants React après chaque test
});

// Mock window.matchMedia (nécessaire pour Material-UI)
Object.defineProperty(window, 'matchMedia', {
  writable: true,
  value: vi.fn().mockImplementation((query) => ({
    matches: false,
    media: query,
    onchange: null,
    addListener: vi.fn(),
    removeListener: vi.fn(),
    addEventListener: vi.fn(),
    removeEventListener: vi.fn(),
    dispatchEvent: vi.fn(),
  })),
});
```

**Ce que fait setup.ts** :
1. Ajoute les matchers `jest-dom` (toBeInTheDocument, toHaveTextContent, etc.)
2. Nettoie automatiquement le DOM après chaque test
3. Mock `window.matchMedia` pour Material-UI responsive

### package.json Scripts

```json
{
  "scripts": {
    "test": "vitest",                    // Mode watch
    "test:ui": "vitest --ui",            // Interface UI interactive
    "test:coverage": "vitest --coverage" // Rapport de couverture
  }
}
```

---

## Structure des Tests

### Organisation des Fichiers

```
src/
├── components/
│   ├── Avatar.tsx
│   ├── Avatar.test.tsx              # Tests du composant Avatar
│   ├── NavigationBar.tsx
│   └── NavigationBar.test.tsx
├── contexts/
│   ├── AuthContext.tsx
│   └── AuthContext.test.tsx         # Tests du context
├── pages/
│   ├── Login.tsx
│   └── Login.test.tsx               # Tests de la page
├── utils/
│   ├── helpers.ts
│   └── helpers.test.ts              # Tests des fonctions utilitaires
└── test/
    ├── setup.ts                     # Configuration globale
    ├── utils.tsx                    # Utilitaires de test réutilisables
    └── vitest-setup.d.ts            # Types TypeScript pour Vitest
```

**Convention** : Fichier de test à côté du fichier source avec l'extension `.test.tsx` ou `.test.ts`.

### Anatomie d'un Fichier de Test

```typescript
// 1. Imports
import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import MyComponent from './MyComponent';

// 2. Mocks
vi.mock('../services/api', () => ({
  authAPI: {
    login: vi.fn(),
  },
}));

// 3. Suite de tests
describe('MyComponent', () => {
  // 4. Setup/Teardown
  beforeEach(() => {
    vi.clearAllMocks();
  });

  afterEach(() => {
    localStorage.clear();
  });

  // 5. Tests
  it('should render correctly', () => {
    render(<MyComponent />);
    expect(screen.getByText('Hello')).toBeInTheDocument();
  });

  it('should handle user interactions', async () => {
    const user = userEvent.setup();
    render(<MyComponent />);

    const button = screen.getByRole('button', { name: /click me/i });
    await user.click(button);

    expect(screen.getByText('Clicked!')).toBeInTheDocument();
  });
});
```

---

## Écrire des Tests

### 1. Tests de Composants Simples

**Exemple** : Composant Avatar

```typescript
import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import Avatar from './Avatar';
import type { User } from '../types';

// Mock LESS module
vi.mock('../css/Avatar.module.less', () => ({
  default: { avatar: 'avatar-mock' },
}));

describe('Avatar Component', () => {
  const mockUser: User = {
    id: 1,
    login: 'testuser',
    firstName: 'John',
    lastName: 'Doe',
    avatar: 'avatar.png',
    // ... autres champs
  };

  it('should render with user initials when no avatar URL', () => {
    const userNoAvatar = { ...mockUser, avatar: '' };
    render(<Avatar user={userNoAvatar} />);

    expect(screen.getByText('JD')).toBeInTheDocument();
  });

  it('should render with first name initial only when no last name', () => {
    const userFirstNameOnly = { ...mockUser, lastName: '', avatar: '' };
    render(<Avatar user={userFirstNameOnly} />);

    expect(screen.getByText('J')).toBeInTheDocument();
  });

  it('should render with ? when user is null', () => {
    render(<Avatar user={null} />);
    expect(screen.getByText('?')).toBeInTheDocument();
  });

  it('should apply custom className', () => {
    const userNoAvatar = { ...mockUser, avatar: '' };
    const { container } = render(<Avatar user={userNoAvatar} className="custom-class" />);

    const avatar = container.querySelector('.custom-class');
    expect(avatar).toBeInTheDocument();
  });

  it('should use alt text from user firstName', () => {
    render(<Avatar user={mockUser} />);

    const avatar = screen.getByAltText('John');
    expect(avatar).toBeInTheDocument();
  });
});
```

**Points clés** :
- ✅ Teste le rendu visuel (initiales, texte, images)
- ✅ Teste les props (className, size, user)
- ✅ Teste les cas limites (null, undefined, champs vides)
- ✅ Mock les modules CSS

### 2. Tests de Composants avec Interactions

**Exemple** : Page Login avec interactions utilisateur

```typescript
import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { BrowserRouter } from 'react-router-dom';
import Login from './Login';
import { useAuth } from '../contexts/AuthContext';
import * as api from '../services/api';

const mockNavigate = vi.fn();
const mockLogin = vi.fn();

vi.mock('react-router-dom', async () => {
  const actual = await vi.importActual('react-router-dom');
  return {
    ...actual,
    useNavigate: () => mockNavigate,
  };
});

vi.mock('../contexts/AuthContext', () => ({
  useAuth: vi.fn(),
}));

vi.mock('../services/api', () => ({
  authAPI: {
    requestMigrationReset: vi.fn(),
  },
}));

const renderLogin = () => {
  return render(
    <BrowserRouter>
      <Login />
    </BrowserRouter>
  );
};

describe('Login - MD5 Migration Flow', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    localStorage.clear();

    vi.mocked(useAuth).mockReturnValue({
      user: null,
      token: null,
      loading: false,
      managingChild: null,
      login: mockLogin,
      logout: vi.fn(),
      updateUser: vi.fn(),
      startManagingChild: vi.fn(),
      stopManagingChild: vi.fn(),
      isAuthenticated: false,
    });
  });

  it('should display migration warning when LEGACY_PASSWORD error is returned', async () => {
    const user = userEvent.setup();

    mockLogin.mockResolvedValueOnce({
      success: false,
      errorCode: 'LEGACY_PASSWORD',
      email: 'user@example.com',
    });

    renderLogin();

    const loginInput = screen.getByLabelText(/identifiant/i);
    const passwordInput = screen.getByLabelText(/mot de passe/i);

    await user.type(loginInput, 'testuser');
    await user.type(passwordInput, 'password123');

    const submitButton = screen.getByRole('button', { name: /se connecter/i });
    await user.click(submitButton);

    await waitFor(() => {
      expect(screen.getByText(/mise à jour de sécurité requise/i)).toBeInTheDocument();
    });

    expect(
      screen.getByText(/pour améliorer la sécurité de votre compte/i)
    ).toBeInTheDocument();
  });

  it('should call requestMigrationReset when reset button is clicked', async () => {
    const user = userEvent.setup();

    mockLogin.mockResolvedValueOnce({
      success: false,
      errorCode: 'LEGACY_PASSWORD',
      email: 'user@example.com',
    });

    vi.mocked(api.authAPI.requestMigrationReset).mockResolvedValueOnce({
      data: { message: 'Email envoyé' },
      status: 200,
      statusText: 'OK',
      headers: {},
      config: {} as any,
    });

    renderLogin();

    const loginInput = screen.getByLabelText(/identifiant/i);
    const passwordInput = screen.getByLabelText(/mot de passe/i);

    await user.type(loginInput, 'testuser');
    await user.type(passwordInput, 'password123');

    const submitButton = screen.getByRole('button', { name: /se connecter/i });
    await user.click(submitButton);

    await waitFor(() => {
      expect(screen.getByText(/mise à jour de sécurité requise/i)).toBeInTheDocument();
    });

    const resetButton = screen.getByRole('button', {
      name: /recevoir un email de réinitialisation/i,
    });
    await user.click(resetButton);

    await waitFor(() => {
      expect(api.authAPI.requestMigrationReset).toHaveBeenCalledWith({
        login: 'testuser',
      });
    });
  });

  it('should show loading state while sending migration email', async () => {
    const user = userEvent.setup();

    mockLogin.mockResolvedValueOnce({
      success: false,
      errorCode: 'LEGACY_PASSWORD',
      email: 'user@example.com',
    });

    vi.mocked(api.authAPI.requestMigrationReset).mockImplementationOnce(
      () =>
        new Promise((resolve) =>
          setTimeout(
            () =>
              resolve({
                data: { message: 'Email envoyé' },
                status: 200,
                statusText: 'OK',
                headers: {},
                config: {} as any,
              }),
            100
          )
        )
    );

    renderLogin();

    const loginInput = screen.getByLabelText(/identifiant/i);
    const passwordInput = screen.getByLabelText(/mot de passe/i);

    await user.type(loginInput, 'testuser');
    await user.type(passwordInput, 'password123');

    const submitButton = screen.getByRole('button', { name: /se connecter/i });
    await user.click(submitButton);

    await waitFor(() => {
      expect(screen.getByText(/mise à jour de sécurité requise/i)).toBeInTheDocument();
    });

    const resetButton = screen.getByRole('button', {
      name: /recevoir un email de réinitialisation/i,
    });

    await user.click(resetButton);

    // Button should be disabled during loading
    expect(resetButton).toBeDisabled();
  });
});
```

**Points clés** :
- ✅ `userEvent.setup()` pour interactions réalistes
- ✅ `waitFor()` pour opérations asynchrones
- ✅ Teste le flow complet utilisateur
- ✅ Vérifie les états de chargement
- ✅ Vérifie les appels API

---

## Stratégies de Mocking

### 1. Mock de Modules CSS/LESS

```typescript
vi.mock('../css/MyComponent.module.less', () => ({
  default: {
    container: 'container-mock',
    title: 'title-mock',
  },
}));
```

**Pourquoi** : Les fichiers CSS ne peuvent pas être importés directement dans Node.js.

### 2. Mock de React Router

**Mock de `useNavigate`** :
```typescript
const mockNavigate = vi.fn();

vi.mock('react-router-dom', async () => {
  const actual = await vi.importActual('react-router-dom');
  return {
    ...actual,  // Garde les autres exports intacts
    useNavigate: () => mockNavigate,
  };
});
```

**Mock de `useParams`** :
```typescript
vi.mock('react-router-dom', async () => {
  const actual = await vi.importActual('react-router-dom');
  return {
    ...actual,
    useParams: () => ({ id: '123' }),
  };
});
```

### 3. Mock de Contexts React

**Mock de `useAuth`** :
```typescript
vi.mock('../contexts/AuthContext', () => ({
  useAuth: vi.fn(),
}));

// Dans le test
beforeEach(() => {
  vi.mocked(useAuth).mockReturnValue({
    user: mockUser,
    token: 'test-token',
    loading: false,
    login: vi.fn(),
    logout: vi.fn(),
    isAuthenticated: true,
  });
});
```

### 4. Mock d'API

**Mock complet du module API** :
```typescript
vi.mock('../services/api', () => ({
  authAPI: {
    login: vi.fn(),
    logout: vi.fn(),
  },
  giftsAPI: {
    getAllGifts: vi.fn(),
    createGift: vi.fn(),
  },
}));

// Dans le test
import * as api from '../services/api';

it('should call login API', async () => {
  vi.mocked(api.authAPI.login).mockResolvedValueOnce({
    data: { token: 'abc123', user: mockUser },
    status: 200,
  });

  // Test...

  expect(api.authAPI.login).toHaveBeenCalledWith({
    login: 'testuser',
    password: 'password123',
  });
});
```

### 5. Mock de localStorage

**Pas besoin de mocker** : jsdom inclut une implémentation de localStorage.

```typescript
beforeEach(() => {
  localStorage.clear();
});

afterEach(() => {
  localStorage.clear();
});

// Dans le test
it('should store token in localStorage', () => {
  localStorage.setItem('token', 'abc123');
  expect(localStorage.getItem('token')).toBe('abc123');
});
```

---

## Tests de Composants

### Sélectionner des Éléments

**Ordre de priorité recommandé** :

1. **`getByRole`** (préféré) : Sélection par rôle ARIA
   ```typescript
   screen.getByRole('button', { name: /se connecter/i });
   screen.getByRole('textbox', { name: /email/i });
   screen.getByRole('checkbox', { name: /accepter/i });
   ```

2. **`getByLabelText`** : Sélection par label (formulaires)
   ```typescript
   screen.getByLabelText(/identifiant/i);
   screen.getByLabelText(/mot de passe/i);
   ```

3. **`getByPlaceholderText`** : Sélection par placeholder
   ```typescript
   screen.getByPlaceholderText('Entrez votre email');
   ```

4. **`getByText`** : Sélection par texte visible
   ```typescript
   screen.getByText('Bienvenue');
   screen.getByText(/bienvenue/i);  // Case insensitive
   ```

5. **`getByTestId`** : Dernier recours
   ```typescript
   screen.getByTestId('user-name');
   // Dans le composant: <div data-testid="user-name">John</div>
   ```

**Variants** :
- `getBy*` : Retourne l'élément ou lance une erreur
- `queryBy*` : Retourne l'élément ou `null` (pour tester absence)
- `findBy*` : Retourne une Promise (pour éléments asynchrones)

```typescript
// getBy: élément doit exister
expect(screen.getByText('Hello')).toBeInTheDocument();

// queryBy: tester absence
expect(screen.queryByText('Hidden')).not.toBeInTheDocument();

// findBy: attendre apparition asynchrone
const element = await screen.findByText('Loaded');
expect(element).toBeInTheDocument();
```

### Interactions Utilisateur

**Utiliser `userEvent` (recommandé)** :
```typescript
import userEvent from '@testing-library/user-event';

it('should handle user input', async () => {
  const user = userEvent.setup();

  render(<MyForm />);

  // Taper du texte
  const input = screen.getByLabelText(/email/i);
  await user.type(input, 'test@example.com');

  // Cliquer sur un bouton
  const button = screen.getByRole('button', { name: /envoyer/i });
  await user.click(button);

  // Cocher une checkbox
  const checkbox = screen.getByRole('checkbox');
  await user.click(checkbox);
});
```

**Alternatives (moins réalistes)** :
```typescript
import { fireEvent } from '@testing-library/react';

// Moins réaliste, mais plus rapide
fireEvent.change(input, { target: { value: 'test@example.com' } });
fireEvent.click(button);
```

---

## Tests de Contexts

### Test de AuthContext

```typescript
import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import { AuthProvider, useAuth } from './AuthContext';
import type { User } from '../types';

vi.mock('../services/api', () => ({
  authAPI: {
    login: vi.fn(),
  },
}));

// Composant de test pour accéder au context
const TestComponent = () => {
  const { user, isAuthenticated, logout } = useAuth();
  return (
    <div>
      {isAuthenticated ? (
        <>
          <div data-testid="user-name">{user?.firstName || user?.login}</div>
          <button onClick={logout}>Logout</button>
        </>
      ) : (
        <div data-testid="not-authenticated">Not authenticated</div>
      )}
    </div>
  );
};

describe('AuthContext', () => {
  const mockUser: User = {
    id: 1,
    login: 'testuser',
    firstName: 'John',
    lastName: 'Doe',
    // ... autres champs
  };

  beforeEach(() => {
    localStorage.clear();
    vi.clearAllMocks();
  });

  afterEach(() => {
    localStorage.clear();
  });

  it('should initialize with no user when localStorage is empty', async () => {
    render(
      <AuthProvider>
        <TestComponent />
      </AuthProvider>
    );

    await waitFor(() => {
      expect(screen.getByTestId('not-authenticated')).toBeInTheDocument();
    });
  });

  it('should restore user from localStorage on mount', async () => {
    localStorage.setItem('token', 'test-token');
    localStorage.setItem('user', JSON.stringify(mockUser));

    render(
      <AuthProvider>
        <TestComponent />
      </AuthProvider>
    );

    await waitFor(() => {
      expect(screen.getByTestId('user-name')).toHaveTextContent('John');
    });
  });

  it('should clear localStorage on logout', async () => {
    localStorage.setItem('token', 'test-token');
    localStorage.setItem('user', JSON.stringify(mockUser));

    render(
      <AuthProvider>
        <TestComponent />
      </AuthProvider>
    );

    await waitFor(() => {
      expect(screen.getByTestId('user-name')).toBeInTheDocument();
    });

    const logoutButton = screen.getByText('Logout');
    logoutButton.click();

    await waitFor(() => {
      expect(screen.getByTestId('not-authenticated')).toBeInTheDocument();
      expect(localStorage.getItem('token')).toBeNull();
      expect(localStorage.getItem('user')).toBeNull();
    });
  });
});
```

**Pattern** :
1. Créer un `TestComponent` qui consomme le context
2. Render `TestComponent` à l'intérieur du `Provider`
3. Tester les valeurs du context via le `TestComponent`

---

## Tests de Pages

### Test de Page Complète

```typescript
import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import { BrowserRouter } from 'react-router-dom';
import Home from './Home';
import { AuthProvider } from '../contexts/AuthContext';

// Mock de l'API
vi.mock('../services/api', () => ({
  familiesAPI: {
    getAllFamilies: vi.fn(),
  },
}));

const renderHome = () => {
  return render(
    <BrowserRouter>
      <AuthProvider>
        <Home />
      </AuthProvider>
    </BrowserRouter>
  );
};

describe('Home Page', () => {
  it('should render families list', async () => {
    const mockFamilies = [
      {
        id: 1,
        name: 'Famille Dupont',
        users: [
          { id: 1, firstName: 'Jean', lastName: 'Dupont' },
          { id: 2, firstName: 'Marie', lastName: 'Dupont' },
        ],
      },
    ];

    vi.mocked(api.familiesAPI.getAllFamilies).mockResolvedValueOnce({
      data: mockFamilies,
      status: 200,
    });

    renderHome();

    await waitFor(() => {
      expect(screen.getByText('Famille Dupont')).toBeInTheDocument();
      expect(screen.getByText('Jean Dupont')).toBeInTheDocument();
      expect(screen.getByText('Marie Dupont')).toBeInTheDocument();
    });
  });
});
```

**Points clés** :
- Wrapper avec tous les Providers nécessaires (Router, Auth, Theme, etc.)
- Créer une fonction `renderPage()` réutilisable
- Tester le chargement des données
- Tester les états (loading, error, success)

---

## Assertions Courantes

### Matchers `jest-dom`

```typescript
// Présence dans le DOM
expect(element).toBeInTheDocument();
expect(element).not.toBeInTheDocument();

// Texte
expect(element).toHaveTextContent('Hello');
expect(element).toHaveTextContent(/hello/i);  // Case insensitive

// Attributs
expect(input).toHaveValue('test@example.com');
expect(input).toHaveAttribute('type', 'email');
expect(link).toHaveAttribute('href', '/home');

// Classes CSS
expect(element).toHaveClass('active');
expect(element).toHaveClass('btn btn-primary');

// Styles
expect(element).toHaveStyle({ color: 'red' });
expect(element).toHaveStyle('display: none');

// États
expect(button).toBeDisabled();
expect(button).toBeEnabled();
expect(checkbox).toBeChecked();
expect(input).toHaveFocus();
expect(form).toBeInvalid();
expect(form).toBeValid();

// Visibilité
expect(element).toBeVisible();
expect(element).not.toBeVisible();
expect(element).toBeEmptyDOMElement();
```

### Matchers Vitest Standards

```typescript
// Égalité
expect(result).toBe(true);
expect(result).toEqual({ name: 'John' });
expect(array).toContain('item');

// Nombres
expect(count).toBeGreaterThan(5);
expect(count).toBeLessThan(10);
expect(price).toBeCloseTo(19.99, 2);

// Strings
expect(text).toMatch(/hello/i);
expect(text).toContain('world');

// Arrays/Objects
expect(array).toHaveLength(3);
expect(obj).toHaveProperty('name', 'John');

// Fonctions
expect(mockFn).toHaveBeenCalled();
expect(mockFn).toHaveBeenCalledTimes(2);
expect(mockFn).toHaveBeenCalledWith('arg1', 'arg2');
expect(mockFn).toHaveBeenLastCalledWith('arg');

// Exceptions
expect(() => fn()).toThrow();
expect(() => fn()).toThrow('Error message');

// Promises
await expect(promise).resolves.toBe('value');
await expect(promise).rejects.toThrow('Error');
```

---

## Utilitaires de Test

### Custom Render avec Providers

**src/test/utils.tsx** :
```typescript
import { ReactElement } from 'react';
import { render, RenderOptions } from '@testing-library/react';
import { BrowserRouter } from 'react-router-dom';
import { ThemeProvider } from '@mui/material/styles';
import { AuthProvider } from '../contexts/AuthContext';
import theme from '../theme';

interface CustomRenderOptions extends Omit<RenderOptions, 'wrapper'> {
  initialRoute?: string;
}

export const renderWithProviders = (
  ui: ReactElement,
  options?: CustomRenderOptions
) => {
  const { initialRoute = '/', ...renderOptions } = options || {};

  window.history.pushState({}, 'Test page', initialRoute);

  return render(
    <BrowserRouter>
      <ThemeProvider theme={theme}>
        <AuthProvider>
          {ui}
        </AuthProvider>
      </ThemeProvider>
    </BrowserRouter>,
    renderOptions
  );
};

// Re-export everything from RTL
export * from '@testing-library/react';
export { default as userEvent } from '@testing-library/user-event';
```

**Usage** :
```typescript
import { renderWithProviders, screen } from '../test/utils';

it('should render with all providers', () => {
  renderWithProviders(<MyComponent />);
  expect(screen.getByText('Hello')).toBeInTheDocument();
});
```

### Mock Data Factories

**src/test/factories.ts** :
```typescript
import type { User, Gift } from '../types';

export const createMockUser = (overrides?: Partial<User>): User => ({
  id: 1,
  login: 'testuser',
  firstName: 'John',
  lastName: 'Doe',
  avatar: '',
  isChildren: false,
  isAdmin: false,
  familyId: 1,
  familyName: 'Test Family',
  email: 'test@example.com',
  notifyListEdit: false,
  notifyGiftTaken: false,
  displayPopup: true,
  createdAt: '2024-01-01T00:00:00Z',
  updatedAt: '2024-01-01T00:00:00Z',
  ...overrides,
});

export const createMockGift = (overrides?: Partial<Gift>): Gift => ({
  id: 1,
  name: 'Test Gift',
  description: 'Test description',
  link: 'https://example.com',
  cost: 19.99,
  currency: 'EUR',
  image: '',
  year: 2025,
  userId: 1,
  userName: 'John Doe',
  status: 'Available',
  isGroupGift: false,
  participants: [],
  createdAt: '2024-01-01T00:00:00Z',
  updatedAt: '2024-01-01T00:00:00Z',
  ...overrides,
});
```

**Usage** :
```typescript
import { createMockUser, createMockGift } from '../test/factories';

it('should display user avatar', () => {
  const user = createMockUser({ firstName: 'Alice', avatar: 'alice.png' });
  render(<Avatar user={user} />);
  // ...
});

it('should display gift price', () => {
  const gift = createMockGift({ cost: 49.99, currency: 'USD' });
  render(<GiftCard gift={gift} />);
  // ...
});
```

---

## Exécution des Tests

### Commandes

```bash
# Mode watch (démarre automatiquement, re-run à chaque changement)
npm test

# Run une fois et quitter
npm test -- --run

# Run tests spécifiques
npm test Avatar.test.tsx
npm test -- Avatar

# UI interactive (recommandé pour debug)
npm run test:ui

# Couverture de code
npm run test:coverage

# Mode verbose
npm test -- --reporter=verbose

# Filtrer par nom de test
npm test -- -t "should render"
```

### Mode Watch

Par défaut, Vitest tourne en mode watch :
- Re-run automatique à chaque sauvegarde
- Tests filtré automatiquement (seuls les fichiers modifiés)
- Interface interactive dans le terminal

**Commandes interactives** (dans le mode watch) :
- `a` : Run all tests
- `f` : Run only failed tests
- `t` : Filter by test name pattern
- `p` : Filter by filename pattern
- `q` : Quit

### UI Mode

```bash
npm run test:ui
```

Interface web sur `http://localhost:51204/__vitest__/` :
- Vue d'ensemble de tous les tests
- Filtrage interactif
- Inspection des assertions
- Vue du code source
- Re-run individuel

---

## Couverture de Code

### Rapport de Couverture

```bash
npm run test:coverage
```

**Formats générés** :
- **Console** : Résumé dans le terminal
- **HTML** : Rapport détaillé dans `coverage/index.html`
- **JSON** : Données brutes dans `coverage/coverage-final.json`

### Fichiers Exclus

**vitest.config.ts** :
```typescript
coverage: {
  exclude: [
    'node_modules/',       // Dépendances
    'src/test/',          // Fichiers de test
    '**/*.d.ts',          // Fichiers de types
    '**/*.config.*',      // Fichiers de config
    '**/mockData',        // Mock data
    'dist/',              // Build output
  ],
}
```

### Objectifs de Couverture

**Recommandations** :
- **Statements** : 80%+
- **Branches** : 75%+
- **Functions** : 80%+
- **Lines** : 80%+

**Prioriser** :
1. Logique métier critique (authentification, réservations, etc.)
2. Composants réutilisables (Avatar, forms, etc.)
3. Contexts et state management
4. Utilitaires et helpers

**Ne pas sur-tester** :
- Types TypeScript (déjà validés par le compilateur)
- Styles CSS (tests visuels manuels)
- Configuration (fichiers config simples)
- Mock data

### Rapport Actuel

**État actuel** (Décembre 2024) :
- **49 tests** passants
- **3 fichiers testés** :
  - `Avatar.test.tsx` : 13 tests
  - `AuthContext.test.tsx` : 20 tests
  - `Login.test.tsx` : 16 tests

---

## Bonnes Pratiques

### 1. Tester le Comportement, Pas l'Implémentation

**❌ Mauvais** :
```typescript
it('should call setState', () => {
  const { result } = renderHook(() => useState(0));
  act(() => result.current[1](1));
  expect(result.current[0]).toBe(1);  // Teste l'implémentation interne
});
```

**✅ Bon** :
```typescript
it('should display count after increment', async () => {
  const user = userEvent.setup();
  render(<Counter />);

  const button = screen.getByRole('button', { name: /increment/i });
  await user.click(button);

  expect(screen.getByText('Count: 1')).toBeInTheDocument();  // Teste ce que voit l'utilisateur
});
```

### 2. Éviter les Test IDs Sauf Nécessaire

**❌ Éviter** :
```typescript
const element = screen.getByTestId('submit-button');
```

**✅ Préférer** :
```typescript
const element = screen.getByRole('button', { name: /soumettre/i });
```

**Quand utiliser test-id** :
- Éléments sans rôle sémantique approprié
- Éléments dynamiques difficiles à sélectionner autrement

### 3. Utiliser `waitFor` pour Asynchrone

**❌ Mauvais** :
```typescript
render(<AsyncComponent />);
expect(screen.getByText('Loaded')).toBeInTheDocument();  // Peut échouer
```

**✅ Bon** :
```typescript
render(<AsyncComponent />);
await waitFor(() => {
  expect(screen.getByText('Loaded')).toBeInTheDocument();
});
```

### 4. Nommer les Tests Clairement

**❌ Vague** :
```typescript
it('should work', () => { ... });
```

**✅ Descriptif** :
```typescript
it('should display error message when login fails', () => { ... });
it('should disable submit button while form is submitting', () => { ... });
it('should restore user from localStorage on mount', () => { ... });
```

**Pattern** : `should [action] when [condition]`

### 5. Grouper les Tests Logiquement

```typescript
describe('Avatar Component', () => {
  describe('with user data', () => {
    it('should render initials when no avatar URL', () => { ... });
    it('should render avatar image when URL provided', () => { ... });
  });

  describe('without user data', () => {
    it('should render ? when user is null', () => { ... });
    it('should render ? when user is undefined', () => { ... });
  });

  describe('with custom props', () => {
    it('should apply custom className', () => { ... });
    it('should apply custom size', () => { ... });
  });
});
```

### 6. Nettoyer Après Chaque Test

```typescript
beforeEach(() => {
  vi.clearAllMocks();
  localStorage.clear();
});

afterEach(() => {
  localStorage.clear();
  sessionStorage.clear();
});
```

### 7. Utiliser Regex pour Texte

**Case insensitive** :
```typescript
screen.getByText(/bonjour/i);
screen.getByRole('button', { name: /se connecter/i });
```

**Matcher partiel** :
```typescript
screen.getByText(/mise à jour/i);  // Match "Mise à jour de sécurité requise"
```

---

## Patterns Avancés

### 1. Tester les Hooks Custom

```typescript
import { renderHook, act } from '@testing-library/react';
import useCounter from './useCounter';

it('should increment counter', () => {
  const { result } = renderHook(() => useCounter(0));

  expect(result.current.count).toBe(0);

  act(() => {
    result.current.increment();
  });

  expect(result.current.count).toBe(1);
});
```

### 2. Tester les Timers

```typescript
import { vi } from 'vitest';

it('should debounce API calls', async () => {
  vi.useFakeTimers();

  const mockApiCall = vi.fn();
  render(<SearchInput onSearch={mockApiCall} />);

  const input = screen.getByRole('textbox');
  await userEvent.type(input, 'test');

  // Pas d'appel immédiatement
  expect(mockApiCall).not.toHaveBeenCalled();

  // Avancer de 500ms
  vi.advanceTimersByTime(500);

  // Maintenant l'appel devrait avoir été fait
  expect(mockApiCall).toHaveBeenCalledWith('test');

  vi.useRealTimers();
});
```

### 3. Tester les Erreurs

```typescript
it('should display error when API fails', async () => {
  const mockApiCall = vi.fn().mockRejectedValueOnce(new Error('Network error'));

  render(<UserList fetchUsers={mockApiCall} />);

  await waitFor(() => {
    expect(screen.getByText(/erreur/i)).toBeInTheDocument();
  });
});
```

### 4. Tester les Redirections

```typescript
import { useNavigate } from 'react-router-dom';

const mockNavigate = vi.fn();
vi.mock('react-router-dom', async () => ({
  ...await vi.importActual('react-router-dom'),
  useNavigate: () => mockNavigate,
}));

it('should redirect after login', async () => {
  render(<Login />);

  // ... login process

  await waitFor(() => {
    expect(mockNavigate).toHaveBeenCalledWith('/home');
  });
});
```

### 5. Snapshot Testing (À utiliser avec parcimonie)

```typescript
it('should match snapshot', () => {
  const { container } = render(<MyComponent />);
  expect(container).toMatchSnapshot();
});
```

**Quand utiliser** :
- Composants statiques (Footer, Header, etc.)
- Vérifier qu'un changement n'a pas cassé le rendu

**Quand NE PAS utiliser** :
- Composants dynamiques
- Composants avec beaucoup de logique

---

## Dépannage

### Erreur: `toBeInTheDocument` is not a function

**Cause** : Matchers `jest-dom` non chargés.

**Solution** :
```typescript
// src/test/setup.ts
import * as matchers from '@testing-library/jest-dom/matchers';
expect.extend(matchers);
```

### Erreur: `window.matchMedia is not a function`

**Cause** : Material-UI nécessite `matchMedia` pour le responsive.

**Solution** :
```typescript
// src/test/setup.ts
Object.defineProperty(window, 'matchMedia', {
  writable: true,
  value: vi.fn().mockImplementation((query) => ({
    matches: false,
    media: query,
    onchange: null,
    addListener: vi.fn(),
    removeListener: vi.fn(),
    addEventListener: vi.fn(),
    removeEventListener: vi.fn(),
    dispatchEvent: vi.fn(),
  })),
});
```

### Erreur: Cannot find module './MyComponent.module.less'

**Cause** : Vitest ne peut pas importer les fichiers LESS.

**Solution** : Mock le module LESS
```typescript
vi.mock('./MyComponent.module.less', () => ({
  default: {
    container: 'container-mock',
  },
}));
```

### Test Timeout

**Cause** : Opération asynchrone trop longue ou `waitFor` qui ne résout jamais.

**Solution** :
```typescript
// Augmenter le timeout
it('should load data', async () => {
  // ...
}, 10000);  // 10 secondes au lieu de 5

// Ou utiliser waitFor avec timeout custom
await waitFor(
  () => {
    expect(screen.getByText('Loaded')).toBeInTheDocument();
  },
  { timeout: 5000 }
);
```

### Act Warning

**Message** :
```
Warning: An update to Component inside a test was not wrapped in act(...)
```

**Cause** : État React mis à jour en dehors d'un `act()`.

**Solution** :
```typescript
// Utiliser userEvent (gère act automatiquement)
const user = userEvent.setup();
await user.click(button);

// Ou wrapper avec act si nécessaire
await act(async () => {
  // Code qui met à jour l'état
});
```

---

## Checklist de Test

### Avant de Commiter

- [ ] Tous les tests passent (`npm test`)
- [ ] Aucune console.error dans les tests
- [ ] Pas de test ignoré (skip/only)
- [ ] Couverture > 80% pour les fichiers modifiés
- [ ] Pas de warnings React (act, etc.)

### Pour un Nouveau Composant

- [ ] Test de rendu de base
- [ ] Test avec props requises
- [ ] Test avec props optionnelles
- [ ] Test des cas limites (null, undefined, empty)
- [ ] Test des interactions utilisateur
- [ ] Test des états de chargement
- [ ] Test des messages d'erreur
- [ ] Test de l'accessibilité (roles, labels)

### Pour une Nouvelle Page

- [ ] Test de chargement des données
- [ ] Test de l'état de loading
- [ ] Test de l'état d'erreur
- [ ] Test du flow complet utilisateur
- [ ] Test des redirections
- [ ] Test de l'authentification requise

---

## Ressources

- [Vitest Documentation](https://vitest.dev/)
- [React Testing Library](https://testing-library.com/react)
- [Testing Library Best Practices](https://kentcdodds.com/blog/common-mistakes-with-react-testing-library)
- [User Event Documentation](https://testing-library.com/docs/user-event/intro)
- [Jest DOM Matchers](https://github.com/testing-library/jest-dom#custom-matchers)

---

**Version** : 2.0.0
**Dernière mise à jour** : Décembre 2024
