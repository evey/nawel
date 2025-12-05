import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  AppBar,
  Toolbar,
  Typography,
  Button,
  IconButton,
  Menu,
  MenuItem,
  Box,
  useMediaQuery,
  useTheme,
} from '@mui/material';
import {
  Menu as MenuIcon,
  Home as HomeIcon,
  List as ListIcon,
  ShoppingCart as CartIcon,
  Person as PersonIcon,
  Logout as LogoutIcon,
  AdminPanelSettings as AdminIcon,
} from '@mui/icons-material';
import { useAuth } from '../contexts/AuthContext';
import Avatar from './Avatar';

const NavigationBar = ({ title = 'Nawel - Listes de Noël' }) => {
  const navigate = useNavigate();
  const { user, logout } = useAuth();
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down('md'));
  const [anchorEl, setAnchorEl] = useState(null);

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  const handleMenuOpen = (event) => {
    setAnchorEl(event.currentTarget);
  };

  const handleMenuClose = () => {
    setAnchorEl(null);
  };

  const handleNavigate = (path) => {
    navigate(path);
    handleMenuClose();
  };

  return (
    <AppBar position="static">
      <Toolbar>
        <Typography
          variant="h6"
          component="div"
          sx={{
            flexGrow: 1,
            fontSize: { xs: '1rem', sm: '1.25rem' },
            overflow: 'hidden',
            textOverflow: 'ellipsis',
            whiteSpace: 'nowrap'
          }}
        >
          {title}
        </Typography>

        {isMobile ? (
          <>
            <IconButton
              color="inherit"
              onClick={handleMenuOpen}
              edge="end"
            >
              <MenuIcon />
            </IconButton>
            <Menu
              anchorEl={anchorEl}
              open={Boolean(anchorEl)}
              onClose={handleMenuClose}
              anchorOrigin={{
                vertical: 'top',
                horizontal: 'right',
              }}
              transformOrigin={{
                vertical: 'top',
                horizontal: 'right',
              }}
            >
              <MenuItem disabled>
                <Typography variant="body2" color="text.secondary">
                  {user?.firstName || user?.login}
                </Typography>
              </MenuItem>
              <MenuItem onClick={() => handleNavigate('/')}>
                <HomeIcon sx={{ mr: 1 }} /> Accueil
              </MenuItem>
              <MenuItem onClick={() => handleNavigate('/my-list')}>
                <ListIcon sx={{ mr: 1 }} /> Ma liste
              </MenuItem>
              <MenuItem onClick={() => handleNavigate('/cart')}>
                <CartIcon sx={{ mr: 1 }} /> Mon panier
              </MenuItem>
              <MenuItem onClick={() => handleNavigate('/profile')}>
                <PersonIcon sx={{ mr: 1 }} /> Mon profil
              </MenuItem>
              {user?.id === 1 && (
                <MenuItem onClick={() => handleNavigate('/admin')}>
                  <AdminIcon sx={{ mr: 1 }} /> Administration
                </MenuItem>
              )}
              <MenuItem onClick={handleLogout}>
                <LogoutIcon sx={{ mr: 1 }} /> Déconnexion
              </MenuItem>
            </Menu>
          </>
        ) : (
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
              <Avatar user={user} size={32} />
              <Typography variant="body1">
                {user?.firstName || user?.login}
              </Typography>
            </Box>
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
            {user?.id === 1 && (
              <Button color="inherit" onClick={() => navigate('/admin')} sx={{ mr: 1 }}>
                Administration
              </Button>
            )}
            <Button color="inherit" onClick={handleLogout}>
              Déconnexion
            </Button>
          </Box>
        )}
      </Toolbar>
    </AppBar>
  );
};

export default NavigationBar;
