import { useState, useEffect, useRef } from 'react';
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
  IconButton,
} from '@mui/material';
import {
  Save as SaveIcon,
  Lock as LockIcon,
  PhotoCamera as PhotoCameraIcon,
  Delete as DeleteIcon,
} from '@mui/icons-material';
import NavigationBar from '../components/NavigationBar';
import Avatar from '../components/Avatar';
import { useAuth } from '../contexts/AuthContext';
import { usersAPI } from '../services/api';

const Profile = () => {
  const navigate = useNavigate();
  const { user, updateUser } = useAuth();
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [successMessage, setSuccessMessage] = useState('');
  const [openPasswordDialog, setOpenPasswordDialog] = useState(false);
  const [avatarPreview, setAvatarPreview] = useState(null);
  const [uploadingAvatar, setUploadingAvatar] = useState(false);
  const fileInputRef = useRef(null);
  const avatarFileRef = useRef(null);

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

  const handleAvatarChange = (e) => {
    const file = e.target.files[0];
    if (file) {
      // Validate file size (5MB max)
      if (file.size > 5 * 1024 * 1024) {
        setError('La taille du fichier ne doit pas dépasser 5 Mo');
        return;
      }

      // Validate file type
      const allowedTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/gif', 'image/webp'];
      if (!allowedTypes.includes(file.type)) {
        setError('Format de fichier non autorisé. Utilisez JPG, PNG, GIF ou WebP');
        return;
      }

      avatarFileRef.current = file;

      // Create preview
      const reader = new FileReader();
      reader.onloadend = () => {
        setAvatarPreview(reader.result);
      };
      reader.readAsDataURL(file);
      setError('');
    }
  };

  const handleUploadAvatar = async () => {
    const file = avatarFileRef.current;
    if (!file) {
      console.error('No avatar file in ref');
      return;
    }

    console.log('Avatar file object:', file);
    console.log('Avatar file type:', typeof file);
    console.log('Avatar file instanceof File:', file instanceof File);
    console.log('Avatar file name:', file.name);
    console.log('Avatar file size:', file.size);

    try {
      setUploadingAvatar(true);
      const response = await usersAPI.uploadAvatar(file);

      // Refresh user data
      const updatedUser = await usersAPI.getMe();
      updateUser(updatedUser.data);

      setSuccessMessage('Avatar mis à jour avec succès');
      avatarFileRef.current = null;
      setAvatarPreview(null);
      setError('');
      setTimeout(() => setSuccessMessage(''), 3000);
    } catch (err) {
      console.error('Error uploading avatar:', err);
      setError(err.response?.data?.message || 'Erreur lors de l\'upload de l\'avatar');
    } finally {
      setUploadingAvatar(false);
    }
  };

  const handleCancelAvatar = () => {
    avatarFileRef.current = null;
    setAvatarPreview(null);
    if (fileInputRef.current) {
      fileInputRef.current.value = '';
    }
  };

  const handleDeleteAvatar = async () => {
    if (!window.confirm('Êtes-vous sûr de vouloir supprimer votre avatar ?')) {
      return;
    }

    try {
      setUploadingAvatar(true);
      await usersAPI.deleteAvatar();

      // Refresh user data
      const updatedUser = await usersAPI.getMe();
      updateUser(updatedUser.data);

      setSuccessMessage('Avatar supprimé avec succès');
      setError('');
      setTimeout(() => setSuccessMessage(''), 3000);
    } catch (err) {
      console.error('Error deleting avatar:', err);
      setError('Erreur lors de la suppression de l\'avatar');
    } finally {
      setUploadingAvatar(false);
    }
  };

  return (
    <Box sx={{ flexGrow: 1 }}>
      <NavigationBar title="Mon profil" />

      <Container maxWidth="md" sx={{ mt: { xs: 2, sm: 4 }, mb: 4, px: { xs: 2, sm: 3 } }}>
        <Typography variant="h4" gutterBottom sx={{ fontSize: { xs: '1.5rem', sm: '2.125rem' } }}>
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
              Avatar
            </Typography>
            <Box sx={{ display: 'flex', flexDirection: { xs: 'column', sm: 'row' }, alignItems: { xs: 'center', sm: 'flex-start' }, gap: 3, mt: 2 }}>
              <Box sx={{ display: 'flex', flexDirection: 'column', alignItems: 'center', gap: 2 }}>
                {avatarPreview ? (
                  <Box
                    component="img"
                    src={avatarPreview}
                    alt="Avatar preview"
                    sx={{
                      width: 120,
                      height: 120,
                      borderRadius: '50%',
                      objectFit: 'cover',
                      border: '3px solid',
                      borderColor: 'primary.main',
                    }}
                  />
                ) : (
                  <Avatar user={user} size={120} />
                )}
                {user?.avatar && !avatarPreview && (
                  <Button
                    variant="outlined"
                    color="error"
                    size="small"
                    startIcon={<DeleteIcon />}
                    onClick={handleDeleteAvatar}
                    disabled={uploadingAvatar}
                  >
                    Supprimer
                  </Button>
                )}
              </Box>

              <Box sx={{ flex: 1, width: '100%' }}>
                <input
                  ref={fileInputRef}
                  type="file"
                  accept="image/jpeg,image/jpg,image/png,image/gif,image/webp"
                  onChange={handleAvatarChange}
                  style={{ display: 'none' }}
                />

                {!avatarPreview ? (
                  <Button
                    variant="outlined"
                    startIcon={<PhotoCameraIcon />}
                    onClick={() => fileInputRef.current?.click()}
                    fullWidth
                  >
                    Choisir une image
                  </Button>
                ) : (
                  <Box sx={{ display: 'flex', gap: 1, flexDirection: { xs: 'column', sm: 'row' } }}>
                    <Button
                      variant="contained"
                      startIcon={<SaveIcon />}
                      onClick={handleUploadAvatar}
                      disabled={uploadingAvatar}
                      fullWidth
                    >
                      {uploadingAvatar ? 'Envoi...' : 'Enregistrer l\'avatar'}
                    </Button>
                    <Button
                      variant="outlined"
                      onClick={handleCancelAvatar}
                      disabled={uploadingAvatar}
                      fullWidth
                    >
                      Annuler
                    </Button>
                  </Box>
                )}
                <Typography variant="caption" color="text.secondary" sx={{ mt: 1, display: 'block' }}>
                  Formats acceptés : JPG, PNG, GIF, WebP (max 5 Mo)
                </Typography>
              </Box>
            </Box>
          </CardContent>
        </Card>

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

        <Box sx={{ display: 'flex', flexDirection: { xs: 'column', sm: 'row' }, gap: 2 }}>
          <Button
            variant="contained"
            startIcon={<SaveIcon />}
            onClick={handleSaveProfile}
            disabled={loading}
            fullWidth
            sx={{ minWidth: { xs: 'auto', sm: '150px' } }}
          >
            Enregistrer
          </Button>
          <Button
            variant="outlined"
            startIcon={<LockIcon />}
            onClick={handleOpenPasswordDialog}
            fullWidth
            sx={{ minWidth: { xs: 'auto', sm: '200px' } }}
          >
            Changer le mot de passe
          </Button>
        </Box>
      </Container>

      {/* Change Password Dialog */}
      <Dialog
        open={openPasswordDialog}
        onClose={handleClosePasswordDialog}
        maxWidth="sm"
        fullWidth
        fullScreen={window.innerWidth < 600}
      >
        <DialogTitle>Changer le mot de passe</DialogTitle>
        <DialogContent sx={{ pt: { xs: 3, sm: 2 } }}>
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
