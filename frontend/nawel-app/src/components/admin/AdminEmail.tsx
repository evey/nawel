import { useState, useEffect } from 'react';
import {
  Box,
  Typography,
  TextField,
  Button,
  Select,
  MenuItem,
  FormControl,
  InputLabel,
  Alert,
  CircularProgress,
  Chip,
  Card,
  CardContent,
} from '@mui/material';
import {
  Send as SendIcon,
  CheckCircle as CheckCircleIcon,
  Cancel as CancelIcon,
} from '@mui/icons-material';
import { adminAPI } from '../../services/api';
import { useAuth } from '../../contexts/AuthContext';

interface EmailStatus {
  enabled: boolean;
  from: string;
  appUrl: string;
}

const EMAIL_TYPES = [
  {
    value: 'list_edited',
    label: 'Modification de liste',
    description: "Notifie qu'un utilisateur a modifié sa liste de cadeaux",
  },
  {
    value: 'gift_reserved',
    label: 'Réservation de cadeau',
    description: "Notifie qu'un cadeau a été réservé ou participé",
  },
  {
    value: 'migration_reset',
    label: 'Réinitialisation de mot de passe (migration)',
    description: 'Email envoyé aux utilisateurs avec ancien mot de passe MD5',
  },
];

interface AdminEmailProps {
  setError: (error: string) => void;
}

const AdminEmail = ({ setError }: AdminEmailProps): JSX.Element => {
  const { user } = useAuth();
  const [emailStatus, setEmailStatus] = useState<EmailStatus | null>(null);
  const [loadingStatus, setLoadingStatus] = useState(true);
  const [targetEmail, setTargetEmail] = useState(user?.email ?? '');
  const [emailType, setEmailType] = useState('list_edited');
  const [sending, setSending] = useState(false);
  const [successMessage, setSuccessMessage] = useState('');

  useEffect(() => {
    loadEmailStatus();
  }, []);

  const loadEmailStatus = async (): Promise<void> => {
    try {
      const response = await adminAPI.getEmailStatus();
      setEmailStatus(response.data);
    } catch {
      setError('Impossible de charger la configuration email');
    } finally {
      setLoadingStatus(false);
    }
  };

  const handleSend = async (): Promise<void> => {
    if (!targetEmail) return;
    setSuccessMessage('');
    setSending(true);
    try {
      const response = await adminAPI.sendTestEmail(targetEmail, emailType);
      setSuccessMessage(response.data.message);
    } catch (err: unknown) {
      const message =
        (err as { response?: { data?: { message?: string } } })?.response?.data?.message ??
        "Erreur lors de l'envoi";
      setError(message);
    } finally {
      setSending(false);
    }
  };

  if (loadingStatus) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', p: 4 }}>
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Box sx={{ p: 3, maxWidth: 600 }}>
      <Typography variant="h6" gutterBottom>
        Test d'envoi d'emails
      </Typography>

      <Card variant="outlined" sx={{ mb: 3 }}>
        <CardContent>
          <Typography variant="subtitle2" color="text.secondary" gutterBottom>
            Configuration actuelle
          </Typography>
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1 }}>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
              <Typography variant="body2">Statut :</Typography>
              {emailStatus?.enabled ? (
                <Chip icon={<CheckCircleIcon />} label="Activé" color="success" size="small" />
              ) : (
                <Chip icon={<CancelIcon />} label="Désactivé (logs uniquement)" color="warning" size="small" />
              )}
            </Box>
            {emailStatus?.from && (
              <Typography variant="body2" color="text.secondary">
                Expéditeur : {emailStatus.from}
              </Typography>
            )}
            {emailStatus?.appUrl && (
              <Typography variant="body2" color="text.secondary">
                URL de l'app : {emailStatus.appUrl}
              </Typography>
            )}
          </Box>
        </CardContent>
      </Card>

      {!emailStatus?.enabled && (
        <Alert severity="warning" sx={{ mb: 3 }}>
          L'envoi d'emails est désactivé. L'email de test sera loggué dans la console du backend mais
          ne sera pas réellement envoyé.
        </Alert>
      )}

      {successMessage && (
        <Alert severity="success" sx={{ mb: 3 }} onClose={() => setSuccessMessage('')}>
          {successMessage}
        </Alert>
      )}

      <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
        <FormControl fullWidth>
          <InputLabel>Type d'email</InputLabel>
          <Select
            value={emailType}
            label="Type d'email"
            onChange={(e) => setEmailType(e.target.value)}
          >
            {EMAIL_TYPES.map((type) => (
              <MenuItem key={type.value} value={type.value}>
                <Box>
                  <Typography variant="body2">{type.label}</Typography>
                  <Typography variant="caption" color="text.secondary">
                    {type.description}
                  </Typography>
                </Box>
              </MenuItem>
            ))}
          </Select>
        </FormControl>

        <TextField
          label="Adresse email de destination"
          type="email"
          value={targetEmail}
          onChange={(e) => setTargetEmail(e.target.value)}
          fullWidth
          helperText="L'email sera envoyé uniquement à cette adresse"
        />

        <Button
          variant="contained"
          startIcon={sending ? <CircularProgress size={18} color="inherit" /> : <SendIcon />}
          onClick={handleSend}
          disabled={sending || !targetEmail}
          sx={{ alignSelf: 'flex-start' }}
        >
          {sending ? 'Envoi en cours...' : 'Envoyer l\'email de test'}
        </Button>
      </Box>
    </Box>
  );
};

export default AdminEmail;
