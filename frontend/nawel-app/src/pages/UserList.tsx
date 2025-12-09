import { useState, useEffect, ChangeEvent, useCallback } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import {
  Container,
  Box,
  Typography,
  Card,
  CardContent,
  CircularProgress,
  Alert,
  List,
  Select,
  MenuItem,
  FormControl,
  InputLabel,
  Chip,
  SelectChangeEvent,
} from '@mui/material';
import NavigationBar from '../components/NavigationBar';
import Avatar from '../components/Avatar';
import UserGiftListItem from '../components/userlist/UserGiftListItem';
import ReserveDialog from '../components/userlist/ReserveDialog';
import { useAuth } from '../contexts/AuthContext';
import { giftsAPI, usersAPI } from '../services/api';
import type { Gift, User } from '../types';
import styles from '../css/UserList.module.less';
import commonStyles from '../css/common.module.less';

interface ListOwner {
  name: string;
  avatar: string;
  firstName: string;
  login: string;
}

const UserList = (): JSX.Element => {
  const navigate = useNavigate();
  const { userId } = useParams<{ userId: string }>();
  const { user } = useAuth();
  const [gifts, setGifts] = useState<Gift[]>([]);
  const [listOwner, setListOwner] = useState<ListOwner | null>(null);
  const [availableYears, setAvailableYears] = useState<number[]>([]);
  const [selectedYear, setSelectedYear] = useState<number>(new Date().getFullYear());
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string>('');
  const [selectedGift, setSelectedGift] = useState<Gift | null>(null);
  const [openDialog, setOpenDialog] = useState<boolean>(false);
  const [reserveComment, setReserveComment] = useState<string>('');

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

  const fetchInitialData = async (): Promise<void> => {
    try {
      setLoading(true);

      // Fetch user info
      const userResponse = await usersAPI.getById(parseInt(userId!));
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
      const giftsResponse = await giftsAPI.getUserGifts(parseInt(userId!), currentYear);
      setGifts(giftsResponse.data);

      // For now, just show current year. In a real app, we'd have an endpoint to get all years
      const years = [currentYear];
      // Check if there are older years by trying previous years
      for (let year = currentYear - 1; year >= currentYear - 3; year--) {
        try {
          const oldGiftsResponse = await giftsAPI.getUserGifts(parseInt(userId!), year);
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

  const fetchUserAndGifts = async (): Promise<void> => {
    try {
      setLoading(true);

      // Fetch user info
      const userResponse = await usersAPI.getById(parseInt(userId!));
      setListOwner({
        name: userResponse.data.firstName || userResponse.data.login,
        avatar: userResponse.data.avatar,
        firstName: userResponse.data.firstName,
        login: userResponse.data.login
      });

      // Fetch gifts
      const giftsResponse = await giftsAPI.getUserGifts(parseInt(userId!), selectedYear);
      setGifts(giftsResponse.data);

      setError('');
    } catch (err) {
      console.error('Error fetching data:', err);
      setError('Erreur lors du chargement de la liste');
    } finally {
      setLoading(false);
    }
  };

  const handleYearChange = useCallback((event: SelectChangeEvent<number>): void => {
    setSelectedYear(event.target.value as number);
  }, []);

  const handleOpenReserveDialog = useCallback((gift: Gift): void => {
    setSelectedGift(gift);
    setReserveComment(gift.comment || '');
    setOpenDialog(true);
  }, []);

  const handleCloseDialog = useCallback((): void => {
    setOpenDialog(false);
    setSelectedGift(null);
    setReserveComment('');
  }, []);

  const handleReserve = useCallback(async (): Promise<void> => {
    if (!selectedGift) return;

    try {
      await giftsAPI.reserveGift(selectedGift.id, { comment: reserveComment });
      await fetchUserAndGifts();
      handleCloseDialog();
      setError('');
    } catch (err: any) {
      console.error('Error reserving gift:', err);
      setError(err.response?.data?.message || 'Erreur lors de la réservation');
    }
  }, [selectedGift, reserveComment, handleCloseDialog]);

  const handleUnreserve = useCallback(async (giftId: number): Promise<void> => {
    if (!window.confirm('Êtes-vous sûr de vouloir annuler cette réservation ?')) {
      return;
    }

    try {
      await giftsAPI.unreserveGift(giftId);
      await fetchUserAndGifts();
      setError('');
    } catch (err: any) {
      console.error('Error unreserving gift:', err);
      setError(err.response?.data?.message || 'Erreur lors de l\'annulation');
    }
  }, []);

  const isReservedByMe = (gift: Gift): boolean => {
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
    <Box className={commonStyles.pageRoot}>
      <NavigationBar title={`Liste de ${listOwner?.name || 'cadeaux'}`} />

      <Container maxWidth="md" className={styles.container}>
        <Box className={styles.header}>
          <Box className={styles.headerLeft}>
            {listOwner && (
              <Avatar
                user={{
                  id: parseInt(userId || '0'),
                  login: listOwner.login,
                  email: '',
                  firstName: listOwner.firstName,
                  lastName: '',
                  avatar: listOwner.avatar,
                  notifyListEdit: false,
                  notifyGiftTaken: false,
                  displayPopup: false,
                  isChildren: false,
                  isAdmin: false,
                  familyId: 0,
                  createdAt: '',
                  updatedAt: '',
                }}
                size={60}
                className={styles.headerAvatar}
              />
            )}
            <Typography variant="h4" className={styles.title}>
              Cadeaux de {listOwner?.name || 'l\'utilisateur'}
            </Typography>
            {availableYears.length > 1 && (
              <FormControl size="small" className={styles.yearSelect}>
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
          <Alert severity="error" className={styles.alertContainer}>
            {error}
          </Alert>
        )}

        {loading ? (
          <Box className={styles.loadingContainer}>
            <CircularProgress />
          </Box>
        ) : gifts.length === 0 ? (
          <Card>
            <CardContent className={styles.emptyState}>
              <Typography variant="body1" color="text.secondary">
                Aucun cadeau n'a été ajouté à cette liste pour {selectedYear}.
              </Typography>
            </CardContent>
          </Card>
        ) : (
          <Card>
            <List>
              {gifts.map((gift, index) => (
                <UserGiftListItem
                  key={gift.id}
                  gift={gift}
                  index={index}
                  totalGifts={gifts.length}
                  currentUser={user}
                  isPastYear={isPastYear}
                  onReserve={handleOpenReserveDialog}
                  onUnreserve={handleUnreserve}
                />
              ))}
            </List>
          </Card>
        )}
      </Container>

      <ReserveDialog
        open={openDialog}
        gift={selectedGift}
        reserveComment={reserveComment}
        isReservedByMe={selectedGift ? isReservedByMe(selectedGift) : false}
        onClose={handleCloseDialog}
        onCommentChange={(e: ChangeEvent<HTMLInputElement>) => setReserveComment(e.target.value)}
        onConfirm={handleReserve}
      />
    </Box>
  );
};

export default UserList;
