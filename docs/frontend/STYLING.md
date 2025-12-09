# Guide de Styling - Nawel App

## Vue d'Ensemble

L'application Nawel utilise une approche hybride pour le styling :

1. **Material-UI (MUI)** : Composants préfabriqués stylés + système de thème
2. **LESS** : Préprocesseur CSS pour styles personnalisés
3. **CSS Modules** : Scoping automatique des styles

Cette combinaison offre :
- Rapidité de développement (MUI)
- Flexibilité pour styles personnalisés (LESS)
- Pas de conflits de noms (CSS Modules)

---

## Structure des Fichiers

```
src/css/
├── App.css                      # Styles globaux de l'app
├── index.css                    # Reset CSS + styles base
├── common.module.less           # Styles communs réutilisables
│
├── pages/                       # Styles des pages
│   ├── Home.module.less
│   ├── MyList.module.less
│   ├── UserList.module.less
│   ├── Cart.module.less
│   ├── Profile.module.less
│   ├── Login.module.less
│   └── Admin.module.less
│
└── components/                  # Styles des composants
    ├── Avatar.module.less
    ├── NavigationBar.module.less
    ├── ChristmasLayout.module.less
    ├── ManagingChildBanner.module.less
    ├── GiftFormDialog.module.less
    ├── GiftListItem.module.less
    ├── ImportDialog.module.less
    ├── UserGiftListItem.module.less
    ├── ReserveDialog.module.less
    ├── ProfileForm.module.less
    ├── AvatarUpload.module.less
    ├── PasswordChangeForm.module.less
    ├── AdminDashboard.module.less
    ├── AdminUsers.module.less
    └── AdminFamilies.module.less
```

**Total** : 23 fichiers LESS

---

## Material-UI Theme

### Configuration (main.tsx)

Le thème MUI est configuré dans le point d'entrée de l'application :

```typescript
import { createTheme, ThemeProvider } from '@mui/material/styles';
import { frFR } from '@mui/material/locale';

const theme = createTheme({
  palette: {
    mode: 'light',
    primary: {
      main: '#2d5f3f',         // Vert forêt (thème Noël)
      light: '#4a8060',
      dark: '#1e4028',
      contrastText: '#ffffff'
    },
    secondary: {
      main: '#b8860b',         // Or/doré (accent)
      light: '#daa520',
      dark: '#8b6508',
      contrastText: '#ffffff'
    },
    success: {
      main: '#2d5f3f',         // Même que primary
    },
    error: {
      main: '#a83b3b',         // Rouge atténué
      light: '#c75c5c',
      dark: '#7d2828'
    },
    warning: {
      main: '#ff9800',
    },
    info: {
      main: '#2196f3',
    },
    background: {
      default: '#f8f6f3',      // Beige/crème
      paper: '#ffffff'
    },
    text: {
      primary: '#2c2c2c',
      secondary: '#5c5c5c'
    }
  },
  typography: {
    fontFamily: '"Roboto", "Helvetica", "Arial", sans-serif',
    h1: {
      fontSize: '2.5rem',
      fontWeight: 600
    },
    h2: {
      fontSize: '2rem',
      fontWeight: 600
    },
    h3: {
      fontSize: '1.75rem',
      fontWeight: 500
    },
    h4: {
      fontSize: '1.5rem',
      fontWeight: 500
    },
    h5: {
      fontSize: '1.25rem',
      fontWeight: 500
    },
    h6: {
      fontSize: '1rem',
      fontWeight: 500
    },
    button: {
      textTransform: 'none',   // Pas de UPPERCASE
      fontWeight: 500
    }
  },
  shape: {
    borderRadius: 8            // Coins arrondis par défaut
  },
  components: {
    MuiButton: {
      styleOverrides: {
        root: {
          borderRadius: 8,
          padding: '8px 16px',
          boxShadow: 'none',
          '&:hover': {
            boxShadow: '0 2px 8px rgba(0, 0, 0, 0.15)'
          }
        },
        contained: {
          backgroundColor: '#2d5f3f',
          '&:hover': {
            backgroundColor: '#1e4028'
          }
        }
      }
    },
    MuiCard: {
      styleOverrides: {
        root: {
          borderRadius: 12,
          boxShadow: '0 2px 8px rgba(0, 0, 0, 0.1)',
          border: '1px solid rgba(0, 0, 0, 0.08)',
          transition: 'all 0.3s ease',
          '&:hover': {
            boxShadow: '0 4px 16px rgba(0, 0, 0, 0.15)'
          }
        }
      }
    },
    MuiAppBar: {
      styleOverrides: {
        root: {
          background: 'linear-gradient(90deg, #2d5f3f 0%, #4a8060 100%)',
          boxShadow: '0 2px 8px rgba(0, 0, 0, 0.15)'
        }
      }
    },
    MuiChip: {
      styleOverrides: {
        root: {
          borderRadius: 6,
          fontWeight: 500
        },
        colorPrimary: {
          backgroundColor: '#2d5f3f',
          color: '#ffffff'
        },
        colorSuccess: {
          backgroundColor: '#4caf50',
          color: '#ffffff'
        },
        colorInfo: {
          backgroundColor: '#2196f3',
          color: '#ffffff'
        }
      }
    },
    MuiTextField: {
      styleOverrides: {
        root: {
          '& .MuiOutlinedInput-root': {
            borderRadius: 8
          }
        }
      }
    }
  }
}, frFR);  // Localisation française
```

### Usage du Thème

```typescript
import { ThemeProvider } from '@mui/material/styles';

<ThemeProvider theme={theme}>
  <App />
</ThemeProvider>
```

---

## Palette de Couleurs

### Couleurs Principales

| Nom | Hex | RGB | Usage |
|-----|-----|-----|-------|
| **Vert Forêt** | `#2d5f3f` | rgb(45, 95, 63) | Primary, boutons, liens |
| **Or/Doré** | `#b8860b` | rgb(184, 134, 11) | Secondary, accents |
| **Beige/Crème** | `#f8f6f3` | rgb(248, 246, 243) | Background |
| **Blanc** | `#ffffff` | rgb(255, 255, 255) | Cards, paper |
| **Rouge Atténué** | `#a83b3b` | rgb(168, 59, 59) | Erreurs, suppression |
| **Texte Noir** | `#2c2c2c` | rgb(44, 44, 44) | Texte principal |
| **Texte Gris** | `#5c5c5c` | rgb(92, 92, 92) | Texte secondaire |

### Couleurs Sémantiques

| Statut | Couleur | Hex | Usage |
|--------|---------|-----|-------|
| **Success** | Vert forêt | `#2d5f3f` | Succès, validé, réservé par moi |
| **Error** | Rouge atténué | `#a83b3b` | Erreurs, actions destructives |
| **Warning** | Orange | `#ff9800` | Avertissements |
| **Info** | Bleu | `#2196f3` | Informations, cadeaux groupés |
| **Default** | Gris | `#9e9e9e` | Neutre, désactivé |

---

## CSS Modules + LESS

### Principe

Chaque composant/page a son propre fichier `.module.less` :

```
MyComponent.tsx       →    MyComponent.module.less
```

**Avantages** :
- Scoping automatique (pas de conflits de noms)
- Co-location (composant + styles au même endroit)
- Type-safe (TypeScript)

### Usage

**Fichier LESS** : `MyList.module.less`
```less
.container {
  padding: 2rem;
  max-width: 1200px;
  margin: 0 auto;
  background-color: @bg-color;  // Variable LESS
}

.giftCard {
  background: white;
  border-radius: 12px;
  padding: 1rem;
  transition: all 0.3s ease;

  &:hover {
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
    transform: translateY(-2px);
  }
}

.giftCard__title {
  font-size: 1.25rem;
  font-weight: 600;
  color: #2c2c2c;
  margin-bottom: 0.5rem;
}

@bg-color: #f8f6f3;
```

**Fichier TypeScript** : `MyList.tsx`
```typescript
import styles from '../css/pages/MyList.module.less';

const MyList: React.FC = () => {
  return (
    <div className={styles.container}>
      <div className={styles.giftCard}>
        <h2 className={styles.giftCard__title}>Cadeau</h2>
      </div>
    </div>
  );
};
```

**Rendu HTML** (classes hashées automatiquement) :
```html
<div class="MyList_container__abc123">
  <div class="MyList_giftCard__def456">
    <h2 class="MyList_giftCard__title__ghi789">Cadeau</h2>
  </div>
</div>
```

---

## Styles Communs (common.module.less)

Styles réutilisables partagés entre plusieurs composants :

```less
// common.module.less

// Layout
.flexCenter {
  display: flex;
  justify-content: center;
  align-items: center;
}

.flexBetween {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.container {
  max-width: 1200px;
  margin: 0 auto;
  padding: 2rem;
}

// Spacing
.mt-1 { margin-top: 0.5rem; }
.mt-2 { margin-top: 1rem; }
.mt-3 { margin-top: 1.5rem; }
.mb-1 { margin-bottom: 0.5rem; }
.mb-2 { margin-bottom: 1rem; }
.mb-3 { margin-bottom: 1.5rem; }

// Text
.textCenter {
  text-align: center;
}

.textBold {
  font-weight: 600;
}

// Cards
.card {
  background: white;
  border-radius: 12px;
  padding: 1.5rem;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}

// Buttons (custom)
.btnPrimary {
  background-color: #2d5f3f;
  color: white;
  border: none;
  border-radius: 8px;
  padding: 0.5rem 1rem;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.3s ease;

  &:hover {
    background-color: #1e4028;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.15);
  }
}
```

**Usage** :
```typescript
import common from '../css/common.module.less';

<div className={common.flexCenter}>
  <button className={common.btnPrimary}>Click me</button>
</div>
```

---

## Styles Globaux

### index.css

Reset CSS + styles de base :

```css
/* Reset */
* {
  margin: 0;
  padding: 0;
  box-sizing: border-box;
}

/* Body */
body {
  font-family: 'Roboto', 'Helvetica', 'Arial', sans-serif;
  background-color: #f8f6f3;
  color: #2c2c2c;
  line-height: 1.6;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
}

/* Links */
a {
  color: #2d5f3f;
  text-decoration: none;
  transition: color 0.3s ease;
}

a:hover {
  color: #1e4028;
  text-decoration: underline;
}

/* Scrollbar (Webkit) */
::-webkit-scrollbar {
  width: 8px;
}

::-webkit-scrollbar-track {
  background: #f1f1f1;
}

::-webkit-scrollbar-thumb {
  background: #2d5f3f;
  border-radius: 4px;
}

::-webkit-scrollbar-thumb:hover {
  background: #1e4028;
}
```

### App.css

Styles spécifiques à l'app (layout global) :

```css
.app {
  min-height: 100vh;
  display: flex;
  flex-direction: column;
}

.app-content {
  flex: 1;
  padding-top: 64px; /* Height of AppBar */
}

/* Animations */
@keyframes fadeIn {
  from {
    opacity: 0;
    transform: translateY(10px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

.fade-in {
  animation: fadeIn 0.3s ease-in;
}
```

---

## Conventions de Nommage

### BEM (Block Element Modifier)

```less
// Block
.giftCard {
  // Styles du block
}

// Element
.giftCard__title {
  // Styles de l'élément
}

.giftCard__description {
  // Styles de l'élément
}

// Modifier
.giftCard--highlighted {
  // Variation du block
}

.giftCard__title--large {
  // Variation de l'élément
}
```

**Exemple d'usage** :
```typescript
<div className={styles.giftCard}>
  <h2 className={styles.giftCard__title}>Titre</h2>
  <p className={styles.giftCard__description}>Description</p>
</div>

<div className={`${styles.giftCard} ${styles['giftCard--highlighted']}`}>
  <h2 className={styles.giftCard__title}>Titre en surbrillance</h2>
</div>
```

---

## Variables LESS

### Définition

```less
// Variables couleurs
@primary-color: #2d5f3f;
@secondary-color: #b8860b;
@bg-color: #f8f6f3;
@text-color: #2c2c2c;
@text-secondary: #5c5c5c;
@error-color: #a83b3b;

// Variables spacing
@spacing-xs: 0.25rem;
@spacing-sm: 0.5rem;
@spacing-md: 1rem;
@spacing-lg: 1.5rem;
@spacing-xl: 2rem;

// Variables border-radius
@radius-sm: 4px;
@radius-md: 8px;
@radius-lg: 12px;
@radius-xl: 16px;

// Variables transitions
@transition-fast: 0.15s ease;
@transition-normal: 0.3s ease;
@transition-slow: 0.5s ease;
```

### Usage

```less
.button {
  background-color: @primary-color;
  color: white;
  padding: @spacing-sm @spacing-md;
  border-radius: @radius-md;
  transition: all @transition-normal;

  &:hover {
    background-color: darken(@primary-color, 10%);
  }
}
```

---

## Mixins LESS

### Définition

```less
// Mixin flexbox center
.flex-center() {
  display: flex;
  justify-content: center;
  align-items: center;
}

// Mixin truncate text
.truncate() {
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

// Mixin card shadow
.card-shadow() {
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  transition: box-shadow @transition-normal;

  &:hover {
    box-shadow: 0 4px 16px rgba(0, 0, 0, 0.15);
  }
}
```

### Usage

```less
.avatarContainer {
  .flex-center();
  width: 50px;
  height: 50px;
}

.giftTitle {
  .truncate();
  max-width: 300px;
}

.card {
  .card-shadow();
  background: white;
  border-radius: @radius-lg;
}
```

---

## Responsive Design

### Breakpoints

```less
// Variables breakpoints
@screen-xs: 480px;
@screen-sm: 768px;
@screen-md: 1024px;
@screen-lg: 1280px;
@screen-xl: 1920px;

// Mixins media queries
.mobile-only() {
  @media (max-width: @screen-sm - 1) {
    @content();
  }
}

.tablet-up() {
  @media (min-width: @screen-sm) {
    @content();
  }
}

.desktop-up() {
  @media (min-width: @screen-md) {
    @content();
  }
}
```

### Usage

**Mobile-first approach** :

```less
.container {
  padding: 1rem;          // Mobile par défaut

  @media (min-width: 768px) {  // Tablet
    padding: 2rem;
  }

  @media (min-width: 1024px) {  // Desktop
    padding: 3rem;
    max-width: 1200px;
    margin: 0 auto;
  }
}

.giftGrid {
  display: grid;
  grid-template-columns: 1fr;    // 1 colonne mobile
  gap: 1rem;

  @media (min-width: 768px) {
    grid-template-columns: repeat(2, 1fr);  // 2 colonnes tablet
  }

  @media (min-width: 1024px) {
    grid-template-columns: repeat(3, 1fr);  // 3 colonnes desktop
  }
}
```

### Material-UI Responsive

MUI fournit un système de breakpoints intégré :

```typescript
import { useMediaQuery, useTheme } from '@mui/material';

const MyComponent = () => {
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down('md'));

  return (
    <Box
      sx={{
        display: { xs: 'block', md: 'flex' },  // block mobile, flex desktop
        padding: { xs: 2, md: 4 },             // padding responsive
        fontSize: { xs: '1rem', md: '1.25rem' }
      }}
    >
      {isMobile ? <MobileMenu /> : <DesktopMenu />}
    </Box>
  );
};
```

---

## Animations

### Transitions CSS

```less
.button {
  background-color: @primary-color;
  transition: all 0.3s ease;

  &:hover {
    background-color: darken(@primary-color, 10%);
    transform: translateY(-2px);
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
  }

  &:active {
    transform: translateY(0);
  }
}
```

### Keyframes

```less
@keyframes slideIn {
  from {
    opacity: 0;
    transform: translateX(-20px);
  }
  to {
    opacity: 1;
    transform: translateX(0);
  }
}

@keyframes pulse {
  0% {
    transform: scale(1);
  }
  50% {
    transform: scale(1.05);
  }
  100% {
    transform: scale(1);
  }
}

.notification {
  animation: slideIn 0.3s ease;
}

.avatar--active {
  animation: pulse 2s infinite;
}
```

---

## Bonnes Pratiques

### 1. Co-location

Garder les styles proches des composants :

```
src/components/Avatar/
├── Avatar.tsx
├── Avatar.test.tsx
└── Avatar.module.less
```

### 2. Éviter les !important

```less
// ❌ Bad
.button {
  color: red !important;
}

// ✅ Good
.button {
  color: red;
}

// Si vraiment nécessaire, augmenter la spécificité
.container .button {
  color: red;
}
```

### 3. Utiliser des Variables

```less
// ❌ Bad : Valeurs en dur
.button {
  background-color: #2d5f3f;
  padding: 8px 16px;
  border-radius: 8px;
}

// ✅ Good : Variables
.button {
  background-color: @primary-color;
  padding: @spacing-sm @spacing-md;
  border-radius: @radius-md;
}
```

### 4. Mobile-First

```less
// ❌ Bad : Desktop-first
.container {
  padding: 3rem;

  @media (max-width: 768px) {
    padding: 1rem;
  }
}

// ✅ Good : Mobile-first
.container {
  padding: 1rem;

  @media (min-width: 768px) {
    padding: 3rem;
  }
}
```

### 5. Combiner MUI et LESS

```typescript
// Utiliser sx pour overrides ponctuels
<Button
  variant="contained"
  sx={{
    backgroundColor: '#2d5f3f',
    '&:hover': {
      backgroundColor: '#1e4028'
    }
  }}
>
  Click
</Button>

// Utiliser LESS modules pour styles complexes
<div className={styles.complexLayout}>
  <Button variant="contained">Click</Button>
</div>
```

---

## Outils de Développement

### Browser DevTools

- **Inspect Element** : Voir les styles appliqués
- **Computed Tab** : Voir les valeurs finales
- **Sources** : Debugger LESS (avec sourcemaps)

### Extensions Chrome

- **React DevTools** : Inspecter composants React
- **Redux DevTools** : Inspecter state (si Redux utilisé)
- **CSS Peeper** : Extraire styles de sites

### Vite Configuration

```typescript
// vite.config.ts
export default defineConfig({
  css: {
    modules: {
      localsConvention: 'camelCase',  // myClass au lieu de my-class
      generateScopedName: '[name]_[local]__[hash:base64:5]'
    },
    preprocessorOptions: {
      less: {
        javascriptEnabled: true,
        modifyVars: {
          '@primary-color': '#2d5f3f'
        }
      }
    }
  }
});
```

---

## Exemples Complets

### Exemple 1 : Gift Card

**LESS** : `GiftListItem.module.less`
```less
@primary-color: #2d5f3f;
@spacing-md: 1rem;
@radius-lg: 12px;

.card {
  background: white;
  border-radius: @radius-lg;
  padding: @spacing-md;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  transition: all 0.3s ease;

  &:hover {
    box-shadow: 0 4px 16px rgba(0, 0, 0, 0.15);
    transform: translateY(-2px);
  }
}

.card__image {
  width: 100%;
  height: 200px;
  object-fit: cover;
  border-radius: 8px 8px 0 0;
}

.card__title {
  font-size: 1.25rem;
  font-weight: 600;
  color: #2c2c2c;
  margin: 0.5rem 0;
}

.card__price {
  font-size: 1.5rem;
  font-weight: 700;
  color: @primary-color;
  margin-top: 0.5rem;
}

.card__actions {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-top: 1rem;
}
```

**TSX** : `GiftListItem.tsx`
```typescript
import styles from './GiftListItem.module.less';

const GiftListItem: React.FC<Props> = ({ gift }) => (
  <div className={styles.card}>
    <img src={gift.image} className={styles.card__image} alt={gift.name} />
    <h3 className={styles.card__title}>{gift.name}</h3>
    <p className={styles.card__price}>{gift.cost}€</p>
    <div className={styles.card__actions}>
      <Button onClick={onEdit}>Modifier</Button>
      <IconButton onClick={onDelete}>
        <DeleteIcon />
      </IconButton>
    </div>
  </div>
);
```

---

### Exemple 2 : Responsive Grid

**LESS** :
```less
.giftsGrid {
  display: grid;
  gap: 1.5rem;
  padding: 2rem;

  // Mobile : 1 colonne
  grid-template-columns: 1fr;

  // Tablet : 2 colonnes
  @media (min-width: 768px) {
    grid-template-columns: repeat(2, 1fr);
  }

  // Desktop : 3 colonnes
  @media (min-width: 1024px) {
    grid-template-columns: repeat(3, 1fr);
    max-width: 1200px;
    margin: 0 auto;
  }

  // Large Desktop : 4 colonnes
  @media (min-width: 1440px) {
    grid-template-columns: repeat(4, 1fr);
  }
}
```

---

## Récapitulatif

| Approche | Usage | Fichiers |
|----------|-------|----------|
| **Material-UI** | Composants préfabriqués, thème global | main.tsx |
| **LESS Modules** | Styles personnalisés scoped | *.module.less |
| **CSS Global** | Reset, base styles | index.css, App.css |
| **Variables LESS** | Couleurs, spacing, réutilisables | common.module.less |
| **Mixins LESS** | Patterns réutilisables | common.module.less |

---

## Références

- [Material-UI Documentation](https://mui.com/material-ui/)
- [Material-UI Theming](https://mui.com/material-ui/customization/theming/)
- [LESS Documentation](https://lesscss.org/)
- [CSS Modules](https://github.com/css-modules/css-modules)
- [BEM Methodology](http://getbem.com/)
