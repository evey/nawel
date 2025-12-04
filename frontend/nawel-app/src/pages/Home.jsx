import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Container,
  Box,
  Typography,
  Card,
  CardContent,
  CardActionArea,
  Avatar,
  Grid,
  Paper,
  CircularProgress,
  Alert,
} from '@mui/material';
import NavigationBar from '../components/NavigationBar';
import { listsAPI } from '../services/api';

const Home = () => {
  const navigate = useNavigate();
  const [listsByFamily, setListsByFamily] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    fetchLists();
  }, []);

  const fetchLists = async () => {
    try {
      setLoading(true);
      const response = await listsAPI.getAll();
      setListsByFamily(response.data.families);
      setError('');
    } catch (err) {
      console.error('Error fetching lists:', err);
      setError('Erreur lors du chargement des listes');
    } finally {
      setLoading(false);
    }
  };

  const handleListClick = (userId) => {
    navigate(`/list/${userId}`);
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

      <Container maxWidth="lg" sx={{ mt: 4, mb: 4 }}>
        <Typography variant="h4" gutterBottom>
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
              <Typography variant="h5" gutterBottom sx={{ mb: 2 }}>
                Famille {family.familyName}
              </Typography>

              <Grid container spacing={2}>
                {family.lists.map((list) => (
                  <Grid item xs={12} sm={6} md={4} lg={3} key={list.userId}>
                    <Card>
                      <CardActionArea onClick={() => handleListClick(list.userId)}>
                        <CardContent sx={{ textAlign: 'center' }}>
                          <Avatar
                            src={list.avatarUrl}
                            alt={list.userName}
                            sx={{ width: 80, height: 80, margin: '0 auto 16px' }}
                          >
                            {!list.avatarUrl && list.userName.charAt(0).toUpperCase()}
                          </Avatar>
                          <Typography variant="h6" component="div">
                            {list.userName}
                          </Typography>
                          <Typography variant="body2" color="text.secondary">
                            {list.giftCount} cadeau{list.giftCount !== 1 ? 'x' : ''}
                          </Typography>
                        </CardContent>
                      </CardActionArea>
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
