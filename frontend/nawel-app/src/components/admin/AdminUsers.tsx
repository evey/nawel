import { useState, useEffect, ChangeEvent } from 'react';
import {
  Box,
  Button,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
  IconButton,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  Select,
  MenuItem,
  FormControl,
  InputLabel,
  FormControlLabel,
  Checkbox,
  CircularProgress,
  Chip,
  SelectChangeEvent,
  Typography,
} from '@mui/material';
import {
  Edit as EditIcon,
  Delete as DeleteIcon,
  Add as AddIcon,
} from '@mui/icons-material';
import { adminAPI } from '../../services/api';
import { useAuth } from '../../contexts/AuthContext';
import type { User, Family } from '../../types';
import styles from '../../css/AdminUsers.module.less';

interface AdminUsersProps {
  setError: (error: string) => void;
}

interface UserFormData {
  login: string;
  password: string;
  email: string;
  firstName: string;
  lastName: string;
  familyId: number | '';
  isChildren: boolean;
  isAdmin: boolean;
}

const AdminUsers = ({ setError }: AdminUsersProps): JSX.Element => {
  const { user: currentUser } = useAuth();
  const [users, setUsers] = useState<User[]>([]);
  const [families, setFamilies] = useState<Family[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [openDialog, setOpenDialog] = useState<boolean>(false);
  const [editingUser, setEditingUser] = useState<User | null>(null);
  const [formData, setFormData] = useState<UserFormData>({
    login: '',
    password: '',
    email: '',
    firstName: '',
    lastName: '',
    familyId: '',
    isChildren: false,
    isAdmin: false,
  });

  useEffect(() => {
    fetchData();
  }, []);

  const fetchData = async (): Promise<void> => {
    try {
      setLoading(true);
      const [usersRes, familiesRes] = await Promise.all([
        adminAPI.getUsers(),
        adminAPI.getFamilies(),
      ]);
      setUsers(usersRes.data);
      setFamilies(familiesRes.data);
      setError('');
    } catch (err) {
      console.error('Error fetching data:', err);
      setError('Erreur lors du chargement des données');
    } finally {
      setLoading(false);
    }
  };

  const handleOpenDialog = (user: User | null = null): void => {
    if (user) {
      setEditingUser(user);
      setFormData({
        login: user.login,
        password: '',
        email: user.email || '',
        firstName: user.firstName || '',
        lastName: user.lastName || '',
        familyId: user.familyId,
        isChildren: user.isChildren,
        isAdmin: user.isAdmin,
      });
    } else {
      setEditingUser(null);
      setFormData({
        login: '',
        password: '',
        email: '',
        firstName: '',
        lastName: '',
        familyId: families.length > 0 ? families[0].id : '',
        isChildren: false,
        isAdmin: false,
      });
    }
    setOpenDialog(true);
  };

  const handleCloseDialog = (): void => {
    setOpenDialog(false);
    setEditingUser(null);
  };

  const handleSubmit = async (): Promise<void> => {
    try {
      if (editingUser) {
        // Update
        const updateData = {
          email: formData.email,
          firstName: formData.firstName,
          lastName: formData.lastName,
          familyId: formData.familyId === '' ? undefined : formData.familyId,
          isChildren: formData.isChildren,
          isAdmin: formData.isAdmin,
        };
        await adminAPI.updateUser(editingUser.id, updateData);
      } else {
        // Create
        const createData = {
          ...formData,
          familyId: formData.familyId === '' ? undefined : formData.familyId,
        };
        await adminAPI.createUser(createData);
      }
      await fetchData();
      handleCloseDialog();
      setError('');
    } catch (err: any) {
      console.error('Error saving user:', err);
      setError(err.response?.data?.message || 'Erreur lors de l\'enregistrement');
    }
  };

  const handleDelete = async (userId: number): Promise<void> => {
    if (!window.confirm('Êtes-vous sûr de vouloir supprimer cet utilisateur ?')) {
      return;
    }

    try {
      await adminAPI.deleteUser(userId);
      await fetchData();
      setError('');
    } catch (err: any) {
      console.error('Error deleting user:', err);
      setError(err.response?.data?.message || 'Erreur lors de la suppression');
    }
  };

  if (loading) {
    return (
      <Box className={styles.loadingContainer}>
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Box>
      <Box className={styles.header}>
        <Box />
        <Button
          variant="contained"
          startIcon={<AddIcon />}
          onClick={() => handleOpenDialog()}
        >
          Nouvel utilisateur
        </Button>
      </Box>

      <TableContainer component={Paper}>
        <Table>
          <TableHead>
            <TableRow>
              <TableCell>Login</TableCell>
              <TableCell>Nom</TableCell>
              <TableCell>Email</TableCell>
              <TableCell>Famille</TableCell>
              <TableCell>Type</TableCell>
              <TableCell>Rôle</TableCell>
              <TableCell align="right">Actions</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {users.map((user) => (
              <TableRow key={user.id}>
                <TableCell>{user.login}</TableCell>
                <TableCell>
                  {user.firstName} {user.lastName}
                </TableCell>
                <TableCell>{user.email || '-'}</TableCell>
                <TableCell>{user.familyName}</TableCell>
                <TableCell>
                  {user.isChildren ? (
                    <Chip label="Enfant" color="info" size="small" />
                  ) : (
                    <Chip label="Adulte" size="small" />
                  )}
                </TableCell>
                <TableCell>
                  {user.isAdmin && (
                    <Chip label="Admin" color="error" size="small" />
                  )}
                </TableCell>
                <TableCell align="right">
                  <IconButton
                    size="small"
                    onClick={() => handleOpenDialog(user)}
                    color="primary"
                  >
                    <EditIcon />
                  </IconButton>
                  <IconButton
                    size="small"
                    onClick={() => handleDelete(user.id)}
                    color="error"
                  >
                    <DeleteIcon />
                  </IconButton>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </TableContainer>

      {/* Dialog for create/edit */}
      <Dialog open={openDialog} onClose={handleCloseDialog} maxWidth="sm" fullWidth>
        <DialogTitle>
          {editingUser ? 'Modifier l\'utilisateur' : 'Nouvel utilisateur'}
        </DialogTitle>
        <DialogContent>
          <Box className={styles.dialogFormContainer}>
            <TextField
              label="Login"
              value={formData.login}
              onChange={(e: ChangeEvent<HTMLInputElement>) => setFormData({ ...formData, login: e.target.value })}
              required
              disabled={!!editingUser}
              fullWidth
            />
            {!editingUser && (
              <TextField
                label="Mot de passe"
                type="password"
                value={formData.password}
                onChange={(e: ChangeEvent<HTMLInputElement>) => setFormData({ ...formData, password: e.target.value })}
                required
                fullWidth
              />
            )}
            <TextField
              label="Email"
              type="email"
              value={formData.email}
              onChange={(e: ChangeEvent<HTMLInputElement>) => setFormData({ ...formData, email: e.target.value })}
              fullWidth
            />
            <TextField
              label="Prénom"
              value={formData.firstName}
              onChange={(e: ChangeEvent<HTMLInputElement>) => setFormData({ ...formData, firstName: e.target.value })}
              fullWidth
            />
            <TextField
              label="Nom"
              value={formData.lastName}
              onChange={(e: ChangeEvent<HTMLInputElement>) => setFormData({ ...formData, lastName: e.target.value })}
              fullWidth
            />
            <FormControl fullWidth>
              <InputLabel>Famille</InputLabel>
              <Select
                value={formData.familyId}
                onChange={(e: SelectChangeEvent<number | ''>) => setFormData({ ...formData, familyId: e.target.value })}
                label="Famille"
                required
              >
                {families.map((family) => (
                  <MenuItem key={family.id} value={family.id}>
                    {family.name}
                  </MenuItem>
                ))}
              </Select>
            </FormControl>
            <FormControlLabel
              control={
                <Checkbox
                  checked={formData.isChildren}
                  onChange={(e: ChangeEvent<HTMLInputElement>) => setFormData({ ...formData, isChildren: e.target.checked })}
                />
              }
              label="Enfant"
            />
            <FormControlLabel
              control={
                <Checkbox
                  checked={formData.isAdmin}
                  onChange={(e: ChangeEvent<HTMLInputElement>) => setFormData({ ...formData, isAdmin: e.target.checked })}
                  disabled={editingUser?.id === currentUser?.id}
                />
              }
              label="Administrateur"
            />
            {editingUser?.id === currentUser?.id && (
              <Box className={styles.passwordWarning}>
                <Typography variant="caption" color="text.secondary">
                  Vous ne pouvez pas modifier votre propre rôle d'administrateur
                </Typography>
              </Box>
            )}
          </Box>
        </DialogContent>
        <DialogActions>
          <Button onClick={handleCloseDialog}>Annuler</Button>
          <Button onClick={handleSubmit} variant="contained">
            {editingUser ? 'Modifier' : 'Créer'}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default AdminUsers;
