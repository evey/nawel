import { useState, useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import {
  Container,
  Box,
  Typography,
  Card,
  CardContent,
  Button,
  CircularProgress,
  Alert,
  List,
  ListItem,
  ListItemText,
  Chip,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Select,
  MenuItem,
  FormControl,
  InputLabel,
} from '@mui/material';
import {
  ShoppingCart as ShoppingCartIcon,
  Group as GroupIcon,
  CheckCircle as CheckCircleIcon,
} from '@mui/icons-material';
import NavigationBar from '../components/NavigationBar';
import { useAuth } from '../contexts/AuthContext';
import { giftsAPI, usersAPI } from '../services/api';

const UserList = () => {
  const navigate = useNavigate();
  const { userId } = useParams();
  const { user } = useAuth();
  const [gifts, setGifts] = useState([]);
  const [listOwner, setListOwner] = useState(null);
  const [availableYears, setAvailableYears] = useState([]);
  const [selectedYear, setSelectedYear] = useState(new Date().getFullYear());
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [selectedGift, setSelectedGift] = useState(null);
  const [openDialog, setOpenDialog] = useState(false);

  useEffect(() => {
    // For now, get years from gifts API
    // Ideally we'd have a separate endpoint to get available years for a user
    fetchInitialData();
  }, [userId]);

  useEffect(() => {
    if (availableYears.length > 0) {
      fetchUserAndGifts();
    }
  }, [userId, selectedYear]);

  const fetchInitialData = async () => {
    try {
      setLoading(true);
      // Fetch current year first
      const currentYear = new Date().getFullYear();
      setSelectedYear(currentYear);

      // Get gifts for current year to determine available years
      const giftsResponse = await giftsAPI.getUserGifts(parseInt(userId), currentYear);
      setGifts(giftsResponse.data);

      // For now, just show current year. In a real app, we'd have an endpoint to get all years
      const years = [currentYear];
      // Check if there are older years by trying previous years
      for (let year = currentYear - 1; year >= currentYear - 3; year--) {
        try {
          const oldGiftsResponse = await giftsAPI.getUserGifts(parseInt(userId), year);
          if (oldGiftsResponse.data.length > 0) {
            years.push(year);
          }
        } catch (err) {
          // Year doesn't exist, stop checking
          break;
        }
      }

      setAvailableYears(years.sort((a, b) => b - a));
      setError('');
    } catch (err) {
      console.error('Error fetching data:', err);
      setError('Erreur lors du chargement de la liste');
    } finally {
      setLoading(false);
    }
  };

  const fetchUserAndGifts = async () => {
    try {
      setLoading(true);

      // Fetch gifts
      const giftsResponse = await giftsAPI.getUserGifts(parseInt(userId), selectedYear);
      setGifts(giftsResponse.data);

      // Get user info from the first gift or fetch separately if needed
      if (giftsResponse.data.length > 0) {
        // We'll need to add an endpoint to get user info, for now use placeholder
        setListOwner({ name: 'Utilisateur' });
      }

      setError('');
    } catch (err) {
      console.error('Error fetching data:', err);
      setError('Erreur lors du chargement de la liste');
    } finally {
      setLoading(false);
    }
  };

  const handleYearChange = (event) => {
    setSelectedYear(event.target.value);
  };

  const handleOpenReserveDialog = (gift) => {
    setSelectedGift(gift);
    setOpenDialog(true);
  };

  const handleCloseDialog = () => {
    setOpenDialog(false);
    setSelectedGift(null);
  };

  const handleReserve = async () => {
    if (!selectedGift) return;

    try {
      await giftsAPI.reserveGift(selectedGift.id);
      await fetchUserAndGifts();
      handleCloseDialog();
      setError('');
    } catch (err) {
      console.error('Error reserving gift:', err);
      setError(err.response?.data?.message || 'Erreur lors de la réservation');
    }
  };

  const handleUnreserve = async (giftId) => {
    if (!window.confirm('Êtes-vous sûr de vouloir annuler cette réservation ?')) {
      return;
    }

    try {
      await giftsAPI.unreserveGift(giftId);
      await fetchUserAndGifts();
      setError('');
    } catch (err) {
      console.error('Error unreserving gift:', err);
      setError(err.response?.data?.message || 'Erreur lors de l\'annulation');
    }
  };

  const isReservedByMe = (gift) => {
    if (gift.isGroupGift) {
      return gift.participantCount > 0 && gift.takenByUserId === user?.id;
    }
    return gift.isTaken && gift.takenByUserId === user?.id;
  };

  const isPastYear = selectedYear < new Date().getFullYear();

  if (loading && availableYears.length === 0) {
    return (
      <Box display="flex" justifyContent="center" alignItems="center" minHeight="100vh">
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Box sx={{ flexGrow: 1 }}>
      <NavigationBar title="Liste de cadeaux" />

      <Container maxWidth="md" sx={{ mt: 4, mb: 4 }}>
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3, flexWrap: 'wrap', gap: 2 }}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
            <Typography variant="h4">
              Cadeaux de {listOwner?.name || 'l\'utilisateur'}
            </Typography>
            {availableYears.length > 1 && (
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
            )}
            {isPastYear && (
              <Chip label="Historique" color="info" size="small" />
            )}
          </Box>
        </Box>

        {error && (
          <Alert severity="error" sx={{ mb: 3 }}>
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
                Aucun cadeau n'a été ajouté à cette liste pour {selectedYear}.
              </Typography>
            </CardContent>
          </Card>
        ) : (
          <Card>
            <List>
              {gifts.map((gift, index) => {
                const reservedByMe = isReservedByMe(gift);
                const canReserve = !isPastYear && (!gift.isTaken || gift.isGroupGift);

                return (
                  <ListItem
                    key={gift.id}
                    divider={index < gifts.length - 1}
                    sx={{ py: 2 }}
                  >
                    <ListItemText
                      primary={
                        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, flexWrap: 'wrap' }}>
                          <Typography variant="h6">{gift.name}</Typography>
                          {gift.isGroupGift && (
                            <Chip
                              icon={<GroupIcon />}
                              label={`Cadeau groupé (${gift.participantCount} participant${gift.participantCount !== 1 ? 's' : ''})`}
                              color="primary"
                              size="small"
                            />
                          )}
                          {gift.isTaken && !gift.isGroupGift && (
                            <Chip
                              icon={<CheckCircleIcon />}
                              label={reservedByMe ? 'Réservé par vous' : 'Déjà réservé'}
                              color={reservedByMe ? 'success' : 'default'}
                              size="small"
                            />
                          )}
                        </Box>
                      }
                      secondary={
                        <Box sx={{ mt: 1 }}>
                          {gift.description && (
                            <Typography variant="body2" color="text.secondary" sx={{ mb: 1 }}>
                              {gift.description}
                            </Typography>
                          )}
                          {gift.url && (
                            <Typography variant="body2" sx={{ mb: 0.5 }}>
                              <a href={gift.url} target="_blank" rel="noopener noreferrer">
                                Voir le lien
                              </a>
                            </Typography>
                          )}
                          {gift.price && (
                            <Typography variant="body2" sx={{ fontWeight: 'bold' }}>
                              Prix: {gift.price.toFixed(2)} €
                            </Typography>
                          )}
                        </Box>
                      }
                    />
                    {!isPastYear && (
                      <Box sx={{ ml: 2, display: 'flex', flexDirection: 'column', gap: 1 }}>
                        {reservedByMe ? (
                          <Button
                            variant="outlined"
                            color="error"
                            onClick={() => handleUnreserve(gift.id)}
                            size="small"
                          >
                            Annuler
                          </Button>
                        ) : canReserve ? (
                          <Button
                            variant="contained"
                            color="primary"
                            onClick={() => handleOpenReserveDialog(gift)}
                            size="small"
                            startIcon={<ShoppingCartIcon />}
                          >
                            Réserver
                          </Button>
                        ) : (
                          <Chip label="Non disponible" size="small" />
                        )}
                      </Box>
                    )}
                  </ListItem>
                );
              })}
            </List>
          </Card>
        )}
      </Container>

      {/* Reserve Confirmation Dialog */}
      <Dialog open={openDialog} onClose={handleCloseDialog}>
        <DialogTitle>Confirmer la réservation</DialogTitle>
        <DialogContent>
          <Typography>
            Voulez-vous réserver le cadeau "{selectedGift?.name}" ?
          </Typography>
          {selectedGift?.isGroupGift && (
            <Alert severity="info" sx={{ mt: 2 }}>
              Il s'agit d'un cadeau groupé. Vous participerez à ce cadeau avec d'autres personnes.
            </Alert>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={handleCloseDialog}>Annuler</Button>
          <Button onClick={handleReserve} variant="contained" color="primary">
            Confirmer
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default UserList;
