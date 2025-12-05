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
  CircularProgress,
  Chip,
} from '@mui/material';
import {
  Edit as EditIcon,
  Delete as DeleteIcon,
  Add as AddIcon,
} from '@mui/icons-material';
import { adminAPI } from '../../services/api';

const AdminFamilies = ({ setError }) => {
  const [families, setFamilies] = useState([]);
  const [loading, setLoading] = useState(true);
  const [openDialog, setOpenDialog] = useState(false);
  const [editingFamily, setEditingFamily] = useState(null);
  const [formData, setFormData] = useState({
    name: '',
  });

  useEffect(() => {
    fetchFamilies();
  }, []);

  const fetchFamilies = async () => {
    try {
      setLoading(true);
      const response = await adminAPI.getFamilies();
      setFamilies(response.data);
      setError('');
    } catch (err) {
      console.error('Error fetching families:', err);
      setError('Erreur lors du chargement des familles');
    } finally {
      setLoading(false);
    }
  };

  const handleOpenDialog = (family = null) => {
    if (family) {
      setEditingFamily(family);
      setFormData({
        name: family.name,
      });
    } else {
      setEditingFamily(null);
      setFormData({
        name: '',
      });
    }
    setOpenDialog(true);
  };

  const handleCloseDialog = () => {
    setOpenDialog(false);
    setEditingFamily(null);
  };

  const handleSubmit = async () => {
    try {
      if (editingFamily) {
        await adminAPI.updateFamily(editingFamily.id, formData);
      } else {
        await adminAPI.createFamily(formData);
      }
      await fetchFamilies();
      handleCloseDialog();
      setError('');
    } catch (err) {
      console.error('Error saving family:', err);
      setError(err.response?.data?.message || 'Erreur lors de l\'enregistrement');
    }
  };

  const handleDelete = async (familyId) => {
    if (!window.confirm('Êtes-vous sûr de vouloir supprimer cette famille ?')) {
      return;
    }

    try {
      await adminAPI.deleteFamily(familyId);
      await fetchFamilies();
      setError('');
    } catch (err) {
      console.error('Error deleting family:', err);
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
          Nouvelle famille
        </Button>
      </Box>

      <TableContainer component={Paper}>
        <Table>
          <TableHead>
            <TableRow>
              <TableCell>Nom</TableCell>
              <TableCell>Nombre d'utilisateurs</TableCell>
              <TableCell>Date de création</TableCell>
              <TableCell align="right">Actions</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {families.map((family) => (
              <TableRow key={family.id}>
                <TableCell>
                  <strong>{family.name}</strong>
                </TableCell>
                <TableCell>
                  <Chip label={family.userCount} color="primary" size="small" />
                </TableCell>
                <TableCell>
                  {new Date(family.createdAt).toLocaleDateString('fr-FR')}
                </TableCell>
                <TableCell align="right">
                  <IconButton
                    size="small"
                    onClick={() => handleOpenDialog(family)}
                    color="primary"
                  >
                    <EditIcon />
                  </IconButton>
                  <IconButton
                    size="small"
                    onClick={() => handleDelete(family.id)}
                    color="error"
                    disabled={family.userCount > 0}
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
          {editingFamily ? 'Modifier la famille' : 'Nouvelle famille'}
        </DialogTitle>
        <DialogContent>
          <Box sx={{ pt: 2 }}>
            <TextField
              label="Nom de la famille"
              value={formData.name}
              onChange={(e) => setFormData({ ...formData, name: e.target.value })}
              required
              fullWidth
              autoFocus
            />
          </Box>
        </DialogContent>
        <DialogActions>
          <Button onClick={handleCloseDialog}>Annuler</Button>
          <Button onClick={handleSubmit} variant="contained">
            {editingFamily ? 'Modifier' : 'Créer'}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default AdminFamilies;
