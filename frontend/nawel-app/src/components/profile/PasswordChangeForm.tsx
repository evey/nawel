import { ChangeEvent, memo } from 'react';
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Box,
  TextField,
  Button,
} from '@mui/material';
import styles from '../../css/PasswordChangeForm.module.less';

interface PasswordChangeFormProps {
  open: boolean;
  currentPassword: string;
  newPassword: string;
  confirmPassword: string;
  loading: boolean;
  onClose: () => void;
  onChange: (e: ChangeEvent<HTMLInputElement>) => void;
  onSubmit: () => void;
}

const PasswordChangeForm = memo(({
  open,
  currentPassword,
  newPassword,
  confirmPassword,
  loading,
  onClose,
  onChange,
  onSubmit,
}: PasswordChangeFormProps): JSX.Element => {
  return (
    <Dialog
      open={open}
      onClose={onClose}
      maxWidth="sm"
      fullWidth
      fullScreen={window.innerWidth < 600}
    >
      <DialogTitle>Changer le mot de passe</DialogTitle>
      <DialogContent className={styles.dialogContent}>
        <Box className={styles.fieldsContainer}>
          <TextField
            name="currentPassword"
            label="Mot de passe actuel"
            type="password"
            value={currentPassword}
            onChange={onChange}
            fullWidth
            required
          />
          <TextField
            name="newPassword"
            label="Nouveau mot de passe"
            type="password"
            value={newPassword}
            onChange={onChange}
            fullWidth
            required
            helperText="Au moins 6 caractères"
          />
          <TextField
            name="confirmPassword"
            label="Confirmer le nouveau mot de passe"
            type="password"
            value={confirmPassword}
            onChange={onChange}
            fullWidth
            required
          />
        </Box>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>Annuler</Button>
        <Button
          onClick={onSubmit}
          variant="contained"
          disabled={
            loading ||
            !currentPassword ||
            !newPassword ||
            !confirmPassword
          }
        >
          Changer
        </Button>
      </DialogActions>
    </Dialog>
  );
});

PasswordChangeForm.displayName = 'PasswordChangeForm';

export default PasswordChangeForm;
