import { Alert, AlertTitle, Button, Box } from '@mui/material';
import { Close as CloseIcon } from '@mui/icons-material';
import { useAuth } from '../contexts/AuthContext';
import { useNavigate } from 'react-router-dom';
import styles from '../css/ManagingChildBanner.module.less';

const ManagingChildBanner = (): JSX.Element | null => {
  const { managingChild, stopManagingChild } = useAuth();
  const navigate = useNavigate();

  if (!managingChild) {
    return null;
  }

  const handleStop = (): void => {
    stopManagingChild();
    navigate('/'); // Retour à la home
  };

  return (
    <Alert
      severity="info"
      icon={false}
      className={styles.banner}
      action={
        <Button
          color="inherit"
          size="small"
          startIcon={<CloseIcon />}
          onClick={handleStop}
        >
          Retour
        </Button>
      }
    >
      <AlertTitle className={styles.alertTitle}>
        Mode gestion
      </AlertTitle>
      <Box>
        Vous gérez actuellement la liste de <strong>{managingChild.userName}</strong>
      </Box>
    </Alert>
  );
};

export default ManagingChildBanner;
