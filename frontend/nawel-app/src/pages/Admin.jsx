import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Container,
  Box,
  Typography,
  Tabs,
  Tab,
  Paper,
  Alert,
} from '@mui/material';
import {
  Dashboard as DashboardIcon,
  People as PeopleIcon,
  FamilyRestroom as FamilyIcon,
} from '@mui/icons-material';
import NavigationBar from '../components/NavigationBar';
import { useAuth } from '../contexts/AuthContext';
import AdminDashboard from '../components/admin/AdminDashboard';
import AdminUsers from '../components/admin/AdminUsers';
import AdminFamilies from '../components/admin/AdminFamilies';

const Admin = () => {
  const navigate = useNavigate();
  const { user } = useAuth();
  const [currentTab, setCurrentTab] = useState(0);
  const [error, setError] = useState('');

  useEffect(() => {
    // Check if user is admin (user ID 1)
    if (user?.id !== 1) {
      navigate('/');
    }
  }, [user, navigate]);

  const handleTabChange = (event, newValue) => {
    setCurrentTab(newValue);
  };

  if (user?.id !== 1) {
    return null;
  }

  return (
    <Box sx={{ flexGrow: 1 }}>
      <NavigationBar title="Administration" />

      <Container maxWidth="xl" sx={{ mt: { xs: 2, sm: 4 }, mb: 4, px: { xs: 2, sm: 3 } }}>
        <Typography variant="h4" gutterBottom sx={{ fontSize: { xs: '1.5rem', sm: '2.125rem' } }}>
          Panneau d'administration
        </Typography>

        {error && (
          <Alert severity="error" sx={{ mb: 3 }} onClose={() => setError('')}>
            {error}
          </Alert>
        )}

        <Paper sx={{ width: '100%', mb: 2 }}>
          <Tabs
            value={currentTab}
            onChange={handleTabChange}
            variant="scrollable"
            scrollButtons="auto"
            sx={{ borderBottom: 1, borderColor: 'divider' }}
          >
            <Tab icon={<DashboardIcon />} label="Tableau de bord" iconPosition="start" />
            <Tab icon={<PeopleIcon />} label="Utilisateurs" iconPosition="start" />
            <Tab icon={<FamilyIcon />} label="Familles" iconPosition="start" />
          </Tabs>

          <Box sx={{ p: { xs: 2, sm: 3 } }}>
            {currentTab === 0 && <AdminDashboard setError={setError} />}
            {currentTab === 1 && <AdminUsers setError={setError} />}
            {currentTab === 2 && <AdminFamilies setError={setError} />}
          </Box>
        </Paper>
      </Container>
    </Box>
  );
};

export default Admin;
