import { useState, useEffect } from 'react';
import {
  Box,
  GridLegacy as Grid,
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
import { SvgIconComponent } from '@mui/icons-material';
import { adminAPI, AdminStats } from '../../services/api';
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
import styles from '../../css/AdminDashboard.module.less';

interface StatCardProps {
  title: string;
  value: number;
  icon: SvgIconComponent;
  color: string;
}

const StatCard = ({ title, value, icon: Icon, color }: StatCardProps): JSX.Element => (
  <Card>
    <CardContent>
      <Box className={styles.statCardHeader}>
        <Box>
          <Typography color="text.secondary" gutterBottom variant="body2">
            {title}
          </Typography>
          <Typography variant="h4">
            {value}
          </Typography>
        </Box>
        <Icon className={styles.statIcon} sx={{ color }} />
      </Box>
    </CardContent>
  </Card>
);

interface AdminDashboardProps {
  setError: (error: string) => void;
}

interface ChartData {
  name: string;
  'Requêtes totales': number;
  'Requêtes réussies': number;
  'Requêtes échouées': number;
}

const AdminDashboard = ({ setError }: AdminDashboardProps): JSX.Element | null => {
  const [loading, setLoading] = useState<boolean>(true);
  const [stats, setStats] = useState<AdminStats | null>(null);

  useEffect(() => {
    fetchStats();
  }, []);

  const fetchStats = async (): Promise<void> => {
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
      <Box className={styles.loadingContainer}>
        <CircularProgress />
      </Box>
    );
  }

  if (!stats) {
    return null;
  }

  // Préparer les données pour le graphique
  const chartData: ChartData[] = stats.requestsByMonth.map((item) => ({
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
          <Paper className={styles.chartPaper}>
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
            <Typography variant="body2" color="text.secondary" className={styles.chartFooter}>
              Total des requêtes sur 12 mois : {stats.requestsByMonth.reduce((acc, item) => acc + item.count, 0)}
            </Typography>
          </Paper>
        </Grid>
      </Grid>
    </Box>
  );
};

export default AdminDashboard;
