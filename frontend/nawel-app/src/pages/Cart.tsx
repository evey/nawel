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
import { isParticipating } from '../utils/giftHelpers';
import type { Gift } from '../types';
import styles from '../css/Cart.module.less';
import commonStyles from '../css/common.module.less';

interface ExtendedGift extends Gift {
  ownerName: string;
  familyName: string;
}

const Cart = (): JSX.Element => {
  const navigate = useNavigate();
  const { user } = useAuth();
  const [reservedGifts, setReservedGifts] = useState<ExtendedGift[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string>('');

  useEffect(() => {
    fetchReservedGifts();
  }, []);

  const fetchReservedGifts = async (): Promise<void> => {
    try {
      setLoading(true);

      // Get all lists to fetch all gifts
      const listsResponse = await listsAPI.getAll();
      const allGifts: ExtendedGift[] = [];

      // For each user in each family, fetch their gifts
      for (const family of listsResponse.data.families) {
        for (const userList of family.lists) {
          try {
            const giftsResponse = await giftsAPI.getUserGifts(userList.userId, new Date().getFullYear());
            // Filter gifts reserved by current user
            const myReservedGifts = giftsResponse.data
              .filter(gift => {
                if (gift.isGroupGift) {
                  // For group gifts, check if user is participating
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

  const handleUnreserve = async (giftId: number): Promise<void> => {
    if (!window.confirm('Êtes-vous sûr de vouloir annuler cette réservation ?')) {
      return;
    }

    try {
      await giftsAPI.unreserveGift(giftId);
      await fetchReservedGifts();
      setError('');
    } catch (err: any) {
      console.error('Error unreserving gift:', err);
      setError(err.response?.data?.message || 'Erreur lors de l\'annulation');
    }
  };

  const calculateTotal = (): number => {
    return reservedGifts
      .filter(gift => gift.price)
      .reduce((sum, gift) => {
        if (gift.isGroupGift && gift.participantCount > 0) {
          // For group gifts, divide by number of participants
          return sum + (gift.price! / gift.participantCount);
        }
        return sum + gift.price!;
      }, 0);
  };

  if (loading) {
    return (
      <Box className={commonStyles.loadingContainer}>
        <CircularProgress />
      </Box>
    );
  }

  const total = calculateTotal();

  return (
    <Box className={commonStyles.pageRoot}>
      <NavigationBar title="Mon panier" />

      <Container maxWidth="md" className={commonStyles.pageContainer}>
        <Box className={styles.header}>
          <Typography variant="h4" className={styles.title}>
            Mes réservations
          </Typography>
          <Chip
            label={`${reservedGifts.length} cadeau${reservedGifts.length !== 1 ? 'x' : ''}`}
            color="primary"
            size="medium"
          />
        </Box>

        {error && (
          <Alert severity="error" className={styles.alert}>
            {error}
          </Alert>
        )}

        {reservedGifts.length === 0 ? (
          <Card>
            <CardContent className={styles.emptyState}>
              <Typography variant="body1" color="text.secondary">
                Vous n'avez pas encore réservé de cadeaux.
              </Typography>
              <Button
                variant="contained"
                onClick={() => navigate('/')}
                className={styles.emptyStateButton}
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
                    className={styles.listItem}
                  >
                    <ListItemText
                      className={styles.listItemText}
                      primary={
                        <Box className={styles.giftTitleContainer}>
                          <Typography variant="h6" className={styles.giftTitle}>{gift.name}</Typography>
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
                                color={isParticipating(gift, user) ? 'success' : 'warning'}
                                size="small"
                              />
                            </Tooltip>
                          )}
                        </Box>
                      }
                      secondary={
                        <Box className={styles.secondaryInfo}>
                          <Typography variant="body2" color="text.secondary" className={styles.ownerName}>
                            Pour: {gift.ownerName} ({gift.familyName})
                          </Typography>
                          {gift.description && (
                            <Typography variant="body2" color="text.secondary" className={styles.description}>
                              {gift.description}
                            </Typography>
                          )}
                          {gift.url && (
                            <Typography variant="body2" className={styles.link}>
                              <a href={gift.url} target="_blank" rel="noopener noreferrer">
                                Voir le lien
                              </a>
                            </Typography>
                          )}
                          {gift.price && (
                            <Typography variant="body2" className={styles.price}>
                              {gift.isGroupGift && gift.participantCount > 0
                                ? `Votre part: ${(gift.price / gift.participantCount).toFixed(2)} € (Total: ${gift.price.toFixed(2)} €)`
                                : `Prix: ${gift.price.toFixed(2)} €`}
                            </Typography>
                          )}
                        </Box>
                      }
                    />
                    <Box className={styles.actions}>
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
              <Card className={styles.totalCard}>
                <CardContent>
                  <Box className={styles.totalContent}>
                    <Typography variant="h5" className={styles.totalLabel}>
                      Total estimé:
                    </Typography>
                    <Typography variant="h4" className={styles.totalAmount}>
                      {total.toFixed(2)} €
                    </Typography>
                  </Box>
                  <Typography variant="body2" className={styles.totalDisclaimer}>
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
