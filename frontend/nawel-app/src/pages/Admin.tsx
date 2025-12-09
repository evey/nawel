import { useState, useEffect, SyntheticEvent } from 'react';
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
import styles from '../css/Admin.module.less';

const Admin = (): JSX.Element | null => {
  const navigate = useNavigate();
  const { user } = useAuth();
  const [currentTab, setCurrentTab] = useState<number>(0);
  const [error, setError] = useState<string>('');

  useEffect(() => {
    // Check if user is admin
    if (!user?.isAdmin) {
      navigate('/');
    }
  }, [user, navigate]);

  const handleTabChange = (_event: SyntheticEvent, newValue: number): void => {
    setCurrentTab(newValue);
  };

  if (!user?.isAdmin) {
    return null;
  }

  return (
    <Box className={styles.pageRoot}>
      <NavigationBar title="Administration" />

      <Container maxWidth="xl" className={styles.container}>
        <Typography variant="h4" gutterBottom className={styles.title}>
          Panneau d'administration
        </Typography>

        {error && (
          <Alert severity="error" className={styles.alert} onClose={() => setError('')}>
            {error}
          </Alert>
        )}

        <Paper className={styles.paper}>
          <Tabs
            value={currentTab}
            onChange={handleTabChange}
            variant="scrollable"
            scrollButtons="auto"
            className={styles.tabs}
            sx={{ borderColor: 'divider' }}
          >
            <Tab icon={<DashboardIcon />} label="Tableau de bord" iconPosition="start" />
            <Tab icon={<PeopleIcon />} label="Utilisateurs" iconPosition="start" />
            <Tab icon={<FamilyIcon />} label="Familles" iconPosition="start" />
          </Tabs>

          <Box className={styles.tabContent}>
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
