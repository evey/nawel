import { useState, useEffect, useRef, ChangeEvent, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Container,
  Box,
  Typography,
  Button,
  Alert,
} from '@mui/material';
import {
  Save as SaveIcon,
  Lock as LockIcon,
} from '@mui/icons-material';
import NavigationBar from '../components/NavigationBar';
import ProfileForm from '../components/profile/ProfileForm';
import PasswordChangeForm from '../components/profile/PasswordChangeForm';
import AvatarUpload from '../components/profile/AvatarUpload';
import { useAuth } from '../contexts/AuthContext';
import { usersAPI } from '../services/api';
import styles from '../css/Profile.module.less';
import commonStyles from '../css/common.module.less';

interface ProfileFormData {
  firstName: string;
  lastName: string;
  email: string;
  pseudo: string;
  notifyListEdit: boolean;
  notifyGiftTaken: boolean;
  displayPopup: boolean;
}

interface PasswordFormData {
  currentPassword: string;
  newPassword: string;
  confirmPassword: string;
}

const Profile = (): JSX.Element => {
  const navigate = useNavigate();
  const { user, updateUser } = useAuth();
  const [loading, setLoading] = useState<boolean>(false);
  const [error, setError] = useState<string>('');
  const [successMessage, setSuccessMessage] = useState<string>('');
  const [openPasswordDialog, setOpenPasswordDialog] = useState<boolean>(false);
  const [avatarPreview, setAvatarPreview] = useState<string | null>(null);
  const [uploadingAvatar, setUploadingAvatar] = useState<boolean>(false);
  const fileInputRef = useRef<HTMLInputElement>(null);
  const avatarFileRef = useRef<File | null>(null);

  const [formData, setFormData] = useState<ProfileFormData>({
    firstName: '',
    lastName: '',
    email: '',
    pseudo: '',
    notifyListEdit: false,
    notifyGiftTaken: false,
    displayPopup: true,
  });

  const [passwordData, setPasswordData] = useState<PasswordFormData>({
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

  const handleInputChange = useCallback((e: ChangeEvent<HTMLInputElement>): void => {
    const { name, value, checked, type } = e.target;
    setFormData((prev) => ({
      ...prev,
      [name]: type === 'checkbox' ? checked : value,
    }));
  }, []);

  const handlePasswordInputChange = useCallback((e: ChangeEvent<HTMLInputElement>): void => {
    const { name, value } = e.target;
    setPasswordData((prev) => ({
      ...prev,
      [name]: value,
    }));
  }, []);

  const handleSaveProfile = useCallback(async (): Promise<void> => {
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
  }, [formData, updateUser]);

  const handleOpenPasswordDialog = useCallback((): void => {
    setPasswordData({
      currentPassword: '',
      newPassword: '',
      confirmPassword: '',
    });
    setOpenPasswordDialog(true);
  }, []);

  const handleClosePasswordDialog = useCallback((): void => {
    setOpenPasswordDialog(false);
  }, []);

  const handleChangePassword = useCallback(async (): Promise<void> => {
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
    } catch (err: any) {
      console.error('Error changing password:', err);
      setError(err.response?.data?.message || 'Erreur lors du changement de mot de passe');
    } finally {
      setLoading(false);
    }
  }, [passwordData, handleClosePasswordDialog]);

  const handleAvatarChange = useCallback((e: ChangeEvent<HTMLInputElement>): void => {
    const file = e.target.files?.[0];
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
        setAvatarPreview(reader.result as string);
      };
      reader.readAsDataURL(file);
      setError('');
    }
  }, []);

  const handleUploadAvatar = useCallback(async (): Promise<void> => {
    const file = avatarFileRef.current;
    if (!file) {
      console.error('No avatar file in ref');
      return;
    }

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
    } catch (err: any) {
      console.error('Error uploading avatar:', err);
      setError(err.response?.data?.message || 'Erreur lors de l\'upload de l\'avatar');
    } finally {
      setUploadingAvatar(false);
    }
  }, [updateUser]);

  const handleCancelAvatar = useCallback((): void => {
    avatarFileRef.current = null;
    setAvatarPreview(null);
    if (fileInputRef.current) {
      fileInputRef.current.value = '';
    }
  }, []);

  const handleDeleteAvatar = useCallback(async (): Promise<void> => {
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
  }, [updateUser]);

  return (
    <Box className={commonStyles.pageRoot}>
      <NavigationBar title="Mon profil" />

      <Container maxWidth="md" className={styles.container}>
        <Typography variant="h4" gutterBottom className={styles.title}>
          Mon profil
        </Typography>

        {successMessage && (
          <Alert severity="success" className={styles.alertContainer} onClose={() => setSuccessMessage('')}>
            {successMessage}
          </Alert>
        )}

        {error && (
          <Alert severity="error" className={styles.alertContainer} onClose={() => setError('')}>
            {error}
          </Alert>
        )}

        <AvatarUpload
          user={user}
          avatarPreview={avatarPreview}
          uploadingAvatar={uploadingAvatar}
          fileInputRef={fileInputRef}
          onAvatarChange={handleAvatarChange}
          onUploadAvatar={handleUploadAvatar}
          onCancelAvatar={handleCancelAvatar}
          onDeleteAvatar={handleDeleteAvatar}
        />

        <ProfileForm
          login={user?.login || ''}
          firstName={formData.firstName}
          lastName={formData.lastName}
          email={formData.email}
          pseudo={formData.pseudo}
          notifyListEdit={formData.notifyListEdit}
          notifyGiftTaken={formData.notifyGiftTaken}
          displayPopup={formData.displayPopup}
          onChange={handleInputChange}
        />

        <Box className={styles.buttonContainer}>
          <Button
            variant="contained"
            startIcon={<SaveIcon />}
            onClick={handleSaveProfile}
            disabled={loading}
            fullWidth
            className={styles.primaryButton}
          >
            Enregistrer
          </Button>
          <Button
            variant="outlined"
            startIcon={<LockIcon />}
            onClick={handleOpenPasswordDialog}
            fullWidth
            className={styles.secondaryButton}
          >
            Changer le mot de passe
          </Button>
        </Box>
      </Container>

      <PasswordChangeForm
        open={openPasswordDialog}
        currentPassword={passwordData.currentPassword}
        newPassword={passwordData.newPassword}
        confirmPassword={passwordData.confirmPassword}
        loading={loading}
        onClose={handleClosePasswordDialog}
        onChange={handlePasswordInputChange}
        onSubmit={handleChangePassword}
      />
    </Box>
  );
};

export default Profile;
