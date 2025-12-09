import { useState, MouseEvent } from 'react';
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
  HelpOutline as HelpIcon,
} from '@mui/icons-material';
import { useAuth } from '../contexts/AuthContext';
import Avatar from './Avatar';
import styles from '../css/NavigationBar.module.less';

interface NavigationBarProps {
  title?: string;
}

const NavigationBar = ({ title = 'Nawel - Listes de Noël' }: NavigationBarProps): JSX.Element => {
  const navigate = useNavigate();
  const { user, logout } = useAuth();
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down('md'));
  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);

  const handleLogout = (): void => {
    logout();
    navigate('/login');
  };

  const handleMenuOpen = (event: MouseEvent<HTMLElement>): void => {
    setAnchorEl(event.currentTarget);
  };

  const handleMenuClose = (): void => {
    setAnchorEl(null);
  };

  const handleNavigate = (path: string): void => {
    navigate(path);
    handleMenuClose();
  };

  return (
    <AppBar position="static">
      <Toolbar>
        <Typography
          variant="h6"
          component="div"
          className={styles.title}
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
                <HomeIcon className={styles.menuIcon} /> Accueil
              </MenuItem>
              <MenuItem onClick={() => handleNavigate('/my-list')}>
                <ListIcon className={styles.menuIcon} /> Ma liste
              </MenuItem>
              <MenuItem onClick={() => handleNavigate('/cart')}>
                <CartIcon className={styles.menuIcon} /> Mon panier
              </MenuItem>
              <MenuItem onClick={() => handleNavigate('/profile')}>
                <PersonIcon className={styles.menuIcon} /> Mon profil
              </MenuItem>
              {user?.isAdmin && (
                <MenuItem onClick={() => handleNavigate('/admin')}>
                  <AdminIcon className={styles.menuIcon} /> Administration
                </MenuItem>
              )}
              <MenuItem onClick={() => handleNavigate('/help')}>
                <HelpIcon className={styles.menuIcon} /> Aide
              </MenuItem>
              <MenuItem onClick={handleLogout}>
                <LogoutIcon className={styles.menuIcon} /> Déconnexion
              </MenuItem>
            </Menu>
          </>
        ) : (
          <Box className={styles.desktopNav}>
            <Box className={styles.userInfo}>
              <Avatar user={user} size={32} />
              <Typography variant="body1">
                {user?.firstName || user?.login}
              </Typography>
            </Box>
            <Button color="inherit" onClick={() => navigate('/')} className={styles.navButton}>
              Accueil
            </Button>
            <Button color="inherit" onClick={() => navigate('/my-list')} className={styles.navButton}>
              Ma liste
            </Button>
            <Button color="inherit" onClick={() => navigate('/cart')} className={styles.navButton}>
              Mon panier
            </Button>
            <Button color="inherit" onClick={() => navigate('/profile')} className={styles.navButton}>
              Mon profil
            </Button>
            {user?.isAdmin && (
              <Button color="inherit" onClick={() => navigate('/admin')} className={styles.navButton}>
                Administration
              </Button>
            )}
            <Button color="inherit" onClick={() => navigate('/help')} className={styles.navButton}>
              Aide
            </Button>
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
