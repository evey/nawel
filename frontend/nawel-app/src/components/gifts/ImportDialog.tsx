import { memo } from 'react';
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  Typography,
  List,
  ListItem,
  ListItemText,
} from '@mui/material';
import styles from '../../css/ImportDialog.module.less';

interface ImportDialogProps {
  open: boolean;
  availableYears: number[];
  onClose: () => void;
  onImport: (year: number) => void;
}

const ImportDialog = memo(({
  open,
  availableYears,
  onClose,
  onImport,
}: ImportDialogProps): JSX.Element => {
  return (
    <Dialog
      open={open}
      onClose={onClose}
      fullWidth
      maxWidth="sm"
      fullScreen={window.innerWidth < 600}
    >
      <DialogTitle>Importer des cadeaux d'une année précédente</DialogTitle>
      <DialogContent className={styles.dialogContent}>
        <Typography variant="body2" className={styles.description}>
          Sélectionnez une année pour importer les cadeaux non achetés:
        </Typography>
        <List>
          {availableYears.map((year) => (
            <ListItem
              key={year}
              onClick={() => onImport(year)}
              className={styles.yearListItem}
              sx={{ borderColor: 'divider' }}
            >
              <ListItemText
                primary={year}
                secondary="Importer les cadeaux non achetés"
              />
            </ListItem>
          ))}
        </List>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>Annuler</Button>
      </DialogActions>
    </Dialog>
  );
});

ImportDialog.displayName = 'ImportDialog';

export default ImportDialog;
