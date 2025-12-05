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
  Grid,
  Paper,
  CircularProgress,
  Alert,
} from '@mui/material';
import { Edit as EditIcon, List as ListIcon } from '@mui/icons-material';
import NavigationBar from '../components/NavigationBar';
import Avatar from '../components/Avatar';
import { listsAPI } from '../services/api';
import { useAuth } from '../contexts/AuthContext';

const Home = () => {
  const navigate = useNavigate();
  const { user, startManagingChild, managingChild } = useAuth();
  const [listsByFamily, setListsByFamily] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [hasLoaded, setHasLoaded] = useState(false);

  const fetchLists = useCallback(async () => {
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

  const handleListClick = (userId) => {
    navigate(`/list/${userId}`);
  };

  const handleManageChild = (childInfo) => {
    console.log('[Home] handleManageChild called with:', childInfo);
    console.log('[Home] Current user:', user);
    console.log('[Home] Current managingChild:', managingChild);

    // Vérifier que l'utilisateur connecté est adulte et de la même famille
    if (user?.isChildren) {
      console.log('[Home] User is a child, cannot manage');
      return; // Les enfants ne peuvent pas gérer d'autres listes
    }

    console.log('[Home] Calling startManagingChild...');
    startManagingChild({
      userId: childInfo.userId,
      userName: childInfo.userName,
      avatarUrl: childInfo.avatarUrl
    });
    console.log('[Home] Navigating to /my-list...');
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
    <Box sx={{ flexGrow: 1 }}>
      <NavigationBar title="Nawel - Listes de Noël" />

      <Container maxWidth="lg" sx={{ mt: { xs: 2, sm: 4 }, mb: 4, px: { xs: 2, sm: 3 } }}>
        <Typography variant="h4" gutterBottom sx={{ fontSize: { xs: '1.5rem', sm: '2.125rem' } }}>
          Listes de Noël
        </Typography>

        {error && (
          <Alert severity="error" sx={{ mb: 3 }}>
            {error}
          </Alert>
        )}

        {listsByFamily.length === 0 ? (
          <Paper sx={{ p: 3, textAlign: 'center' }}>
            <Typography variant="body1" color="text.secondary">
              Aucune liste disponible pour le moment
            </Typography>
          </Paper>
        ) : (
          listsByFamily.map((family) => (
            <Box key={family.familyId} sx={{ mb: 4 }}>
              <Typography variant="h5" gutterBottom sx={{ mb: 2, fontSize: { xs: '1.25rem', sm: '1.5rem' } }}>
                Famille {family.familyName}
              </Typography>

              <Grid container spacing={{ xs: 2, sm: 3, md: 4 }}>
                {family.lists.map((list) => (
                  <Grid item xs={6} sm={6} md={4} lg={4} key={list.userId}>
                    <Card sx={{ display: 'flex', flexDirection: 'column', height: '100%' }}>
                      {!list.isChildren ? (
                        <CardActionArea onClick={() => handleListClick(list.userId)} sx={{ flexGrow: 1 }}>
                          <CardContent sx={{ textAlign: 'center', py: { xs: 2, sm: 3 }, px: { xs: 2, sm: 3, md: 4 } }}>
                            <Box sx={{ display: 'flex', justifyContent: 'center', mb: { xs: 1.5, sm: 2 } }}>
                              <Avatar
                                user={{
                                  avatar: list.avatarUrl,
                                  firstName: list.userName,
                                  login: list.userName
                                }}
                                size={100}
                                sx={{
                                  width: { xs: 80, sm: 100 },
                                  height: { xs: 80, sm: 100 },
                                  fontSize: { xs: 32, sm: 40 }
                                }}
                              />
                            </Box>
                            <Typography variant="h6" component="div" sx={{ fontSize: { xs: '1rem', sm: '1.25rem' } }}>
                              {list.userName}
                            </Typography>
                            <Typography variant="body2" color="text.secondary" sx={{ fontSize: { xs: '0.75rem', sm: '0.875rem' } }}>
                              {list.giftCount} cadeau{list.giftCount !== 1 ? 'x' : ''}
                            </Typography>
                          </CardContent>
                        </CardActionArea>
                      ) : (
                        <>
                          <CardContent sx={{ textAlign: 'center', py: { xs: 2, sm: 3 }, px: { xs: 2, sm: 3, md: 4 }, flexGrow: 1 }}>
                            <Box sx={{ display: 'flex', justifyContent: 'center', mb: { xs: 1.5, sm: 2 } }}>
                              <Avatar
                                user={{
                                  avatar: list.avatarUrl,
                                  firstName: list.userName,
                                  login: list.userName
                                }}
                                size={100}
                                sx={{
                                  width: { xs: 80, sm: 100 },
                                  height: { xs: 80, sm: 100 },
                                  fontSize: { xs: 32, sm: 40 }
                                }}
                              />
                            </Box>
                            <Typography variant="h6" component="div" sx={{ fontSize: { xs: '1rem', sm: '1.25rem' } }}>
                              {list.userName}
                            </Typography>
                            <Typography variant="body2" color="text.secondary" sx={{ fontSize: { xs: '0.75rem', sm: '0.875rem' } }}>
                              {list.giftCount} cadeau{list.giftCount !== 1 ? 'x' : ''}
                            </Typography>
                            <Typography variant="caption" color="primary" sx={{ display: 'block', mt: 0.5, fontWeight: 500 }}>
                              👶 Enfant
                            </Typography>
                          </CardContent>
                          <CardActions sx={{ justifyContent: 'center', pb: 2, px: 2 }}>
                            <Button
                              size="small"
                              startIcon={<EditIcon />}
                              variant="contained"
                              color="secondary"
                              onClick={() => handleManageChild(list)}
                              sx={{ mr: 1 }}
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
