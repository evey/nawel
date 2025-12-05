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
  Tooltip,
  TextField,
} from '@mui/material';
import {
  ShoppingCart as ShoppingCartIcon,
  Group as GroupIcon,
  CheckCircle as CheckCircleIcon,
} from '@mui/icons-material';
import NavigationBar from '../components/NavigationBar';
import Avatar from '../components/Avatar';
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
  const [reserveComment, setReserveComment] = useState('');

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

      // Fetch user info
      const userResponse = await usersAPI.getById(parseInt(userId));
      setListOwner({
        name: userResponse.data.firstName || userResponse.data.login,
        avatar: userResponse.data.avatar,
        firstName: userResponse.data.firstName,
        login: userResponse.data.login
      });

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

      // Fetch user info
      const userResponse = await usersAPI.getById(parseInt(userId));
      setListOwner({
        name: userResponse.data.firstName || userResponse.data.login,
        avatar: userResponse.data.avatar,
        firstName: userResponse.data.firstName,
        login: userResponse.data.login
      });

      // Fetch gifts
      const giftsResponse = await giftsAPI.getUserGifts(parseInt(userId), selectedYear);
      setGifts(giftsResponse.data);

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
    setReserveComment(gift.comment || '');
    setOpenDialog(true);
  };

  const handleCloseDialog = () => {
    setOpenDialog(false);
    setSelectedGift(null);
    setReserveComment('');
  };

  const handleReserve = async () => {
    if (!selectedGift) return;

    try {
      await giftsAPI.reserveGift(selectedGift.id, { comment: reserveComment });
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

  const isParticipating = (gift) => {
    if (!gift.isGroupGift || !gift.participantNames) return false;
    const userDisplayName = user?.firstName || user?.login;
    return gift.participantNames.includes(userDisplayName);
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
      <NavigationBar title={`Liste de ${listOwner?.name || 'cadeaux'}`} />

      <Container maxWidth="md" sx={{ mt: { xs: 2, sm: 4 }, mb: 4, px: { xs: 2, sm: 3 } }}>
        <Box sx={{ display: 'flex', flexDirection: { xs: 'column', sm: 'row' }, justifyContent: 'space-between', alignItems: { xs: 'flex-start', sm: 'center' }, mb: 3, gap: 2 }}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, flexWrap: 'wrap' }}>
            {listOwner && (
              <Avatar
                user={listOwner}
                size={60}
                sx={{
                  width: { xs: 50, sm: 60 },
                  height: { xs: 50, sm: 60 },
                  fontSize: { xs: 20, sm: 24 }
                }}
              />
            )}
            <Typography variant="h4" sx={{ fontSize: { xs: '1.5rem', sm: '2.125rem' } }}>
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
                const participatingInGroup = gift.isGroupGift && isParticipating(gift);
                const reservedBySomeoneElse = gift.isTaken && !reservedByMe && !gift.isGroupGift;
                const canReserve = !isPastYear && (!gift.isTaken || (gift.isGroupGift && !participatingInGroup));

                return (
                  <ListItem
                    key={gift.id}
                    divider={index < gifts.length - 1}
                    sx={{
                      py: 2,
                      flexDirection: { xs: 'column', sm: 'row' },
                      alignItems: { xs: 'stretch', sm: 'flex-start' },
                      gap: { xs: 2, sm: 0 }
                    }}
                  >
                    <ListItemText
                      primary={
                        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, flexWrap: 'wrap' }}>
                          <Typography variant="h6" sx={{ fontSize: { xs: '1.1rem', sm: '1.25rem' } }}>{gift.name}</Typography>
                          {gift.isGroupGift && (
                            <Tooltip
                              title={gift.participantNames && gift.participantNames.length > 0
                                ? `Participants : ${gift.participantNames.join(', ')}`
                                : 'Cadeau groupé'}
                              arrow
                            >
                              <Chip
                                icon={<GroupIcon />}
                                label={`Cadeau groupé (${gift.participantCount} participant${gift.participantCount !== 1 ? 's' : ''})`}
                                color={isParticipating(gift) ? 'success' : 'warning'}
                                size="small"
                              />
                            </Tooltip>
                          )}
                          {gift.isTaken && !gift.isGroupGift && (
                            <Chip
                              icon={<CheckCircleIcon />}
                              label={reservedByMe ? 'Réservé par vous' : `Réservé par ${gift.takenByUserName || 'quelqu\'un'}`}
                              color={reservedByMe ? 'success' : 'warning'}
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
                            <Typography variant="body2" sx={{ fontWeight: 'bold', mb: 0.5 }}>
                              Prix: {gift.price.toFixed(2)} €
                            </Typography>
                          )}
                          {gift.comment && gift.isTaken && (
                            <Alert severity="info" sx={{ mt: 1 }}>
                              <strong>Commentaire :</strong>
                              <Box component="span" sx={{ whiteSpace: 'pre-wrap', display: 'block', mt: 0.5 }}>
                                {gift.comment}
                              </Box>
                            </Alert>
                          )}
                        </Box>
                      }
                    />
                    {!isPastYear && (
                      <Box sx={{
                        ml: { xs: 0, sm: 2 },
                        display: 'flex',
                        flexDirection: { xs: 'row', sm: 'column' },
                        gap: 1,
                        width: { xs: '100%', sm: 'auto' }
                      }}>
                        {reservedByMe || participatingInGroup ? (
                          <Button
                            variant="outlined"
                            color="error"
                            onClick={() => handleUnreserve(gift.id)}
                            size="small"
                            fullWidth
                            sx={{ minWidth: { xs: 'auto', sm: '100px' } }}
                          >
                            Annuler
                          </Button>
                        ) : canReserve ? (
                          <Button
                            variant="contained"
                            color={gift.isGroupGift ? 'secondary' : 'primary'}
                            onClick={() => handleOpenReserveDialog(gift)}
                            size="small"
                            fullWidth
                            startIcon={gift.isGroupGift ? <GroupIcon /> : <ShoppingCartIcon />}
                            sx={{ minWidth: { xs: 'auto', sm: '100px' } }}
                          >
                            {gift.isGroupGift ? 'Participer' : 'Réserver'}
                          </Button>
                        ) : reservedBySomeoneElse ? (
                          <Button
                            variant="contained"
                            color="secondary"
                            onClick={() => handleOpenReserveDialog(gift)}
                            size="small"
                            fullWidth
                            startIcon={<GroupIcon />}
                            sx={{ minWidth: { xs: 'auto', sm: '100px' } }}
                          >
                            Participer
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
      <Dialog
        open={openDialog}
        onClose={handleCloseDialog}
        maxWidth="sm"
        fullWidth
        fullScreen={window.innerWidth < 600}
      >
        <DialogTitle>
          {selectedGift?.isGroupGift || (selectedGift?.isTaken && !isReservedByMe(selectedGift))
            ? 'Participer au cadeau'
            : 'Confirmer la réservation'}
        </DialogTitle>
        <DialogContent sx={{ pt: { xs: 3, sm: 2 } }}>
          <Typography sx={{ mb: 2 }}>
            {selectedGift?.isGroupGift || (selectedGift?.isTaken && !isReservedByMe(selectedGift))
              ? `Voulez-vous participer au cadeau "${selectedGift?.name}" ?`
              : `Voulez-vous réserver le cadeau "${selectedGift?.name}" ?`}
          </Typography>
          {selectedGift?.isGroupGift && (
            <Alert severity="info" sx={{ mb: 2 }}>
              Il s'agit d'un cadeau groupé. Vous participerez à ce cadeau avec d'autres personnes.
            </Alert>
          )}
          {selectedGift?.isTaken && !selectedGift?.isGroupGift && !isReservedByMe(selectedGift) && (
            <Alert severity="info" sx={{ mb: 2 }}>
              Ce cadeau a déjà été réservé par <strong>{selectedGift?.takenByUserName}</strong>.
              En participant, vous créez automatiquement un cadeau groupé.
            </Alert>
          )}
          <TextField
            fullWidth
            multiline
            rows={3}
            label="Commentaire (optionnel)"
            placeholder="Ex: J'ai pris le tome 1 et 2"
            value={reserveComment}
            onChange={(e) => setReserveComment(e.target.value)}
            helperText="Ajoutez un commentaire pour informer les autres participants"
          />
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
