import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Container,
  Box,
  Typography,
  Card,
  CardContent,
  Button,
  IconButton,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  CircularProgress,
  Alert,
  List,
  ListItem,
  ListItemText,
  ListItemSecondaryAction,
  Select,
  MenuItem,
  FormControl,
  InputLabel,
  Chip,
} from '@mui/material';
import {
  Add as AddIcon,
  Edit as EditIcon,
  Delete as DeleteIcon,
  Download as DownloadIcon,
} from '@mui/icons-material';
import NavigationBar from '../components/NavigationBar';
import { giftsAPI } from '../services/api';

const MyList = () => {
  const navigate = useNavigate();
  const [gifts, setGifts] = useState([]);
  const [availableYears, setAvailableYears] = useState([]);
  const [selectedYear, setSelectedYear] = useState(new Date().getFullYear());
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [successMessage, setSuccessMessage] = useState('');
  const [openDialog, setOpenDialog] = useState(false);
  const [openImportDialog, setOpenImportDialog] = useState(false);
  const [editingGift, setEditingGift] = useState(null);
  const [formData, setFormData] = useState({
    name: '',
    description: '',
    url: '',
    imageUrl: '',
    price: '',
  });

  useEffect(() => {
    fetchYears();
  }, []);

  useEffect(() => {
    fetchGifts();
  }, [selectedYear]);

  const fetchYears = async () => {
    try {
      const response = await giftsAPI.getAvailableYears();
      const years = response.data;
      // Si aucune année n'existe, ajouter l'année courante
      if (years.length === 0) {
        years.push(new Date().getFullYear());
      }
      setAvailableYears(years);
    } catch (err) {
      console.error('Error fetching years:', err);
      // En cas d'erreur, utiliser au moins l'année courante
      setAvailableYears([new Date().getFullYear()]);
    }
  };

  const fetchGifts = async () => {
    try {
      setLoading(true);
      const response = await giftsAPI.getMyGifts(selectedYear);
      setGifts(response.data);
      setError('');
    } catch (err) {
      console.error('Error fetching gifts:', err);
      setError('Erreur lors du chargement des cadeaux');
    } finally {
      setLoading(false);
    }
  };

  const handleYearChange = (event) => {
    setSelectedYear(event.target.value);
  };

  const handleOpenDialog = (gift = null) => {
    if (gift) {
      setEditingGift(gift);
      setFormData({
        name: gift.name,
        description: gift.description || '',
        url: gift.url || '',
        imageUrl: gift.imageUrl || '',
        price: gift.price || '',
      });
    } else {
      setEditingGift(null);
      setFormData({
        name: '',
        description: '',
        url: '',
        imageUrl: '',
        price: '',
      });
    }
    setOpenDialog(true);
  };

  const handleCloseDialog = () => {
    setOpenDialog(false);
    setEditingGift(null);
  };

  const handleInputChange = (e) => {
    const { name, value, checked, type } = e.target;
    setFormData((prev) => ({
      ...prev,
      [name]: type === 'checkbox' ? checked : value,
    }));
  };

  const handleSubmit = async () => {
    try {
      const giftData = {
        name: formData.name,
        description: formData.description || null,
        url: formData.url || null,
        imageUrl: formData.imageUrl || null,
        price: formData.price ? parseFloat(formData.price) : null,
        isGroupGift: false,
      };

      if (editingGift) {
        await giftsAPI.updateGift(editingGift.id, giftData);
        setSuccessMessage('Cadeau modifié avec succès');
      } else {
        await giftsAPI.createGift(giftData);
        setSuccessMessage('Cadeau ajouté avec succès');
      }

      await fetchGifts();
      await fetchYears();
      handleCloseDialog();
      setError('');
      setTimeout(() => setSuccessMessage(''), 3000);
    } catch (err) {
      console.error('Error saving gift:', err);
      setError('Erreur lors de l\'enregistrement du cadeau');
    }
  };

  const handleDelete = async (id) => {
    if (!window.confirm('Êtes-vous sûr de vouloir supprimer ce cadeau ?')) {
      return;
    }

    try {
      await giftsAPI.deleteGift(id);
      await fetchGifts();
      setSuccessMessage('Cadeau supprimé avec succès');
      setError('');
      setTimeout(() => setSuccessMessage(''), 3000);
    } catch (err) {
      console.error('Error deleting gift:', err);
      setError('Erreur lors de la suppression du cadeau');
    }
  };

  const handleOpenImportDialog = () => {
    setOpenImportDialog(true);
  };

  const handleCloseImportDialog = () => {
    setOpenImportDialog(false);
  };

  const handleImport = async (year) => {
    try {
      const response = await giftsAPI.importFromYear(year);
      await fetchGifts();
      handleCloseImportDialog();
      setSuccessMessage(`${response.data.count} cadeau(x) importé(s) avec succès`);
      setTimeout(() => setSuccessMessage(''), 3000);
    } catch (err) {
      console.error('Error importing gifts:', err);
      setError(err.response?.data?.message || 'Erreur lors de l\'import');
    }
  };

  const isPastYear = selectedYear < new Date().getFullYear();
  const pastYearsForImport = availableYears.filter(y => y < new Date().getFullYear());

  if (loading && availableYears.length === 0) {
    return (
      <Box display="flex" justifyContent="center" alignItems="center" minHeight="100vh">
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Box sx={{ flexGrow: 1 }}>
      <NavigationBar title="Ma liste de cadeaux" />

      <Container maxWidth="md" sx={{ mt: 4, mb: 4 }}>
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3, flexWrap: 'wrap', gap: 2 }}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
            <Typography variant="h4">
              Mes cadeaux
            </Typography>
            <FormControl size="small" sx={{ minWidth: 120 }}>
              <InputLabel>Année</InputLabel>
              <Select
                value={selectedYear}
                label="Année"
                onChange={handleYearChange}
              >
                {availableYears.map((year) => (
                  <MenuItem key={year} value={year}>
                    {year}
                  </MenuItem>
                ))}
              </Select>
            </FormControl>
            {isPastYear && (
              <Chip label="Lecture seule" color="info" size="small" />
            )}
          </Box>
          <Box sx={{ display: 'flex', gap: 1 }}>
            {!isPastYear && pastYearsForImport.length > 0 && (
              <Button
                variant="outlined"
                startIcon={<DownloadIcon />}
                onClick={handleOpenImportDialog}
              >
                Importer
              </Button>
            )}
            {!isPastYear && (
              <Button
                variant="contained"
                startIcon={<AddIcon />}
                onClick={() => handleOpenDialog()}
              >
                Ajouter
              </Button>
            )}
          </Box>
        </Box>

        {successMessage && (
          <Alert severity="success" sx={{ mb: 3 }} onClose={() => setSuccessMessage('')}>
            {successMessage}
          </Alert>
        )}

        {error && (
          <Alert severity="error" sx={{ mb: 3 }} onClose={() => setError('')}>
            {error}
          </Alert>
        )}

        {loading ? (
          <Box display="flex" justifyContent="center" py={4}>
            <CircularProgress />
          </Box>
        ) : gifts.length === 0 ? (
          <Card>
            <CardContent sx={{ textAlign: 'center', py: 4 }}>
              <Typography variant="body1" color="text.secondary">
                {isPastYear
                  ? `Vous n'aviez pas de cadeaux pour ${selectedYear}.`
                  : 'Vous n\'avez pas encore ajouté de cadeaux à votre liste.'}
              </Typography>
              {!isPastYear && (
                <Button
                  variant="contained"
                  startIcon={<AddIcon />}
                  onClick={() => handleOpenDialog()}
                  sx={{ mt: 2 }}
                >
                  Ajouter un premier cadeau
                </Button>
              )}
            </CardContent>
          </Card>
        ) : (
          <Card>
            <List>
              {gifts.map((gift, index) => (
                <ListItem
                  key={gift.id}
                  divider={index < gifts.length - 1}
                  sx={{ py: 2, alignItems: 'flex-start' }}
                >
                  {gift.imageUrl && (
                    <Box
                      component="img"
                      src={gift.imageUrl}
                      alt={gift.name}
                      sx={{
                        width: 120,
                        minHeight: 100,
                        maxHeight: 200,
                        objectFit: 'cover',
                        borderRadius: 1,
                        mr: 2,
                        flexShrink: 0,
                        alignSelf: 'stretch',
                      }}
                      onError={(e) => {
                        e.target.style.display = 'none';
                      }}
                    />
                  )}
                  <ListItemText
                    primary={
                      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                        <Typography variant="h6">{gift.name}</Typography>
                        {gift.isGroupGift && (
                          <Typography
                            variant="caption"
                            sx={{
                              bgcolor: 'primary.main',
                              color: 'white',
                              px: 1,
                              py: 0.5,
                              borderRadius: 1,
                            }}
                          >
                            Cadeau groupé
                          </Typography>
                        )}
                        {gift.isTaken && (
                          <Typography
                            variant="caption"
                            sx={{
                              bgcolor: 'success.main',
                              color: 'white',
                              px: 1,
                              py: 0.5,
                              borderRadius: 1,
                            }}
                          >
                            Réservé
                          </Typography>
                        )}
                      </Box>
                    }
                    secondary={
                      <Box sx={{ mt: 1 }}>
                        {gift.description && (
                          <Typography variant="body2" color="text.secondary">
                            {gift.description}
                          </Typography>
                        )}
                        {gift.url && (
                          <Typography variant="body2" sx={{ mt: 0.5 }}>
                            <a href={gift.url} target="_blank" rel="noopener noreferrer">
                              Voir le lien
                            </a>
                          </Typography>
                        )}
                        {gift.price && (
                          <Typography variant="body2" sx={{ mt: 0.5, fontWeight: 'bold' }}>
                            Prix: {gift.price.toFixed(2)} €
                          </Typography>
                        )}
                      </Box>
                    }
                  />
                  {!isPastYear && (
                    <ListItemSecondaryAction>
                      <IconButton
                        edge="end"
                        onClick={() => handleOpenDialog(gift)}
                        sx={{ mr: 1 }}
                      >
                        <EditIcon />
                      </IconButton>
                      <IconButton
                        edge="end"
                        onClick={() => handleDelete(gift.id)}
                        color="error"
                      >
                        <DeleteIcon />
                      </IconButton>
                    </ListItemSecondaryAction>
                  )}
                </ListItem>
              ))}
            </List>
          </Card>
        )}
      </Container>

      {/* Add/Edit Dialog */}
      <Dialog open={openDialog} onClose={handleCloseDialog} maxWidth="sm" fullWidth>
        <DialogTitle>
          {editingGift ? 'Modifier le cadeau' : 'Ajouter un cadeau'}
        </DialogTitle>
        <DialogContent>
          <TextField
            autoFocus
            margin="dense"
            name="name"
            label="Nom du cadeau"
            type="text"
            fullWidth
            required
            value={formData.name}
            onChange={handleInputChange}
          />
          <TextField
            margin="dense"
            name="description"
            label="Description"
            type="text"
            fullWidth
            multiline
            rows={3}
            value={formData.description}
            onChange={handleInputChange}
          />
          <TextField
            margin="dense"
            name="url"
            label="Lien (URL)"
            type="url"
            fullWidth
            value={formData.url}
            onChange={handleInputChange}
          />
          <TextField
            margin="dense"
            name="imageUrl"
            label="URL de l'image"
            type="url"
            fullWidth
            value={formData.imageUrl}
            onChange={handleInputChange}
            helperText="URL d'une image pour illustrer le cadeau"
          />
          <TextField
            margin="dense"
            name="price"
            label="Prix (€)"
            type="number"
            fullWidth
            value={formData.price}
            onChange={handleInputChange}
            inputProps={{ step: '0.01', min: '0' }}
          />
        </DialogContent>
        <DialogActions>
          <Button onClick={handleCloseDialog}>Annuler</Button>
          <Button
            onClick={handleSubmit}
            variant="contained"
            disabled={!formData.name.trim()}
          >
            {editingGift ? 'Modifier' : 'Ajouter'}
          </Button>
        </DialogActions>
      </Dialog>

      {/* Import Dialog */}
      <Dialog open={openImportDialog} onClose={handleCloseImportDialog}>
        <DialogTitle>Importer des cadeaux d'une année précédente</DialogTitle>
        <DialogContent>
          <Typography variant="body2" sx={{ mb: 2 }}>
            Sélectionnez une année pour importer les cadeaux non achetés:
          </Typography>
          <List>
            {pastYearsForImport.map((year) => (
              <ListItem
                key={year}
                button
                onClick={() => handleImport(year)}
                sx={{
                  border: '1px solid',
                  borderColor: 'divider',
                  borderRadius: 1,
                  mb: 1,
                }}
              >
                <ListItemText
                  primary={year}
                  secondary="Importer les cadeaux non achetés"
                />
              </ListItem>
            ))}
          </List>
        </DialogContent>
        <DialogActions>
          <Button onClick={handleCloseImportDialog}>Annuler</Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default MyList;
