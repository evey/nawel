import { ChangeEvent, memo } from 'react';
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  Button,
  Box,
  CircularProgress,
  Alert,
} from '@mui/material';
import { AutoAwesome as AutoAwesomeIcon } from '@mui/icons-material';
import styles from '../../css/GiftFormDialog.module.less';

interface GiftFormData {
  name: string;
  description: string;
  url: string;
  imageUrl: string;
  price: string;
}

interface GiftFormDialogProps {
  open: boolean;
  isEditing: boolean;
  formData: GiftFormData;
  extractingInfo: boolean;
  extractError: string;
  onClose: () => void;
  onSubmit: () => void;
  onInputChange: (e: ChangeEvent<HTMLInputElement>) => void;
  onExtractInfo: () => void;
}

const GiftFormDialog = memo(({
  open,
  isEditing,
  formData,
  extractingInfo,
  extractError,
  onClose,
  onSubmit,
  onInputChange,
  onExtractInfo,
}: GiftFormDialogProps): JSX.Element => {
  return (
    <Dialog
      open={open}
      onClose={onClose}
      maxWidth="sm"
      fullWidth
      fullScreen={window.innerWidth < 600}
    >
      <DialogTitle>
        {isEditing ? 'Modifier le cadeau' : 'Ajouter un cadeau'}
      </DialogTitle>
      <DialogContent className={styles.dialogContent}>
        <TextField
          autoFocus
          margin="dense"
          name="name"
          label="Nom du cadeau"
          type="text"
          fullWidth
          required
          value={formData.name}
          onChange={onInputChange}
        />
        <TextField
          margin="dense"
          name="description"
          label="Description"
          type="text"
          fullWidth
          multiline
          rows={3}
          value={formData.description}
          onChange={onInputChange}
        />
        <Box className={styles.urlContainer}>
          <TextField
            margin="dense"
            name="url"
            label="Lien (URL)"
            type="url"
            fullWidth
            value={formData.url}
            onChange={onInputChange}
            helperText="Collez un lien puis cliquez sur 'Extraire' pour remplir automatiquement les champs"
          />
          <Button
            variant="contained"
            onClick={onExtractInfo}
            disabled={extractingInfo || !formData.url.trim()}
            startIcon={extractingInfo ? <CircularProgress size={20} /> : <AutoAwesomeIcon />}
            className={styles.extractButton}
          >
            {extractingInfo ? 'Extraction...' : 'Extraire'}
          </Button>
        </Box>
        {extractError && (
          <Alert severity="warning" className={styles.extractAlert}>
            {extractError}
          </Alert>
        )}
        <TextField
          margin="dense"
          name="imageUrl"
          label="URL de l'image"
          type="url"
          fullWidth
          value={formData.imageUrl}
          onChange={onInputChange}
          helperText="URL d'une image pour illustrer le cadeau"
        />
        <TextField
          margin="dense"
          name="price"
          label="Prix (€)"
          type="number"
          fullWidth
          value={formData.price}
          onChange={onInputChange}
          inputProps={{ step: '0.01', min: '0' }}
        />
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>Annuler</Button>
        <Button
          onClick={onSubmit}
          variant="contained"
          disabled={!formData.name.trim()}
        >
          {isEditing ? 'Modifier' : 'Ajouter'}
        </Button>
      </DialogActions>
    </Dialog>
  );
});

GiftFormDialog.displayName = 'GiftFormDialog';

export default GiftFormDialog;
