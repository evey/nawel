import { Alert, AlertTitle, Button, Box } from '@mui/material';
import { Close as CloseIcon } from '@mui/icons-material';
import { useAuth } from '../contexts/AuthContext';
import { useNavigate } from 'react-router-dom';

const ManagingChildBanner = () => {
  const { managingChild, stopManagingChild } = useAuth();
  const navigate = useNavigate();

  if (!managingChild) {
    return null;
  }

  const handleStop = () => {
    stopManagingChild();
    navigate('/'); // Retour à la home
  };

  return (
    <Alert
      severity="info"
      icon={false}
      sx={{
        mb: 2,
        borderRadius: 2,
        backgroundColor: '#e3f2fd',
        borderLeft: '4px solid #2196f3',
      }}
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
      <AlertTitle sx={{ fontWeight: 600 }}>
        Mode gestion
      </AlertTitle>
      <Box>
        Vous gérez actuellement la liste de <strong>{managingChild.userName}</strong>
      </Box>
    </Alert>
  );
};

export default ManagingChildBanner;
