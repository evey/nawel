import { useState, useEffect, FormEvent, ChangeEvent } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
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
import { authAPI } from '../services/api';
import styles from '../css/Login.module.less';

const ResetPassword = (): JSX.Element => {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const token = searchParams.get('token');

  const [newPassword, setNewPassword] = useState<string>('');
  const [confirmPassword, setConfirmPassword] = useState<string>('');
  const [error, setError] = useState<string>('');
  const [success, setSuccess] = useState<boolean>(false);
  const [loading, setLoading] = useState<boolean>(false);

  useEffect(() => {
    if (!token) {
      setError('Token de réinitialisation manquant dans l\'URL');
    }
  }, [token]);

  const handleSubmit = async (e: FormEvent<HTMLFormElement>): Promise<void> => {
    e.preventDefault();
    setError('');

    if (!token) {
      setError('Token de réinitialisation manquant');
      return;
    }

    // Validation
    if (newPassword.length < 6) {
      setError('Le mot de passe doit contenir au moins 6 caractères');
      return;
    }

    if (newPassword !== confirmPassword) {
      setError('Les mots de passe ne correspondent pas');
      return;
    }

    setLoading(true);

    try {
      await authAPI.resetPassword({ token, newPassword });
      setSuccess(true);

      // Rediriger vers la page de login après 3 secondes
      setTimeout(() => {
        navigate('/login');
      }, 3000);
    } catch (err: any) {
      const errorMessage = err.response?.data?.message || 'Erreur lors de la réinitialisation du mot de passe';
      setError(errorMessage);
    } finally {
      setLoading(false);
    }
  };

  const handlePasswordChange = (e: ChangeEvent<HTMLInputElement>): void => {
    setNewPassword(e.target.value);
    setError('');
  };

  const handleConfirmPasswordChange = (e: ChangeEvent<HTMLInputElement>): void => {
    setConfirmPassword(e.target.value);
    setError('');
  };

  if (!token) {
    return (
      <Container maxWidth="sm">
        <Box className={styles.pageContainer}>
          <Paper elevation={3} className={styles.paper}>
            <Typography component="h1" variant="h4" align="center" gutterBottom>
              Réinitialisation du mot de passe
            </Typography>

            <Alert severity="error" sx={{ mt: 2 }}>
              <AlertTitle>Lien invalide</AlertTitle>
              Le lien de réinitialisation est invalide ou incomplet.
            </Alert>

            <Button
              fullWidth
              variant="contained"
              onClick={() => navigate('/login')}
              sx={{ mt: 3 }}
            >
              Retour à la connexion
            </Button>
          </Paper>
        </Box>
      </Container>
    );
  }

  return (
    <Container maxWidth="sm">
      <Box className={styles.pageContainer}>
        <Paper elevation={3} className={styles.paper}>
          <Typography component="h1" variant="h4" align="center" gutterBottom>
            Réinitialisation du mot de passe
          </Typography>

          {success ? (
            <Alert severity="success" sx={{ mt: 2 }}>
              <AlertTitle>Mot de passe réinitialisé !</AlertTitle>
              Votre mot de passe a été réinitialisé avec succès. Vous allez être redirigé vers la page de connexion...
            </Alert>
          ) : (
            <>
              <Typography variant="body2" align="center" color="text.secondary" className={styles.subtitle}>
                Choisissez un nouveau mot de passe sécurisé
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
                  name="newPassword"
                  label="Nouveau mot de passe"
                  type="password"
                  id="newPassword"
                  autoComplete="new-password"
                  autoFocus
                  value={newPassword}
                  onChange={handlePasswordChange}
                  disabled={loading}
                  helperText="Au moins 6 caractères"
                />
                <TextField
                  margin="normal"
                  required
                  fullWidth
                  name="confirmPassword"
                  label="Confirmer le mot de passe"
                  type="password"
                  id="confirmPassword"
                  autoComplete="new-password"
                  value={confirmPassword}
                  onChange={handleConfirmPasswordChange}
                  disabled={loading}
                />
                <Button
                  type="submit"
                  fullWidth
                  variant="contained"
                  className={styles.submitButton}
                  disabled={loading}
                >
                  {loading ? <CircularProgress size={24} /> : 'Réinitialiser le mot de passe'}
                </Button>
              </Box>
            </>
          )}
        </Paper>
      </Box>
    </Container>
  );
};

export default ResetPassword;
