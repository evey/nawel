import { ChangeEvent, memo } from 'react';
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Typography,
  TextField,
  Button,
  Alert,
} from '@mui/material';
import type { Gift } from '../../types';
import styles from '../../css/ReserveDialog.module.less';

interface ReserveDialogProps {
  open: boolean;
  gift: Gift | null;
  reserveComment: string;
  isReservedByMe: boolean;
  onClose: () => void;
  onCommentChange: (e: ChangeEvent<HTMLInputElement>) => void;
  onConfirm: () => void;
}

const ReserveDialog = memo(({
  open,
  gift,
  reserveComment,
  isReservedByMe,
  onClose,
  onCommentChange,
  onConfirm,
}: ReserveDialogProps): JSX.Element => {
  if (!gift) return <></>;

  const isGroupGiftOrAlreadyReserved = gift.isGroupGift || (gift.isTaken && !isReservedByMe);

  return (
    <Dialog
      open={open}
      onClose={onClose}
      maxWidth="sm"
      fullWidth
      fullScreen={window.innerWidth < 600}
    >
      <DialogTitle>
        {isGroupGiftOrAlreadyReserved
          ? 'Participer au cadeau'
          : 'Confirmer la réservation'}
      </DialogTitle>
      <DialogContent className={styles.dialogContent}>
        <Typography className={styles.description}>
          {isGroupGiftOrAlreadyReserved
            ? `Voulez-vous participer au cadeau "${gift.name}" ?`
            : `Voulez-vous réserver le cadeau "${gift.name}" ?`}
        </Typography>
        {gift.isGroupGift && (
          <Alert severity="info" className={styles.alert}>
            Il s'agit d'un cadeau groupé. Vous participerez à ce cadeau avec d'autres personnes.
          </Alert>
        )}
        {gift.isTaken && !gift.isGroupGift && !isReservedByMe && (
          <Alert severity="info" className={styles.alert}>
            Ce cadeau a déjà été réservé par <strong>{gift.takenByUserName}</strong>.
            En participant, vous créez automatiquement un cadeau groupé.
          </Alert>
        )}
        <TextField
          fullWidth
          multiline
          rows={3}
          label="Commentaire (optionnel)"
          placeholder="Ex: J'ai pris le tome 1 et 2"
          value={reserveComment}
          onChange={onCommentChange}
          helperText="Ajoutez un commentaire pour informer les autres participants"
        />
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>Annuler</Button>
        <Button onClick={onConfirm} variant="contained" color="primary">
          Confirmer
        </Button>
      </DialogActions>
    </Dialog>
  );
});

ReserveDialog.displayName = 'ReserveDialog';

export default ReserveDialog;
