import { ChangeEvent, RefObject, memo } from 'react';
import {
  Card,
  CardContent,
  Typography,
  Box,
  Button,
} from '@mui/material';
import {
  Save as SaveIcon,
  PhotoCamera as PhotoCameraIcon,
  Delete as DeleteIcon,
} from '@mui/icons-material';
import Avatar from '../Avatar';
import type { User } from '../../types';
import styles from '../../css/AvatarUpload.module.less';

interface AvatarUploadProps {
  user: User | null;
  avatarPreview: string | null;
  uploadingAvatar: boolean;
  fileInputRef: RefObject<HTMLInputElement | null>;
  onAvatarChange: (e: ChangeEvent<HTMLInputElement>) => void;
  onUploadAvatar: () => void;
  onCancelAvatar: () => void;
  onDeleteAvatar: () => void;
}

const AvatarUpload = memo(({
  user,
  avatarPreview,
  uploadingAvatar,
  fileInputRef,
  onAvatarChange,
  onUploadAvatar,
  onCancelAvatar,
  onDeleteAvatar,
}: AvatarUploadProps): JSX.Element => {
  return (
    <Card className={styles.card}>
      <CardContent>
        <Typography variant="h6" gutterBottom>
          Avatar
        </Typography>
        <Box className={styles.container}>
          <Box className={styles.avatarSection}>
            {avatarPreview ? (
              <Box
                component="img"
                src={avatarPreview}
                alt="Avatar preview"
                className={styles.avatarPreview}
                sx={{ borderColor: 'primary.main' }}
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
                onClick={onDeleteAvatar}
                disabled={uploadingAvatar}
              >
                Supprimer
              </Button>
            )}
          </Box>

          <Box className={styles.actionsSection}>
            <input
              ref={fileInputRef}
              type="file"
              accept="image/jpeg,image/jpg,image/png,image/gif,image/webp"
              onChange={onAvatarChange}
              className={styles.hiddenInput}
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
              <Box className={styles.buttonContainer}>
                <Button
                  variant="contained"
                  startIcon={<SaveIcon />}
                  onClick={onUploadAvatar}
                  disabled={uploadingAvatar}
                  fullWidth
                >
                  {uploadingAvatar ? 'Envoi...' : 'Enregistrer l\'avatar'}
                </Button>
                <Button
                  variant="outlined"
                  onClick={onCancelAvatar}
                  disabled={uploadingAvatar}
                  fullWidth
                >
                  Annuler
                </Button>
              </Box>
            )}
            <Typography variant="caption" color="text.secondary" className={styles.helperText}>
              Formats acceptés : JPG, PNG, GIF, WebP (max 5 Mo)
            </Typography>
          </Box>
        </Box>
      </CardContent>
    </Card>
  );
});

AvatarUpload.displayName = 'AvatarUpload';

export default AvatarUpload;
