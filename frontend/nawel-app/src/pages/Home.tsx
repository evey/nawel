import { useState, useEffect, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Container,
  Box,
  Typography,
  Card,
  CardContent,
  CardActionArea,
  CardActions,
  Button,
  Paper,
  CircularProgress,
  Alert,
  GridLegacy as Grid,
} from '@mui/material';
import { Edit as EditIcon, List as ListIcon } from '@mui/icons-material';
import NavigationBar from '../components/NavigationBar';
import Avatar from '../components/Avatar';
import { listsAPI } from '../services/api';
import { useAuth } from '../contexts/AuthContext';
import type { FamilyList, GiftList } from '../types';
import styles from '../css/Home.module.less';
import commonStyles from '../css/common.module.less';

const Home = (): JSX.Element => {
  const navigate = useNavigate();
  const { user, startManagingChild } = useAuth();
  const [listsByFamily, setListsByFamily] = useState<FamilyList[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string>('');
  const [hasLoaded, setHasLoaded] = useState<boolean>(false);

  const fetchLists = useCallback(async (): Promise<void> => {
    try {
      setLoading(true);
      const response = await listsAPI.getAll();
      setListsByFamily(response.data.families);
      setError('');
      setHasLoaded(true);
    } catch (err) {
      console.error('Error fetching lists:', err);
      setError('Erreur lors du chargement des listes');
      setHasLoaded(true);
    } finally {
      setLoading(false);
    }
  }, []);

  // Chargement des listes au montage du composant
  useEffect(() => {
    if (!hasLoaded) {
      fetchLists();
    }
  }, [hasLoaded, fetchLists]);

  const handleListClick = (userId: number): void => {
    navigate(`/list/${userId}`);
  };

  const handleManageChild = (childInfo: GiftList): void => {
    // Vérifier que l'utilisateur connecté est adulte et de la même famille
    if (user?.isChildren) {
      return; // Les enfants ne peuvent pas gérer d'autres listes
    }

    startManagingChild({
      userId: childInfo.userId,
      userName: childInfo.userName,
      avatarUrl: childInfo.avatarUrl
    });
    // Naviguer directement après avoir mis à jour le contexte
    navigate('/my-list');
  };

  if (loading) {
    return (
      <Box display="flex" justifyContent="center" alignItems="center" minHeight="100vh">
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Box className={commonStyles.pageRoot}>
      <NavigationBar title="Nawel - Listes de Noël" />

      <Container maxWidth="lg" className={styles.container}>
        <Typography variant="h4" gutterBottom className={styles.title}>
          Listes de Noël
        </Typography>

        {error && (
          <Alert severity="error" className={styles.alert}>
            {error}
          </Alert>
        )}

        {listsByFamily.length === 0 ? (
          <Paper className={styles.emptyState}>
            <Typography variant="body1" color="text.secondary">
              Aucune liste disponible pour le moment
            </Typography>
          </Paper>
        ) : (
          listsByFamily.map((family) => (
            <Box key={family.familyId} className={styles.familySection}>
              <Typography variant="h5" gutterBottom className={styles.familyTitle}>
                Famille {family.familyName}
              </Typography>

              <Grid container spacing={{ xs: 2, sm: 3, md: 4 }}>
                {family.lists.map((list: GiftList) => (
                  <Grid item xs={6} sm={6} md={4} lg={4} key={list.userId}>
                    <Card className={styles.userCard}>
                      {!list.isChildren ? (
                        <CardActionArea onClick={() => handleListClick(list.userId)} className={styles.cardActionArea}>
                          <CardContent className={styles.cardContent}>
                            <Box className={styles.avatarContainer}>
                              <Avatar
                                user={{
                                  id: list.userId,
                                  login: list.userName,
                                  email: '',
                                  firstName: list.userName,
                                  lastName: '',
                                  avatar: list.avatarUrl,
                                  familyId: family.familyId,
                                  familyName: family.familyName,
                                  isChildren: false,
                                  isAdmin: false,
                                  notifyListEdit: false,
                                  notifyGiftTaken: false,
                                  displayPopup: false,
                                  createdAt: '',
                                  updatedAt: '',
                                }}
                                size={100}
                              />
                            </Box>
                            <Typography variant="h6" component="div" className={styles.cardTitle}>
                              {list.userName}
                            </Typography>
                            <Typography variant="body2" color="text.secondary" className={styles.cardSubtitle}>
                              {list.giftCount} cadeau{list.giftCount !== 1 ? 'x' : ''}
                            </Typography>
                          </CardContent>
                        </CardActionArea>
                      ) : (
                        <>
                          <CardContent className={styles.cardContent}>
                            <Box className={styles.avatarContainer}>
                              <Avatar
                                user={{
                                  id: list.userId,
                                  login: list.userName,
                                  email: '',
                                  firstName: list.userName,
                                  lastName: '',
                                  avatar: list.avatarUrl,
                                  familyId: family.familyId,
                                  familyName: family.familyName,
                                  isChildren: true,
                                  isAdmin: false,
                                  notifyListEdit: false,
                                  notifyGiftTaken: false,
                                  displayPopup: false,
                                  createdAt: '',
                                  updatedAt: '',
                                }}
                                size={100}
                              />
                            </Box>
                            <Typography variant="h6" component="div" className={styles.cardTitle}>
                              {list.userName}
                            </Typography>
                            <Typography variant="body2" color="text.secondary" className={styles.cardSubtitle}>
                              {list.giftCount} cadeau{list.giftCount !== 1 ? 'x' : ''}
                            </Typography>
                            <Typography variant="caption" color="primary" className={styles.childBadge}>
                              👶 Enfant
                            </Typography>
                          </CardContent>
                          <CardActions className={styles.cardActions}>
                            <Button
                              size="small"
                              startIcon={<EditIcon />}
                              variant="contained"
                              color="secondary"
                              onClick={() => handleManageChild(list)}
                              className={styles.buttonSpacing}
                            >
                              Gérer
                            </Button>
                            <Button
                              size="small"
                              startIcon={<ListIcon />}
                              variant="outlined"
                              onClick={() => handleListClick(list.userId)}
                            >
                              Voir
                            </Button>
                          </CardActions>
                        </>
                      )}
                    </Card>
                  </Grid>
                ))}
              </Grid>
            </Box>
          ))
        )}
      </Container>
    </Box>
  );
};

export default Home;
