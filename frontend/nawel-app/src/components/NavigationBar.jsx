import { useNavigate } from 'react-router-dom';
import { AppBar, Toolbar, Typography, Button } from '@mui/material';
import { useAuth } from '../contexts/AuthContext';

const NavigationBar = ({ title = 'Nawel - Listes de Noël' }) => {
  const navigate = useNavigate();
  const { user, logout } = useAuth();

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  return (
    <AppBar position="static">
      <Toolbar>
        <Typography variant="h6" component="div" sx={{ flexGrow: 1 }}>
          {title}
        </Typography>
        <Typography variant="body1" sx={{ mr: 2 }}>
          Bonjour, {user?.firstName || user?.login}
        </Typography>
        <Button color="inherit" onClick={() => navigate('/')} sx={{ mr: 1 }}>
          Accueil
        </Button>
        <Button color="inherit" onClick={() => navigate('/my-list')} sx={{ mr: 1 }}>
          Ma liste
        </Button>
        <Button color="inherit" onClick={() => navigate('/cart')} sx={{ mr: 1 }}>
          Mon panier
        </Button>
        <Button color="inherit" onClick={() => navigate('/profile')} sx={{ mr: 1 }}>
          Mon profil
        </Button>
        <Button color="inherit" onClick={handleLogout}>
          Déconnexion
        </Button>
      </Toolbar>
    </AppBar>
  );
};

export default NavigationBar;
