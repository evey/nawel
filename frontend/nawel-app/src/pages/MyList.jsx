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
  Tooltip,
} from '@mui/material';
import {
  Add as AddIcon,
  Edit as EditIcon,
  Delete as DeleteIcon,
  Download as DownloadIcon,
  Group as GroupIcon,
  AutoAwesome as AutoAwesomeIcon,
} from '@mui/icons-material';
import NavigationBar from '../components/NavigationBar';
import ManagingChildBanner from '../components/ManagingChildBanner';
import { useAuth } from '../contexts/AuthContext';
import { giftsAPI, productsAPI } from '../services/api';

const MyList = () => {
  const navigate = useNavigate();
  const { user, managingChild } = useAuth();
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
  const [extractingInfo, setExtractingInfo] = useState(false);
  const [extractError, setExtractError] = useState('');

  useEffect(() => {
    fetchYears();
  }, []);

  useEffect(() => {
    fetchGifts();
  }, [selectedYear, managingChild]);

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
      const response = managingChild
        ? await giftsAPI.getChildGifts(managingChild.userId, selectedYear)
        : await giftsAPI.getMyGifts(selectedYear);
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
    setExtractError('');
  };

  const handleInputChange = (e) => {
    const { name, value, checked, type } = e.target;
    setFormData((prev) => ({
      ...prev,
      [name]: type === 'checkbox' ? checked : value,
    }));
  };

  const handleExtractInfo = async () => {
    if (!formData.url || !formData.url.trim()) {
      setExtractError('Veuillez entrer une URL valide');
      return;
    }

    try {
      setExtractingInfo(true);
      setExtractError('');

      const response = await productsAPI.extractInfo(formData.url);
      const productInfo = response.data;

      // Remplir automatiquement les champs s'ils sont vides
      setFormData((prev) => ({
        ...prev,
        name: prev.name || productInfo.name || prev.name,
        description: prev.description || productInfo.description || prev.description,
        imageUrl: prev.imageUrl || productInfo.imageUrl || prev.imageUrl,
        price: prev.price || (productInfo.price ? productInfo.price.toString() : '') || prev.price,
      }));

      setSuccessMessage('Informations extraites avec succès !');
      setTimeout(() => setSuccessMessage(''), 3000);
    } catch (err) {
      console.error('Error extracting product info:', err);
      setExtractError(err.response?.data?.message || 'Impossible d\'extraire les informations. Vous pouvez les remplir manuellement.');
    } finally {
      setExtractingInfo(false);
    }
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
        // Use different endpoint based on management mode
        if (managingChild) {
          await giftsAPI.createGiftForChild(managingChild.userId, giftData);
        } else {
          await giftsAPI.createGift(giftData);
        }
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

  const isParticipating = (gift) => {
    if (!gift.isGroupGift || !gift.participantNames) return false;
    const userDisplayName = user?.firstName || user?.login;
    return gift.participantNames.includes(userDisplayName);
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
      <NavigationBar title={managingChild ? `Liste de ${managingChild.userName}` : "Ma liste de cadeaux"} />

      <Container maxWidth="md" sx={{ mt: { xs: 2, sm: 4 }, mb: 4, px: { xs: 2, sm: 3 } }}>
        <ManagingChildBanner />

        <Box sx={{ display: 'flex', flexDirection: { xs: 'column', sm: 'row' }, justifyContent: 'space-between', alignItems: { xs: 'flex-start', sm: 'center' }, mb: 3, gap: 2 }}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, flexWrap: 'wrap' }}>
            <Typography variant="h4" sx={{ fontSize: { xs: '1.5rem', sm: '2.125rem' } }}>
              {managingChild ? `Cadeaux de ${managingChild.userName}` : 'Mes cadeaux'}
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
          <Box sx={{ display: 'flex', gap: 1, width: { xs: '100%', sm: 'auto' } }}>
            {!isPastYear && pastYearsForImport.length > 0 && (
              <Button
                variant="outlined"
                startIcon={<DownloadIcon />}
                onClick={handleOpenImportDialog}
                fullWidth
                sx={{ minWidth: { xs: 'auto', sm: '120px' } }}
              >
                Importer
              </Button>
            )}
            {!isPastYear && (
              <Button
                variant="contained"
                startIcon={<AddIcon />}
                onClick={() => handleOpenDialog()}
                fullWidth
                sx={{ minWidth: { xs: 'auto', sm: '120px' } }}
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
                  sx={{
                    py: 2,
                    alignItems: 'flex-start',
                    flexDirection: { xs: 'column', sm: 'row' },
                    gap: { xs: 2, sm: 0 }
                  }}
                >
                  {gift.imageUrl && (
                    <Box
                      component="img"
                      src={gift.imageUrl}
                      alt={gift.name}
                      sx={{
                        width: { xs: '100%', sm: 120 },
                        minHeight: { xs: 150, sm: 100 },
                        maxHeight: { xs: 250, sm: 200 },
                        objectFit: 'cover',
                        borderRadius: 1,
                        mr: { xs: 0, sm: 2 },
                        flexShrink: 0,
                        alignSelf: { xs: 'center', sm: 'stretch' },
                      }}
                      onError={(e) => {
                        e.target.style.display = 'none';
                      }}
                    />
                  )}
                  <ListItemText
                    sx={{ width: '100%' }}
                    primary={
                      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, flexWrap: 'wrap' }}>
                        <Typography variant="h6" sx={{ fontSize: { xs: '1.1rem', sm: '1.25rem' } }}>{gift.name}</Typography>
                        {/* Sur sa propre liste, on ne montre RIEN sur les réservations pour garder la surprise */}
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
                    <Box sx={{
                      display: 'flex',
                      gap: 1,
                      mt: { xs: 0, sm: 0 },
                      ml: { xs: 0, sm: 'auto' },
                      pl: { xs: 0, sm: 2 },
                      width: { xs: '100%', sm: 'auto' },
                      justifyContent: { xs: 'flex-end', sm: 'flex-start' }
                    }}>
                      <IconButton
                        onClick={() => handleOpenDialog(gift)}
                        size="medium"
                        sx={{ flexShrink: 0 }}
                      >
                        <EditIcon />
                      </IconButton>
                      <IconButton
                        onClick={() => handleDelete(gift.id)}
                        color="error"
                        size="medium"
                        sx={{ flexShrink: 0 }}
                      >
                        <DeleteIcon />
                      </IconButton>
                    </Box>
                  )}
                </ListItem>
              ))}
            </List>
          </Card>
        )}
      </Container>

      {/* Add/Edit Dialog */}
      <Dialog
        open={openDialog}
        onClose={handleCloseDialog}
        maxWidth="sm"
        fullWidth
        fullScreen={window.innerWidth < 600}
      >
        <DialogTitle>
          {editingGift ? 'Modifier le cadeau' : 'Ajouter un cadeau'}
        </DialogTitle>
        <DialogContent sx={{ pt: { xs: 3, sm: 2 } }}>
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
          <Box sx={{ display: 'flex', gap: 1, alignItems: 'flex-end' }}>
            <TextField
              margin="dense"
              name="url"
              label="Lien (URL)"
              type="url"
              fullWidth
              value={formData.url}
              onChange={handleInputChange}
              helperText="Collez un lien puis cliquez sur 'Extraire' pour remplir automatiquement les champs"
            />
            <Button
              variant="contained"
              onClick={handleExtractInfo}
              disabled={extractingInfo || !formData.url.trim()}
              startIcon={extractingInfo ? <CircularProgress size={20} /> : <AutoAwesomeIcon />}
              sx={{ mb: '4px', minWidth: '120px' }}
            >
              {extractingInfo ? 'Extraction...' : 'Extraire'}
            </Button>
          </Box>
          {extractError && (
            <Alert severity="warning" sx={{ mt: 1 }}>
              {extractError}
            </Alert>
          )}
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
      <Dialog
        open={openImportDialog}
        onClose={handleCloseImportDialog}
        fullWidth
        maxWidth="sm"
        fullScreen={window.innerWidth < 600}
      >
        <DialogTitle>Importer des cadeaux d'une année précédente</DialogTitle>
        <DialogContent sx={{ pt: { xs: 3, sm: 2 } }}>
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
