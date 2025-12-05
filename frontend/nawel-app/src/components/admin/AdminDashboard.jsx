import { useState, useEffect } from 'react';
import {
  Box,
  Grid,
  Card,
  CardContent,
  Typography,
  CircularProgress,
  Paper,
} from '@mui/material';
import {
  People as PeopleIcon,
  FamilyRestroom as FamilyIcon,
  CardGiftcard as GiftIcon,
  CheckCircle as CheckCircleIcon,
  Api as ApiIcon,
} from '@mui/icons-material';
import { adminAPI } from '../../services/api';
import {
  LineChart,
  Line,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  Legend,
  ResponsiveContainer,
} from 'recharts';

const StatCard = ({ title, value, icon: Icon, color }) => (
  <Card>
    <CardContent>
      <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
        <Box>
          <Typography color="text.secondary" gutterBottom variant="body2">
            {title}
          </Typography>
          <Typography variant="h4">
            {value}
          </Typography>
        </Box>
        <Icon sx={{ fontSize: 40, color }} />
      </Box>
    </CardContent>
  </Card>
);

const AdminDashboard = ({ setError }) => {
  const [loading, setLoading] = useState(true);
  const [stats, setStats] = useState(null);

  useEffect(() => {
    fetchStats();
  }, []);

  const fetchStats = async () => {
    try {
      setLoading(true);
      const response = await adminAPI.getStats();
      setStats(response.data);
      setError('');
    } catch (err) {
      console.error('Error fetching stats:', err);
      setError('Erreur lors du chargement des statistiques');
    } finally {
      setLoading(false);
    }
  };

  if (loading) {
    return (
      <Box display="flex" justifyContent="center" py={4}>
        <CircularProgress />
      </Box>
    );
  }

  if (!stats) {
    return null;
  }

  // Préparer les données pour le graphique
  const chartData = stats.requestsByMonth.map((item) => ({
    name: `${item.month}/${item.year}`,
    'Requêtes totales': item.count,
    'Requêtes réussies': item.successCount,
    'Requêtes échouées': item.count - item.successCount,
  }));

  return (
    <Box>
      <Grid container spacing={3}>
        {/* Stats cards */}
        <Grid item xs={12} sm={6} md={4}>
          <StatCard
            title="Utilisateurs"
            value={stats.totalUsers}
            icon={PeopleIcon}
            color="primary.main"
          />
        </Grid>
        <Grid item xs={12} sm={6} md={4}>
          <StatCard
            title="Familles"
            value={stats.totalFamilies}
            icon={FamilyIcon}
            color="secondary.main"
          />
        </Grid>
        <Grid item xs={12} sm={6} md={4}>
          <StatCard
            title="Cadeaux cette année"
            value={stats.totalGifts}
            icon={GiftIcon}
            color="success.main"
          />
        </Grid>
        <Grid item xs={12} sm={6} md={4}>
          <StatCard
            title="Cadeaux réservés"
            value={stats.totalReservedGifts}
            icon={CheckCircleIcon}
            color="info.main"
          />
        </Grid>
        <Grid item xs={12} sm={6} md={4}>
          <StatCard
            title="Requêtes OpenGraph ce mois"
            value={stats.openGraphRequestsThisMonth}
            icon={ApiIcon}
            color="warning.main"
          />
        </Grid>

        {/* Chart */}
        <Grid item xs={12}>
          <Paper sx={{ p: 3 }}>
            <Typography variant="h6" gutterBottom>
              Utilisation de l'API OpenGraph.io (12 derniers mois)
            </Typography>
            <ResponsiveContainer width="100%" height={300}>
              <LineChart data={chartData}>
                <CartesianGrid strokeDasharray="3 3" />
                <XAxis dataKey="name" />
                <YAxis />
                <Tooltip />
                <Legend />
                <Line
                  type="monotone"
                  dataKey="Requêtes totales"
                  stroke="#8884d8"
                  strokeWidth={2}
                />
                <Line
                  type="monotone"
                  dataKey="Requêtes réussies"
                  stroke="#82ca9d"
                  strokeWidth={2}
                />
                <Line
                  type="monotone"
                  dataKey="Requêtes échouées"
                  stroke="#ff7c7c"
                  strokeWidth={2}
                />
              </LineChart>
            </ResponsiveContainer>
            <Typography variant="body2" color="text.secondary" sx={{ mt: 2 }}>
              Total des requêtes sur 12 mois : {stats.requestsByMonth.reduce((acc, item) => acc + item.count, 0)}
            </Typography>
          </Paper>
        </Grid>
      </Grid>
    </Box>
  );
};

export default AdminDashboard;
