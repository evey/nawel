import { useState, ChangeEvent, FormEvent } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Container,
  Box,
  Paper,
  TextField,
  Button,
  Typography,
  Alert,
  AlertTitle,
  CircularProgress,
} from '@mui/material';
import { useAuth } from '../contexts/AuthContext';
import { authAPI } from '../services/api';
import type { LoginCredentials } from '../types';
import styles from '../css/Login.module.less';

const Login = (): JSX.Element => {
  const navigate = useNavigate();
  const { login } = useAuth();
  const [formData, setFormData] = useState<LoginCredentials>({
    login: '',
    password: '',
  });
  const [error, setError] = useState<string>('');
  const [loading, setLoading] = useState<boolean>(false);
  const [legacyPasswordDetected, setLegacyPasswordDetected] = useState<boolean>(false);
  const [userEmail, setUserEmail] = useState<string>('');
  const [migrationEmailSent, setMigrationEmailSent] = useState<boolean>(false);
  const [sendingMigrationEmail, setSendingMigrationEmail] = useState<boolean>(false);

  const handleChange = (e: ChangeEvent<HTMLInputElement>): void => {
    setFormData({
      ...formData,
      [e.target.name]: e.target.value,
    });
    setError('');
    setLegacyPasswordDetected(false);
    setMigrationEmailSent(false);
  };

  const handleSubmit = async (e: FormEvent<HTMLFormElement>): Promise<void> => {
    e.preventDefault();
    setLoading(true);
    setError('');
    setLegacyPasswordDetected(false);
    setMigrationEmailSent(false);

    const result = await login(formData);

    if (result.success) {
      navigate('/');
    } else if (result.errorCode === 'LEGACY_PASSWORD') {
      // Détecter l'erreur spécifique MD5
      setLegacyPasswordDetected(true);
      setUserEmail(result.email || '');
      setError('');
    } else {
      setError(result.error || 'Erreur de connexion');
    }

    setLoading(false);
  };

  const handleRequestMigration = async (): Promise<void> => {
    setSendingMigrationEmail(true);
    setError('');

    try {
      await authAPI.requestMigrationReset({ login: formData.login });
      setMigrationEmailSent(true);
      setLegacyPasswordDetected(false);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Erreur lors de l\'envoi de l\'email');
    } finally {
      setSendingMigrationEmail(false);
    }
  };

  return (
    <Container maxWidth="sm">
      <Box className={styles.pageContainer}>
        <Paper elevation={3} className={styles.paper}>
          <Typography component="h1" variant="h4" align="center" gutterBottom>
            Nawel - Listes de Noël
          </Typography>
          <Typography variant="body2" align="center" color="text.secondary" className={styles.subtitle}>
            Connectez-vous pour accéder à votre liste
          </Typography>

          {error && (
            <Alert severity="error" className={styles.alert}>
              {error}
            </Alert>
          )}

          <Box component="form" onSubmit={handleSubmit} noValidate>
            <TextField
              margin="normal"
              required
              fullWidth
              id="login"
              label="Identifiant"
              name="login"
              autoComplete="username"
              autoFocus
              value={formData.login}
              onChange={handleChange}
              disabled={loading}
            />
            <TextField
              margin="normal"
              required
              fullWidth
              name="password"
              label="Mot de passe"
              type="password"
              id="password"
              autoComplete="current-password"
              value={formData.password}
              onChange={handleChange}
              disabled={loading}
            />
            <Button
              type="submit"
              fullWidth
              variant="contained"
              className={styles.submitButton}
              disabled={loading}
            >
              {loading ? <CircularProgress size={24} /> : 'Se connecter'}
            </Button>
          </Box>

          {legacyPasswordDetected && !migrationEmailSent && (
            <Alert severity="warning" sx={{ mt: 2 }}>
              <AlertTitle>Mise à jour de sécurité requise</AlertTitle>
              <Typography variant="body2" sx={{ mb: 2 }}>
                Pour améliorer la sécurité de votre compte, votre mot de passe doit être réinitialisé.
              </Typography>
              <Button
                variant="contained"
                color="primary"
                onClick={handleRequestMigration}
                fullWidth
                disabled={sendingMigrationEmail}
              >
                {sendingMigrationEmail ? <CircularProgress size={24} /> : 'Recevoir un email de réinitialisation'}
              </Button>
              {userEmail && (
                <Typography variant="caption" sx={{ mt: 1, display: 'block' }}>
                  L'email sera envoyé à : {userEmail}
                </Typography>
              )}
            </Alert>
          )}

          {migrationEmailSent && (
            <Alert severity="success" sx={{ mt: 2 }}>
              <AlertTitle>Email envoyé !</AlertTitle>
              <Typography variant="body2">
                Consultez votre boîte mail ({userEmail}) pour réinitialiser votre mot de passe.
              </Typography>
            </Alert>
          )}

          <Box className={styles.testAccountInfo}>
            <Typography variant="body2" color="text.secondary" align="center">
              Comptes de test : sylvain / password123
            </Typography>
          </Box>
        </Paper>
      </Box>
    </Container>
  );
};

export default Login;
