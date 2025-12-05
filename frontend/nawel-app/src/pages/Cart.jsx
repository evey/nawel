import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
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
  Divider,
  IconButton,
  Tooltip,
} from '@mui/material';
import {
  Group as GroupIcon,
  Delete as DeleteIcon,
} from '@mui/icons-material';
import NavigationBar from '../components/NavigationBar';
import { useAuth } from '../contexts/AuthContext';
import { giftsAPI, listsAPI } from '../services/api';

const Cart = () => {
  const navigate = useNavigate();
  const { user } = useAuth();
  const [reservedGifts, setReservedGifts] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    fetchReservedGifts();
  }, []);

  const fetchReservedGifts = async () => {
    try {
      setLoading(true);

      // Get all lists to fetch all gifts
      const listsResponse = await listsAPI.getAll();
      const allGifts = [];

      // For each user in each family, fetch their gifts
      for (const family of listsResponse.data.families) {
        for (const userList of family.lists) {
          try {
            const giftsResponse = await giftsAPI.getUserGifts(userList.userId);
            // Filter gifts reserved by current user
            const myReservedGifts = giftsResponse.data
              .filter(gift => {
                if (gift.isGroupGift) {
                  // For group gifts, check if user is participating
                  // This is a simplified check - in reality we'd need participant details
                  return gift.participantCount > 0;
                }
                return gift.isTaken && gift.takenByUserId === user?.id;
              })
              .map(gift => ({
                ...gift,
                ownerName: userList.userName,
                familyName: family.familyName,
              }));
            allGifts.push(...myReservedGifts);
          } catch (err) {
            console.error(`Error fetching gifts for user ${userList.userId}:`, err);
          }
        }
      }

      setReservedGifts(allGifts);
      setError('');
    } catch (err) {
      console.error('Error fetching reserved gifts:', err);
      setError('Erreur lors du chargement du panier');
    } finally {
      setLoading(false);
    }
  };

  const handleUnreserve = async (giftId) => {
    if (!window.confirm('Êtes-vous sûr de vouloir annuler cette réservation ?')) {
      return;
    }

    try {
      await giftsAPI.unreserveGift(giftId);
      await fetchReservedGifts();
      setError('');
    } catch (err) {
      console.error('Error unreserving gift:', err);
      setError(err.response?.data?.message || 'Erreur lors de l\'annulation');
    }
  };

  const isParticipating = (gift) => {
    if (!gift.isGroupGift || !gift.participantNames) return false;
    const userDisplayName = user?.firstName || user?.login;
    return gift.participantNames.includes(userDisplayName);
  };

  const calculateTotal = () => {
    return reservedGifts
      .filter(gift => gift.price)
      .reduce((sum, gift) => {
        if (gift.isGroupGift && gift.participantCount > 0) {
          // For group gifts, divide by number of participants
          return sum + (gift.price / gift.participantCount);
        }
        return sum + gift.price;
      }, 0);
  };

  if (loading) {
    return (
      <Box display="flex" justifyContent="center" alignItems="center" minHeight="100vh">
        <CircularProgress />
      </Box>
    );
  }

  const total = calculateTotal();

  return (
    <Box sx={{ flexGrow: 1 }}>
      <NavigationBar title="Mon panier" />

      <Container maxWidth="md" sx={{ mt: { xs: 2, sm: 4 }, mb: 4, px: { xs: 2, sm: 3 } }}>
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3, flexWrap: 'wrap', gap: 2 }}>
          <Typography variant="h4" sx={{ fontSize: { xs: '1.5rem', sm: '2.125rem' } }}>
            Mes réservations
          </Typography>
          <Chip
            label={`${reservedGifts.length} cadeau${reservedGifts.length !== 1 ? 'x' : ''}`}
            color="primary"
            size="large"
          />
        </Box>

        {error && (
          <Alert severity="error" sx={{ mb: 3 }}>
            {error}
          </Alert>
        )}

        {reservedGifts.length === 0 ? (
          <Card>
            <CardContent sx={{ textAlign: 'center', py: 4 }}>
              <Typography variant="body1" color="text.secondary">
                Vous n'avez pas encore réservé de cadeaux.
              </Typography>
              <Button
                variant="contained"
                onClick={() => navigate('/')}
                sx={{ mt: 2 }}
              >
                Voir les listes
              </Button>
            </CardContent>
          </Card>
        ) : (
          <>
            <Card>
              <List>
                {reservedGifts.map((gift, index) => (
                  <ListItem
                    key={gift.id}
                    divider={index < reservedGifts.length - 1}
                    sx={{
                      py: 2,
                      flexDirection: { xs: 'column', sm: 'row' },
                      alignItems: { xs: 'stretch', sm: 'flex-start' },
                      gap: { xs: 2, sm: 0 }
                    }}
                  >
                    <ListItemText
                      sx={{ width: '100%' }}
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
                        </Box>
                      }
                      secondary={
                        <Box sx={{ mt: 1 }}>
                          <Typography variant="body2" color="text.secondary" sx={{ fontWeight: 'bold' }}>
                            Pour: {gift.ownerName} ({gift.familyName})
                          </Typography>
                          {gift.description && (
                            <Typography variant="body2" color="text.secondary" sx={{ mt: 0.5 }}>
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
                              {gift.isGroupGift && gift.participantCount > 0
                                ? `Votre part: ${(gift.price / gift.participantCount).toFixed(2)} € (Total: ${gift.price.toFixed(2)} €)`
                                : `Prix: ${gift.price.toFixed(2)} €`}
                            </Typography>
                          )}
                        </Box>
                      }
                    />
                    <Box sx={{
                      display: 'flex',
                      justifyContent: { xs: 'flex-end', sm: 'flex-start' },
                      ml: { xs: 0, sm: 2 },
                      width: { xs: '100%', sm: 'auto' }
                    }}>
                      <IconButton
                        onClick={() => handleUnreserve(gift.id)}
                        color="error"
                        size="medium"
                      >
                        <DeleteIcon />
                      </IconButton>
                    </Box>
                  </ListItem>
                ))}
              </List>
            </Card>

            {total > 0 && (
              <Card sx={{ mt: 3, bgcolor: 'secondary.main', color: 'white' }}>
                <CardContent>
                  <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                    <Typography variant="h5" sx={{ color: 'white' }}>
                      Total estimé:
                    </Typography>
                    <Typography variant="h4" sx={{ fontWeight: 'bold', color: 'white' }}>
                      {total.toFixed(2)} €
                    </Typography>
                  </Box>
                  <Typography variant="body2" sx={{ mt: 1, opacity: 0.9 }}>
                    * Ce total est indicatif et inclut votre part des cadeaux groupés
                  </Typography>
                </CardContent>
              </Card>
            )}
          </>
        )}
      </Container>
    </Box>
  );
};

export default Cart;
