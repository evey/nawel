import { useState, useEffect } from 'react';
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
} from '@mui/material';
import {
  Edit as EditIcon,
  Delete as DeleteIcon,
  Add as AddIcon,
} from '@mui/icons-material';
import { adminAPI } from '../../services/api';

const AdminUsers = ({ setError }) => {
  const [users, setUsers] = useState([]);
  const [families, setFamilies] = useState([]);
  const [loading, setLoading] = useState(true);
  const [openDialog, setOpenDialog] = useState(false);
  const [editingUser, setEditingUser] = useState(null);
  const [formData, setFormData] = useState({
    login: '',
    password: '',
    email: '',
    firstName: '',
    lastName: '',
    familyId: '',
    isChildren: false,
  });

  useEffect(() => {
    fetchData();
  }, []);

  const fetchData = async () => {
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

  const handleOpenDialog = (user = null) => {
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
      });
    }
    setOpenDialog(true);
  };

  const handleCloseDialog = () => {
    setOpenDialog(false);
    setEditingUser(null);
  };

  const handleSubmit = async () => {
    try {
      if (editingUser) {
        // Update
        const updateData = {
          email: formData.email,
          firstName: formData.firstName,
          lastName: formData.lastName,
          familyId: formData.familyId,
          isChildren: formData.isChildren,
        };
        await adminAPI.updateUser(editingUser.id, updateData);
      } else {
        // Create
        await adminAPI.createUser(formData);
      }
      await fetchData();
      handleCloseDialog();
      setError('');
    } catch (err) {
      console.error('Error saving user:', err);
      setError(err.response?.data?.message || 'Erreur lors de l\'enregistrement');
    }
  };

  const handleDelete = async (userId) => {
    if (!window.confirm('Êtes-vous sûr de vouloir supprimer cet utilisateur ?')) {
      return;
    }

    try {
      await adminAPI.deleteUser(userId);
      await fetchData();
      setError('');
    } catch (err) {
      console.error('Error deleting user:', err);
      setError(err.response?.data?.message || 'Erreur lors de la suppression');
    }
  };

  if (loading) {
    return (
      <Box display="flex" justifyContent="center" py={4}>
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Box>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 2 }}>
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
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2, pt: 2 }}>
            <TextField
              label="Login"
              value={formData.login}
              onChange={(e) => setFormData({ ...formData, login: e.target.value })}
              required
              disabled={!!editingUser}
              fullWidth
            />
            {!editingUser && (
              <TextField
                label="Mot de passe"
                type="password"
                value={formData.password}
                onChange={(e) => setFormData({ ...formData, password: e.target.value })}
                required
                fullWidth
              />
            )}
            <TextField
              label="Email"
              type="email"
              value={formData.email}
              onChange={(e) => setFormData({ ...formData, email: e.target.value })}
              fullWidth
            />
            <TextField
              label="Prénom"
              value={formData.firstName}
              onChange={(e) => setFormData({ ...formData, firstName: e.target.value })}
              fullWidth
            />
            <TextField
              label="Nom"
              value={formData.lastName}
              onChange={(e) => setFormData({ ...formData, lastName: e.target.value })}
              fullWidth
            />
            <FormControl fullWidth>
              <InputLabel>Famille</InputLabel>
              <Select
                value={formData.familyId}
                onChange={(e) => setFormData({ ...formData, familyId: e.target.value })}
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
                  onChange={(e) => setFormData({ ...formData, isChildren: e.target.checked })}
                />
              }
              label="Enfant"
            />
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
