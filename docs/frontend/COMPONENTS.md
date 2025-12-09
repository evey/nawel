# Documentation des Composants - Nawel App

## Vue d'Ensemble

L'application Nawel contient **18 composants réutilisables**, organisés en 5 catégories :

| Catégorie | Nombre | Description |
|-----------|--------|-------------|
| **Layout** | 5 | Structure, navigation, protection routes |
| **Gifts** | 3 | Gestion et affichage des cadeaux |
| **UserList** | 2 | Consultation listes et réservations |
| **Profile** | 3 | Gestion du profil utilisateur |
| **Admin** | 3 | Administration (stats, users, families) |

**Total** : 18 composants + 2 wrappers (ChristmasLayout, ProtectedRoute)

---

## Composants Layout

### 1. NavigationBar

**Fichier** : `src/components/NavigationBar.tsx`

**Description** : Barre de navigation principale affichée en haut de toutes les pages (sauf Login).

**Fonctionnalités** :
- Logo et titre de l'application
- Navigation responsive avec menu mobile (burger icon)
- Liens vers pages principales (Home, Ma liste, Panier, Profil)
- Lien Admin (si isAdmin = true)
- Bouton de déconnexion
- Affichage de l'utilisateur connecté avec avatar

**Props** : Aucune (utilise AuthContext)

**State** :
```typescript
const [mobileMenuOpen, setMobileMenuOpen] = useState(false);
```

**Composants MUI utilisés** :
- `AppBar`, `Toolbar`, `IconButton`, `Menu`, `MenuItem`, `Avatar`, `Button`

**Usage** :
```tsx
import NavigationBar from './components/NavigationBar';

<NavigationBar />
```

**Features** :
- **Desktop** : Liens horizontaux dans la barre
- **Mobile** : Menu burger avec drawer latéral
- **Badge Admin** : Lien "Admin" uniquement visible si `user.isAdmin`
- **Avatar** : Cliquable → Redirect vers `/profile`

**Exemple de Code** :
```tsx
const NavigationBar: React.FC = () => {
  const { user, logout, isAuthenticated } = useAuth();
  const navigate = useNavigate();
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false);

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  return (
    <AppBar position="static">
      <Toolbar>
        <Typography variant="h6">Nawel</Typography>

        {/* Desktop Menu */}
        <Box sx={{ display: { xs: 'none', md: 'flex' } }}>
          <Button color="inherit" onClick={() => navigate('/')}>
            Accueil
          </Button>
          <Button color="inherit" onClick={() => navigate('/my-list')}>
            Ma liste
          </Button>
          {user?.isAdmin && (
            <Button color="inherit" onClick={() => navigate('/admin')}>
              Admin
            </Button>
          )}
        </Box>

        {/* Mobile Menu */}
        <IconButton onClick={() => setMobileMenuOpen(true)}>
          <MenuIcon />
        </IconButton>

        {/* Avatar */}
        <Avatar src={user?.avatar} onClick={() => navigate('/profile')} />

        {/* Logout */}
        <Button color="inherit" onClick={handleLogout}>
          Déconnexion
        </Button>
      </Toolbar>
    </AppBar>
  );
};
```

---

### 2. Avatar

**Fichier** : `src/components/Avatar.tsx`

**Description** : Composant d'affichage d'avatar utilisateur avec fallback initiales.

**Props** :
```typescript
interface AvatarProps {
  user: User | null;
  size?: 'small' | 'medium' | 'large';
  onClick?: () => void;
}
```

**Fonctionnalités** :
- Affichage de l'image avatar si disponible
- Fallback initiales (FirstName + LastName) si pas d'avatar
- Gestion erreur chargement image → Fallback initiales
- 3 tailles : small (32px), medium (40px), large (56px)
- Cliquable (optionnel)

**Usage** :
```tsx
<Avatar user={user} size="medium" onClick={() => navigate('/profile')} />
```

**Exemple de Code** :
```tsx
const Avatar: React.FC<AvatarProps> = ({ user, size = 'medium', onClick }) => {
  const [imageError, setImageError] = useState(false);

  const getInitials = (user: User): string => {
    const first = user.firstName?.[0] || '';
    const last = user.lastName?.[0] || '';
    return (first + last).toUpperCase() || user.login[0].toUpperCase();
  };

  const sizes = {
    small: 32,
    medium: 40,
    large: 56
  };

  if (!imageError && user?.avatar) {
    return (
      <MuiAvatar
        src={`${API_URL}/${user.avatar}`}
        sx={{ width: sizes[size], height: sizes[size], cursor: onClick ? 'pointer' : 'default' }}
        onClick={onClick}
        onError={() => setImageError(true)}
      />
    );
  }

  return (
    <MuiAvatar
      sx={{ width: sizes[size], height: sizes[size], cursor: onClick ? 'pointer' : 'default' }}
      onClick={onClick}
    >
      {user ? getInitials(user) : '?'}
    </MuiAvatar>
  );
};
```

**Tests** : ✅ Complets (Avatar.test.tsx)

---

### 3. ChristmasLayout

**Fichier** : `src/components/ChristmasLayout.tsx`

**Description** : Wrapper fournissant le layout avec thème de Noël (background, padding).

**Props** :
```typescript
interface ChristmasLayoutProps {
  children: React.ReactNode;
}
```

**Fonctionnalités** :
- Background beige/crème
- Padding global
- Min-height 100vh

**Usage** :
```tsx
<ChristmasLayout>
  <NavigationBar />
  <Routes>...</Routes>
</ChristmasLayout>
```

**Exemple de Code** :
```tsx
const ChristmasLayout: React.FC<ChristmasLayoutProps> = ({ children }) => {
  return (
    <Box
      sx={{
        minHeight: '100vh',
        backgroundColor: '#f8f6f3',
        paddingBottom: 4
      }}
    >
      {children}
    </Box>
  );
};
```

---

### 4. ManagingChildBanner

**Fichier** : `src/components/ManagingChildBanner.tsx`

**Description** : Banner d'alerte affiché quand un parent gère la liste d'un enfant.

**Props** : Aucune (utilise AuthContext)

**Fonctionnalités** :
- Visible uniquement si `managingChild` existe dans AuthContext
- Affiche le nom de l'enfant géré
- Bouton "Revenir à mon compte" → `stopManagingChild()`
- Style : Alert MUI warning

**Usage** :
```tsx
import ManagingChildBanner from './components/ManagingChildBanner';

{managingChild && <ManagingChildBanner />}
```

**Exemple de Code** :
```tsx
const ManagingChildBanner: React.FC = () => {
  const { managingChild, stopManagingChild } = useAuth();
  const navigate = useNavigate();

  if (!managingChild) return null;

  const handleStopManaging = () => {
    stopManagingChild();
    navigate('/my-list');
  };

  return (
    <Alert
      severity="warning"
      action={
        <Button color="inherit" size="small" onClick={handleStopManaging}>
          Revenir à mon compte
        </Button>
      }
    >
      Vous gérez actuellement la liste de <strong>{managingChild.userName}</strong>
    </Alert>
  );
};
```

---

### 5. ProtectedRoute

**Fichier** : `src/components/ProtectedRoute.tsx`

**Description** : HOC (Higher-Order Component) pour protéger les routes nécessitant une authentification.

**Props** :
```typescript
interface ProtectedRouteProps {
  children: React.ReactNode;
}
```

**Fonctionnalités** :
- Vérifie `isAuthenticated` depuis AuthContext
- Affiche loader pendant la vérification du token
- Redirect vers `/login` si non authentifié
- Affiche les children si authentifié

**Usage** :
```tsx
<Route path="/my-list" element={
  <ProtectedRoute>
    <MyList />
  </ProtectedRoute>
} />
```

**Exemple de Code** :
```tsx
const ProtectedRoute: React.FC<ProtectedRouteProps> = ({ children }) => {
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

**Tests** : Non testés

---

## Composants Gifts

### 6. GiftFormDialog

**Fichier** : `src/components/gifts/GiftFormDialog.tsx`

**Description** : Modal de création/édition d'un cadeau avec extraction automatique depuis URL.

**Props** :
```typescript
interface GiftFormDialogProps {
  open: boolean;
  gift?: Gift;  // Si édition, gift pré-rempli
  onClose: () => void;
  onSave: (giftData: CreateGiftData | UpdateGiftData) => Promise<void>;
}
```

**Fonctionnalités** :
- Formulaire complet pour créer/éditer un cadeau
- Champs : Nom, Description, Lien, Prix, Devise, Image, Cadeau groupé
- **Extraction automatique** : Si URL entrée → Click "Extraire" → Appel API OpenGraph → Pré-remplissage
- Validation : Nom requis
- Boutons : Annuler, Sauvegarder
- Loading state pendant extraction

**State** :
```typescript
const [formData, setFormData] = useState<GiftFormData>({
  name: '',
  description: '',
  link: '',
  cost: null,
  currency: 'EUR',
  image: '',
  isGroupGift: false
});
const [extracting, setExtracting] = useState(false);
const [extractError, setExtractError] = useState<string | null>(null);
```

**Usage** :
```tsx
<GiftFormDialog
  open={dialogOpen}
  gift={editingGift}
  onClose={() => setDialogOpen(false)}
  onSave={handleSaveGift}
/>
```

**Exemple Extraction** :
```tsx
const handleExtract = async () => {
  if (!formData.link) return;

  setExtracting(true);
  setExtractError(null);

  try {
    const productInfo = await productsAPI.extractInfo(formData.link);

    setFormData(prev => ({
      ...prev,
      name: productInfo.name || prev.name,
      description: productInfo.description || prev.description,
      cost: productInfo.price || prev.cost,
      currency: productInfo.currency || prev.currency,
      image: productInfo.imageUrl || prev.image
    }));
  } catch (error) {
    setExtractError('Impossible d\'extraire les informations');
  } finally {
    setExtracting(false);
  }
};
```

**Optimisation** : `React.memo()` pour éviter re-renders inutiles

---

### 7. GiftListItem

**Fichier** : `src/components/gifts/GiftListItem.tsx`

**Description** : Item de liste pour afficher un cadeau dans "Ma liste" avec actions d'édition/suppression.

**Props** :
```typescript
interface GiftListItemProps {
  gift: Gift;
  onEdit: (gift: Gift) => void;
  onDelete: (giftId: number) => void;
}
```

**Fonctionnalités** :
- Affichage card avec image, nom, description, prix
- Badge "Cadeau groupé" si `isGroupGift`
- Boutons d'action :
  - Edit (icon button)
  - Delete (icon button avec confirmation)
- Lien cliquable vers URL produit (si existe)

**Usage** :
```tsx
<GiftListItem
  gift={gift}
  onEdit={handleEdit}
  onDelete={handleDelete}
/>
```

**Exemple de Code** :
```tsx
const GiftListItem: React.FC<GiftListItemProps> = ({ gift, onEdit, onDelete }) => {
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);

  const handleDeleteConfirm = () => {
    onDelete(gift.id);
    setDeleteDialogOpen(false);
  };

  return (
    <>
      <Card>
        <CardMedia
          component="img"
          height="140"
          image={gift.image || '/placeholder.png'}
          alt={gift.name}
        />
        <CardContent>
          <Typography variant="h6">{gift.name}</Typography>
          {gift.isGroupGift && (
            <Chip label="Cadeau groupé" color="info" size="small" />
          )}
          <Typography variant="body2">{gift.description}</Typography>
          {gift.cost && (
            <Typography variant="h6" color="primary">
              {gift.cost} {gift.currency}
            </Typography>
          )}
        </CardContent>
        <CardActions>
          <IconButton onClick={() => onEdit(gift)}>
            <EditIcon />
          </IconButton>
          <IconButton onClick={() => setDeleteDialogOpen(true)}>
            <DeleteIcon />
          </IconButton>
        </CardActions>
      </Card>

      <Dialog open={deleteDialogOpen} onClose={() => setDeleteDialogOpen(false)}>
        <DialogTitle>Confirmer la suppression</DialogTitle>
        <DialogContent>
          <DialogContentText>
            Êtes-vous sûr de vouloir supprimer "{gift.name}" ?
          </DialogContentText>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDeleteDialogOpen(false)}>Annuler</Button>
          <Button onClick={handleDeleteConfirm} color="error">Supprimer</Button>
        </DialogActions>
      </Dialog>
    </>
  );
};
```

---

### 8. ImportDialog

**Fichier** : `src/components/gifts/ImportDialog.tsx`

**Description** : Modal pour importer les cadeaux non réservés d'une année précédente.

**Props** :
```typescript
interface ImportDialogProps {
  open: boolean;
  availableYears: number[];
  currentYear: number;
  onClose: () => void;
  onImport: (fromYear: number) => Promise<void>;
}
```

**Fonctionnalités** :
- Sélecteur d'année (années disponibles sauf l'année courante)
- Bouton "Importer"
- Affichage du nombre de cadeaux importés
- Loading state pendant l'import

**State** :
```typescript
const [selectedYear, setSelectedYear] = useState<number | null>(null);
const [importing, setImporting] = useState(false);
```

**Usage** :
```tsx
<ImportDialog
  open={importDialogOpen}
  availableYears={years}
  currentYear={2025}
  onClose={() => setImportDialogOpen(false)}
  onImport={handleImport}
/>
```

**Exemple de Code** :
```tsx
const ImportDialog: React.FC<ImportDialogProps> = ({
  open,
  availableYears,
  currentYear,
  onClose,
  onImport
}) => {
  const [selectedYear, setSelectedYear] = useState<number | null>(null);
  const [importing, setImporting] = useState(false);

  const handleImport = async () => {
    if (!selectedYear) return;

    setImporting(true);
    try {
      await onImport(selectedYear);
      onClose();
    } catch (error) {
      console.error('Erreur import:', error);
    } finally {
      setImporting(false);
    }
  };

  const yearsToImport = availableYears.filter(y => y !== currentYear);

  return (
    <Dialog open={open} onClose={onClose}>
      <DialogTitle>Importer des cadeaux</DialogTitle>
      <DialogContent>
        <Select
          fullWidth
          value={selectedYear || ''}
          onChange={(e) => setSelectedYear(Number(e.target.value))}
        >
          {yearsToImport.map(year => (
            <MenuItem key={year} value={year}>{year}</MenuItem>
          ))}
        </Select>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>Annuler</Button>
        <Button
          onClick={handleImport}
          disabled={!selectedYear || importing}
          variant="contained"
        >
          {importing ? <CircularProgress size={20} /> : 'Importer'}
        </Button>
      </DialogActions>
    </Dialog>
  );
};
```

---

## Composants UserList

### 9. UserGiftListItem

**Fichier** : `src/components/userlist/UserGiftListItem.tsx`

**Description** : Item de liste pour afficher un cadeau d'un autre utilisateur avec actions de réservation.

**Props** :
```typescript
interface UserGiftListItemProps {
  gift: Gift;
  currentUserId: number;
  onReserve: (gift: Gift) => void;
  onUnreserve: (giftId: number) => void;
}
```

**Fonctionnalités** :
- Affichage card avec image, nom, description, prix
- Chip de statut :
  - "Disponible" (primary)
  - "Réservé par vous" (success)
  - "Déjà réservé" (default)
  - "Cadeau groupé" (info)
  - "Vous participez" (success)
- Boutons d'action conditionnels :
  - "Réserver" : Si disponible et non groupé
  - "Participer" : Si cadeau groupé et pas encore participant
  - "Annuler" : Si réservé/participant par moi
- Affichage participants (si cadeau groupé)

**Usage** :
```tsx
<UserGiftListItem
  gift={gift}
  currentUserId={user.id}
  onReserve={handleReserve}
  onUnreserve={handleUnreserve}
/>
```

**Logique Statut** :
```tsx
const getStatus = (gift: Gift, userId: number) => {
  if (gift.isGroupGift) {
    const isParticipating = gift.participants?.some(p => p.userId === userId);
    return {
      label: isParticipating ? 'Vous participez' : 'Cadeau groupé',
      color: isParticipating ? 'success' : 'info',
      canReserve: !isParticipating,
      canUnreserve: isParticipating
    };
  }

  if (!gift.available) {
    return {
      label: gift.takenBy === userId ? 'Réservé par vous' : 'Déjà réservé',
      color: gift.takenBy === userId ? 'success' : 'default',
      canReserve: false,
      canUnreserve: gift.takenBy === userId
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

### 10. ReserveDialog

**Fichier** : `src/components/userlist/ReserveDialog.tsx`

**Description** : Modal de confirmation pour réserver un cadeau avec champ commentaire optionnel.

**Props** :
```typescript
interface ReserveDialogProps {
  open: boolean;
  gift: Gift | null;
  onClose: () => void;
  onConfirm: (giftId: number, comment: string) => Promise<void>;
}
```

**Fonctionnalités** :
- Affichage du nom du cadeau à réserver
- TextField pour commentaire (optionnel)
- Boutons : Annuler, Réserver
- Loading state pendant la réservation

**State** :
```typescript
const [comment, setComment] = useState('');
const [loading, setLoading] = useState(false);
```

**Usage** :
```tsx
<ReserveDialog
  open={dialogOpen}
  gift={selectedGift}
  onClose={() => setDialogOpen(false)}
  onConfirm={handleConfirmReserve}
/>
```

**Exemple de Code** :
```tsx
const ReserveDialog: React.FC<ReserveDialogProps> = ({
  open,
  gift,
  onClose,
  onConfirm
}) => {
  const [comment, setComment] = useState('');
  const [loading, setLoading] = useState(false);

  const handleConfirm = async () => {
    if (!gift) return;

    setLoading(true);
    try {
      await onConfirm(gift.id, comment);
      setComment('');
      onClose();
    } catch (error) {
      console.error('Erreur réservation:', error);
    } finally {
      setLoading(false);
    }
  };

  return (
    <Dialog open={open} onClose={onClose}>
      <DialogTitle>Réserver "{gift?.name}"</DialogTitle>
      <DialogContent>
        <TextField
          fullWidth
          multiline
          rows={3}
          label="Commentaire (optionnel)"
          value={comment}
          onChange={(e) => setComment(e.target.value)}
          placeholder="Ex: Avec plaisir !"
        />
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>Annuler</Button>
        <Button
          onClick={handleConfirm}
          disabled={loading}
          variant="contained"
        >
          {loading ? <CircularProgress size={20} /> : 'Réserver'}
        </Button>
      </DialogActions>
    </Dialog>
  );
};
```

---

## Composants Profile

### 11. ProfileForm

**Fichier** : `src/components/profile/ProfileForm.tsx`

**Description** : Formulaire de modification des informations personnelles et préférences.

**Props** :
```typescript
interface ProfileFormProps {
  initialData: UpdateProfileData;
  onSave: (data: UpdateProfileData) => Promise<void>;
}
```

**Fonctionnalités** :
- Champs : FirstName, LastName, Email, Pseudo
- Checkboxes :
  - `notifyListEdit` : Notification si liste modifiée
  - `notifyGiftTaken` : Notification si cadeau réservé
  - `displayPopup` : Afficher popups info
- Bouton "Sauvegarder"
- Messages succès/erreur

**State** :
```typescript
const [formData, setFormData] = useState<UpdateProfileData>(initialData);
const [saving, setSaving] = useState(false);
const [success, setSuccess] = useState(false);
```

**Usage** :
```tsx
<ProfileForm
  initialData={profile}
  onSave={handleSave}
/>
```

**Exemple de Code** :
```tsx
const ProfileForm = React.memo<ProfileFormProps>(({ initialData, onSave }) => {
  const [formData, setFormData] = useState(initialData);
  const [saving, setSaving] = useState(false);
  const [success, setSuccess] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setSaving(true);
    setSuccess(false);

    try {
      await onSave(formData);
      setSuccess(true);
      setTimeout(() => setSuccess(false), 3000);
    } catch (error) {
      console.error('Erreur sauvegarde:', error);
    } finally {
      setSaving(false);
    }
  };

  return (
    <form onSubmit={handleSubmit}>
      <TextField
        fullWidth
        label="Prénom"
        value={formData.firstName || ''}
        onChange={(e) => setFormData({ ...formData, firstName: e.target.value })}
      />
      <TextField
        fullWidth
        label="Nom"
        value={formData.lastName || ''}
        onChange={(e) => setFormData({ ...formData, lastName: e.target.value })}
      />
      <TextField
        fullWidth
        label="Email"
        type="email"
        value={formData.email || ''}
        onChange={(e) => setFormData({ ...formData, email: e.target.value })}
      />

      <FormControlLabel
        control={
          <Checkbox
            checked={formData.notifyListEdit}
            onChange={(e) => setFormData({ ...formData, notifyListEdit: e.target.checked })}
          />
        }
        label="Notification si liste modifiée"
      />

      {success && <Alert severity="success">Profil mis à jour !</Alert>}

      <Button type="submit" variant="contained" disabled={saving}>
        {saving ? <CircularProgress size={20} /> : 'Sauvegarder'}
      </Button>
    </form>
  );
});
```

**Optimisation** : `React.memo()` pour éviter re-renders

---

### 12. AvatarUpload

**Fichier** : `src/components/profile/AvatarUpload.tsx`

**Description** : Composant d'upload et gestion d'avatar avec preview.

**Props** :
```typescript
interface AvatarUploadProps {
  currentAvatar: string | null;
  onUpload: (file: File) => Promise<void>;
  onDelete: () => Promise<void>;
}
```

**Fonctionnalités** :
- Affichage de l'avatar actuel (ou placeholder)
- Input file caché, trigger par bouton
- Preview de l'image avant upload
- Validation : Format (JPEG, PNG, GIF, WebP), Taille max (5MB)
- Bouton "Changer l'avatar"
- Bouton "Supprimer l'avatar" (si avatar existe)
- Messages d'erreur

**State** :
```typescript
const [preview, setPreview] = useState<string | null>(null);
const [uploading, setUploading] = useState(false);
const [error, setError] = useState<string | null>(null);
```

**Usage** :
```tsx
<AvatarUpload
  currentAvatar={user.avatar}
  onUpload={handleUpload}
  onDelete={handleDelete}
/>
```

**Validation** :
```tsx
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

**Exemple Upload** :
```tsx
const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
  const file = e.target.files?.[0];
  if (!file) return;

  const validationError = validateFile(file);
  if (validationError) {
    setError(validationError);
    return;
  }

  // Preview
  const reader = new FileReader();
  reader.onload = (e) => {
    setPreview(e.target?.result as string);
  };
  reader.readAsDataURL(file);

  // Upload
  handleUpload(file);
};

const handleUpload = async (file: File) => {
  setUploading(true);
  setError(null);

  try {
    await onUpload(file);
    setPreview(null);
  } catch (error) {
    setError('Erreur lors de l\'upload');
  } finally {
    setUploading(false);
  }
};
```

**Optimisation** : `React.memo()` pour éviter re-renders

---

### 13. PasswordChangeForm

**Fichier** : `src/components/profile/PasswordChangeForm.tsx`

**Description** : Modal de changement de mot de passe.

**Props** :
```typescript
interface PasswordChangeFormProps {
  open: boolean;
  onClose: () => void;
  onSubmit: (data: ChangePasswordData) => Promise<void>;
}
```

**Fonctionnalités** :
- Champs : Ancien mot de passe, Nouveau, Confirmation
- Validation :
  - Nouveau mot de passe min 6 caractères
  - Confirmation doit correspondre
- Toggle visibility mot de passe (eye icon)
- Boutons : Annuler, Changer

**State** :
```typescript
const [formData, setFormData] = useState({
  oldPassword: '',
  newPassword: '',
  confirmPassword: ''
});
const [showPassword, setShowPassword] = useState(false);
const [loading, setLoading] = useState(false);
const [error, setError] = useState<string | null>(null);
```

**Usage** :
```tsx
<PasswordChangeForm
  open={dialogOpen}
  onClose={() => setDialogOpen(false)}
  onSubmit={handleChangePassword}
/>
```

**Validation** :
```tsx
const validate = (): boolean => {
  if (formData.newPassword.length < 6) {
    setError('Le nouveau mot de passe doit contenir au moins 6 caractères');
    return false;
  }

  if (formData.newPassword !== formData.confirmPassword) {
    setError('Les mots de passe ne correspondent pas');
    return false;
  }

  return true;
};
```

**Optimisation** : `React.memo()` pour éviter re-renders

---

## Composants Admin

### 14. AdminDashboard

**Fichier** : `src/components/admin/AdminDashboard.tsx`

**Description** : Onglet Dashboard avec statistiques et graphiques.

**Props** :
```typescript
interface AdminDashboardProps {
  stats: AdminStats | null;
  loading: boolean;
}
```

**Fonctionnalités** :
- Cards avec métriques :
  - Total requêtes OpenGraph
  - Taux de succès (%)
  - Utilisateurs actifs
- Graphique linéaire (Recharts) :
  - Requêtes par mois (12 derniers mois)
  - Ligne "Total" et "Succès"

**Types** :
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
```tsx
<AdminDashboard stats={stats} loading={loading} />
```

**Graphique** :
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

### 15. AdminUsers

**Fichier** : `src/components/admin/AdminUsers.tsx`

**Description** : Onglet de gestion des utilisateurs avec table et actions CRUD.

**Props** :
```typescript
interface AdminUsersProps {
  users: User[];
  families: Family[];
  loading: boolean;
  onCreateUser: (data: CreateUserData) => Promise<void>;
  onUpdateUser: (id: number, data: UpdateUserData) => Promise<void>;
  onDeleteUser: (id: number) => Promise<void>;
}
```

**Fonctionnalités** :
- Table avec colonnes :
  - Avatar
  - Login
  - Nom complet
  - Email
  - Famille
  - Rôle (User/Admin)
  - Actions (Edit, Delete)
- Bouton "Créer un utilisateur"
- Dialog de création/édition
- Confirmation avant suppression

**State** :
```typescript
const [dialogOpen, setDialogOpen] = useState(false);
const [editingUser, setEditingUser] = useState<User | null>(null);
const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);
const [userToDelete, setUserToDelete] = useState<number | null>(null);
```

**Usage** :
```tsx
<AdminUsers
  users={users}
  families={families}
  loading={loading}
  onCreateUser={handleCreate}
  onUpdateUser={handleUpdate}
  onDeleteUser={handleDelete}
/>
```

**Formulaire Utilisateur** :
```tsx
<Dialog open={dialogOpen} onClose={() => setDialogOpen(false)}>
  <DialogTitle>{editingUser ? 'Modifier' : 'Créer'} un utilisateur</DialogTitle>
  <DialogContent>
    <TextField label="Login" required />
    <TextField label="Password" type="password" required={!editingUser} />
    <TextField label="Email" type="email" />
    <TextField label="Prénom" />
    <TextField label="Nom" />
    <Select label="Famille" required>
      {families.map(f => <MenuItem value={f.id}>{f.name}</MenuItem>)}
    </Select>
    <FormControlLabel
      control={<Checkbox />}
      label="Administrateur"
    />
    <FormControlLabel
      control={<Checkbox />}
      label="Compte enfant"
    />
  </DialogContent>
  <DialogActions>
    <Button onClick={() => setDialogOpen(false)}>Annuler</Button>
    <Button variant="contained" onClick={handleSubmit}>
      {editingUser ? 'Modifier' : 'Créer'}
    </Button>
  </DialogActions>
</Dialog>
```

---

### 16. AdminFamilies

**Fichier** : `src/components/admin/AdminFamilies.tsx`

**Description** : Onglet de gestion des familles avec table et actions CRUD.

**Props** :
```typescript
interface AdminFamiliesProps {
  families: Family[];
  loading: boolean;
  onCreateFamily: (data: CreateFamilyData) => Promise<void>;
  onUpdateFamily: (id: number, data: UpdateFamilyData) => Promise<void>;
  onDeleteFamily: (id: number) => Promise<void>;
}
```

**Fonctionnalités** :
- Table avec colonnes :
  - Nom de la famille
  - Nombre de membres
  - Actions (Edit, Delete)
- Bouton "Créer une famille"
- Dialog de création/édition (simple : nom uniquement)
- Confirmation avant suppression
- Warning si famille contient des utilisateurs

**State** :
```typescript
const [dialogOpen, setDialogOpen] = useState(false);
const [editingFamily, setEditingFamily] = useState<Family | null>(null);
const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);
const [familyToDelete, setFamilyToDelete] = useState<Family | null>(null);
```

**Usage** :
```tsx
<AdminFamilies
  families={families}
  loading={loading}
  onCreateFamily={handleCreate}
  onUpdateFamily={handleUpdate}
  onDeleteFamily={handleDelete}
/>
```

**Formulaire Famille** :
```tsx
<Dialog open={dialogOpen} onClose={() => setDialogOpen(false)}>
  <DialogTitle>{editingFamily ? 'Modifier' : 'Créer'} une famille</DialogTitle>
  <DialogContent>
    <TextField
      fullWidth
      label="Nom de la famille"
      value={familyName}
      onChange={(e) => setFamilyName(e.target.value)}
      required
    />
  </DialogContent>
  <DialogActions>
    <Button onClick={() => setDialogOpen(false)}>Annuler</Button>
    <Button variant="contained" onClick={handleSubmit}>
      {editingFamily ? 'Modifier' : 'Créer'}
    </Button>
  </DialogActions>
</Dialog>
```

**Suppression avec Warning** :
```tsx
<Dialog open={deleteDialogOpen} onClose={() => setDeleteDialogOpen(false)}>
  <DialogTitle>Confirmer la suppression</DialogTitle>
  <DialogContent>
    {familyToDelete && familyToDelete.memberCount > 0 ? (
      <Alert severity="warning">
        Cette famille contient {familyToDelete.memberCount} membre(s).
        Veuillez d'abord supprimer ou déplacer les membres.
      </Alert>
    ) : (
      <DialogContentText>
        Êtes-vous sûr de vouloir supprimer la famille "{familyToDelete?.name}" ?
      </DialogContentText>
    )}
  </DialogContent>
  <DialogActions>
    <Button onClick={() => setDeleteDialogOpen(false)}>Annuler</Button>
    <Button
      onClick={handleDeleteConfirm}
      color="error"
      disabled={familyToDelete && familyToDelete.memberCount > 0}
    >
      Supprimer
    </Button>
  </DialogActions>
</Dialog>
```

---

## Bonnes Pratiques des Composants

### 1. Props Typées

Tous les composants ont des interfaces TypeScript strictes :

```typescript
interface MyComponentProps {
  required: string;
  optional?: number;
  callback: (id: number) => void;
}

const MyComponent: React.FC<MyComponentProps> = ({ required, optional, callback }) => {
  // ...
};
```

### 2. React.memo pour Performance

Les composants lourds utilisent `React.memo()` :

```typescript
const GiftFormDialog = React.memo<GiftFormDialogProps>(({ open, gift, onSave }) => {
  // Ne re-render que si open, gift ou onSave changent
});
```

### 3. useCallback pour Callbacks

Les callbacks passés aux enfants sont memoizés :

```typescript
const handleSave = useCallback((data: GiftData) => {
  // Action
}, [dependencies]);

<ChildComponent onSave={handleSave} />
```

### 4. Gestion d'Erreurs

Tous les composants gèrent les erreurs gracefully :

```typescript
try {
  await apiCall();
  setSuccess(true);
} catch (error) {
  setError(error.message || 'Une erreur est survenue');
}
```

### 5. Loading States

Les actions asynchrones affichent des loaders :

```typescript
<Button disabled={loading}>
  {loading ? <CircularProgress size={20} /> : 'Sauvegarder'}
</Button>
```

---

## Tests

### Composants Testés

- ✅ **Avatar** : Tests complets (Avatar.test.tsx)
- ❌ Autres composants : À tester

### Exemple de Test

```typescript
// Avatar.test.tsx
describe('Avatar Component', () => {
  it('renders user initials when no avatar', () => {
    const mockUser = { firstName: 'John', lastName: 'Doe', avatar: null };
    render(<Avatar user={mockUser} size="medium" />);
    expect(screen.getByText('JD')).toBeInTheDocument();
  });

  it('renders avatar image when provided', () => {
    const mockUser = { firstName: 'John', avatar: 'avatar.png' };
    const { container } = render(<Avatar user={mockUser} />);
    const img = container.querySelector('img');
    expect(img).toHaveAttribute('src', expect.stringContaining('avatar.png'));
  });

  it('falls back to initials on image error', async () => {
    const mockUser = { firstName: 'John', lastName: 'Doe', avatar: 'broken.png' };
    const { container } = render(<Avatar user={mockUser} />);

    const img = container.querySelector('img');
    fireEvent.error(img!);

    await waitFor(() => {
      expect(screen.getByText('JD')).toBeInTheDocument();
    });
  });
});
```

---

## Récapitulatif

| Composant | Catégorie | Props | State | Memo | Tests |
|-----------|-----------|-------|-------|------|-------|
| NavigationBar | Layout | 0 | ✅ | ❌ | ❌ |
| Avatar | Layout | 3 | ✅ | ❌ | ✅ |
| ChristmasLayout | Layout | 1 | ❌ | ❌ | ❌ |
| ManagingChildBanner | Layout | 0 | ❌ | ❌ | ❌ |
| ProtectedRoute | Layout | 1 | ❌ | ❌ | ❌ |
| GiftFormDialog | Gifts | 4 | ✅ | ✅ | ❌ |
| GiftListItem | Gifts | 3 | ✅ | ❌ | ❌ |
| ImportDialog | Gifts | 5 | ✅ | ❌ | ❌ |
| UserGiftListItem | UserList | 4 | ✅ | ❌ | ❌ |
| ReserveDialog | UserList | 4 | ✅ | ❌ | ❌ |
| ProfileForm | Profile | 2 | ✅ | ✅ | ❌ |
| AvatarUpload | Profile | 3 | ✅ | ✅ | ❌ |
| PasswordChangeForm | Profile | 3 | ✅ | ✅ | ❌ |
| AdminDashboard | Admin | 2 | ❌ | ❌ | ❌ |
| AdminUsers | Admin | 6 | ✅ | ❌ | ❌ |
| AdminFamilies | Admin | 5 | ✅ | ❌ | ❌ |

---

## Références

- [React Component Patterns](https://react.dev/learn/passing-props-to-a-component)
- [Material-UI Components](https://mui.com/material-ui/all-components/)
- [React.memo Documentation](https://react.dev/reference/react/memo)
- [TypeScript with React](https://react-typescript-cheatsheet.netlify.app/)
