import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Container,
  Box,
  Typography,
  Card,
  CardContent,
  Button,
  TextField,
  FormControlLabel,
  Checkbox,
  CircularProgress,
  Alert,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Divider,
} from '@mui/material';
import {
  Save as SaveIcon,
  Lock as LockIcon,
} from '@mui/icons-material';
import NavigationBar from '../components/NavigationBar';
import { useAuth } from '../contexts/AuthContext';
import { usersAPI } from '../services/api';

const Profile = () => {
  const navigate = useNavigate();
  const { user, updateUser } = useAuth();
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [successMessage, setSuccessMessage] = useState('');
  const [openPasswordDialog, setOpenPasswordDialog] = useState(false);

  const [formData, setFormData] = useState({
    firstName: '',
    lastName: '',
    email: '',
    pseudo: '',
    notifyListEdit: false,
    notifyGiftTaken: false,
    displayPopup: true,
  });

  const [passwordData, setPasswordData] = useState({
    currentPassword: '',
    newPassword: '',
    confirmPassword: '',
  });

  useEffect(() => {
    if (user) {
      setFormData({
        firstName: user.firstName || '',
        lastName: user.lastName || '',
        email: user.email || '',
        pseudo: user.pseudo || '',
        notifyListEdit: user.notifyListEdit || false,
        notifyGiftTaken: user.notifyGiftTaken || false,
        displayPopup: user.displayPopup !== undefined ? user.displayPopup : true,
      });
    }
  }, [user]);

  const handleInputChange = (e) => {
    const { name, value, checked, type } = e.target;
    setFormData((prev) => ({
      ...prev,
      [name]: type === 'checkbox' ? checked : value,
    }));
  };

  const handlePasswordInputChange = (e) => {
    const { name, value } = e.target;
    setPasswordData((prev) => ({
      ...prev,
      [name]: value,
    }));
  };

  const handleSaveProfile = async () => {
    try {
      setLoading(true);
      const response = await usersAPI.updateMe(formData);
      updateUser(response.data);
      setSuccessMessage('Profil mis à jour avec succès');
      setError('');
      setTimeout(() => setSuccessMessage(''), 3000);
    } catch (err) {
      console.error('Error updating profile:', err);
      setError('Erreur lors de la mise à jour du profil');
    } finally {
      setLoading(false);
    }
  };

  const handleOpenPasswordDialog = () => {
    setPasswordData({
      currentPassword: '',
      newPassword: '',
      confirmPassword: '',
    });
    setOpenPasswordDialog(true);
  };

  const handleClosePasswordDialog = () => {
    setOpenPasswordDialog(false);
  };

  const handleChangePassword = async () => {
    if (passwordData.newPassword !== passwordData.confirmPassword) {
      setError('Les mots de passe ne correspondent pas');
      return;
    }

    if (passwordData.newPassword.length < 6) {
      setError('Le mot de passe doit contenir au moins 6 caractères');
      return;
    }

    try {
      setLoading(true);
      await usersAPI.changePassword({
        currentPassword: passwordData.currentPassword,
        newPassword: passwordData.newPassword,
      });
      setSuccessMessage('Mot de passe modifié avec succès');
      setError('');
      handleClosePasswordDialog();
      setTimeout(() => setSuccessMessage(''), 3000);
    } catch (err) {
      console.error('Error changing password:', err);
      setError(err.response?.data?.message || 'Erreur lors du changement de mot de passe');
    } finally {
      setLoading(false);
    }
  };

  return (
    <Box sx={{ flexGrow: 1 }}>
      <NavigationBar title="Mon profil" />

      <Container maxWidth="md" sx={{ mt: 4, mb: 4 }}>
        <Typography variant="h4" gutterBottom>
          Mon profil
        </Typography>

        {successMessage && (
          <Alert severity="success" sx={{ mb: 3 }} onClose={() => setSuccessMessage('')}>
            {successMessage}
          </Alert>
        )}

        {error && (
          <Alert severity="error" sx={{ mb: 3 }} onClose={() => setError('')}>
            {error}
          </Alert>
        )}

        <Card sx={{ mb: 3 }}>
          <CardContent>
            <Typography variant="h6" gutterBottom>
              Informations personnelles
            </Typography>
            <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2, mt: 2 }}>
              <TextField
                label="Login"
                value={user?.login || ''}
                disabled
                fullWidth
                helperText="Le login ne peut pas être modifié"
              />
              <TextField
                name="firstName"
                label="Prénom"
                value={formData.firstName}
                onChange={handleInputChange}
                fullWidth
              />
              <TextField
                name="lastName"
                label="Nom"
                value={formData.lastName}
                onChange={handleInputChange}
                fullWidth
              />
              <TextField
                name="email"
                label="Email"
                type="email"
                value={formData.email}
                onChange={handleInputChange}
                fullWidth
              />
              <TextField
                name="pseudo"
                label="Pseudo"
                value={formData.pseudo}
                onChange={handleInputChange}
                fullWidth
              />
            </Box>
          </CardContent>
        </Card>

        <Card sx={{ mb: 3 }}>
          <CardContent>
            <Typography variant="h6" gutterBottom>
              Préférences de notifications
            </Typography>
            <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1, mt: 2 }}>
              <FormControlLabel
                control={
                  <Checkbox
                    name="notifyListEdit"
                    checked={formData.notifyListEdit}
                    onChange={handleInputChange}
                  />
                }
                label="M'envoyer un email quand quelqu'un modifie sa liste"
              />
              <FormControlLabel
                control={
                  <Checkbox
                    name="notifyGiftTaken"
                    checked={formData.notifyGiftTaken}
                    onChange={handleInputChange}
                  />
                }
                label="M'envoyer un email quand quelqu'un réserve un cadeau"
              />
              <FormControlLabel
                control={
                  <Checkbox
                    name="displayPopup"
                    checked={formData.displayPopup}
                    onChange={handleInputChange}
                  />
                }
                label="Afficher les popups d'information"
              />
            </Box>
          </CardContent>
        </Card>

        <Box sx={{ display: 'flex', gap: 2 }}>
          <Button
            variant="contained"
            startIcon={<SaveIcon />}
            onClick={handleSaveProfile}
            disabled={loading}
          >
            Enregistrer
          </Button>
          <Button
            variant="outlined"
            startIcon={<LockIcon />}
            onClick={handleOpenPasswordDialog}
          >
            Changer le mot de passe
          </Button>
        </Box>
      </Container>

      {/* Change Password Dialog */}
      <Dialog open={openPasswordDialog} onClose={handleClosePasswordDialog} maxWidth="sm" fullWidth>
        <DialogTitle>Changer le mot de passe</DialogTitle>
        <DialogContent>
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2, mt: 2 }}>
            <TextField
              name="currentPassword"
              label="Mot de passe actuel"
              type="password"
              value={passwordData.currentPassword}
              onChange={handlePasswordInputChange}
              fullWidth
              required
            />
            <TextField
              name="newPassword"
              label="Nouveau mot de passe"
              type="password"
              value={passwordData.newPassword}
              onChange={handlePasswordInputChange}
              fullWidth
              required
              helperText="Au moins 6 caractères"
            />
            <TextField
              name="confirmPassword"
              label="Confirmer le nouveau mot de passe"
              type="password"
              value={passwordData.confirmPassword}
              onChange={handlePasswordInputChange}
              fullWidth
              required
            />
          </Box>
        </DialogContent>
        <DialogActions>
          <Button onClick={handleClosePasswordDialog}>Annuler</Button>
          <Button
            onClick={handleChangePassword}
            variant="contained"
            disabled={
              loading ||
              !passwordData.currentPassword ||
              !passwordData.newPassword ||
              !passwordData.confirmPassword
            }
          >
            Changer
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default Profile;
