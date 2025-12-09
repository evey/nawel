import { ChangeEvent, memo } from 'react';
import {
  Card,
  CardContent,
  Typography,
  Box,
  TextField,
  FormControlLabel,
  Checkbox,
} from '@mui/material';
import styles from '../../css/ProfileForm.module.less';

interface ProfileFormProps {
  login: string;
  firstName: string;
  lastName: string;
  email: string;
  pseudo: string;
  notifyListEdit: boolean;
  notifyGiftTaken: boolean;
  displayPopup: boolean;
  onChange: (e: ChangeEvent<HTMLInputElement>) => void;
}

const ProfileForm = memo(({
  login,
  firstName,
  lastName,
  email,
  pseudo,
  notifyListEdit,
  notifyGiftTaken,
  displayPopup,
  onChange,
}: ProfileFormProps): JSX.Element => {
  return (
    <>
      <Card className={styles.card}>
        <CardContent>
          <Typography variant="h6" gutterBottom>
            Informations personnelles
          </Typography>
          <Box className={styles.fieldsContainer}>
            <TextField
              label="Login"
              value={login}
              disabled
              fullWidth
              helperText="Le login ne peut pas être modifié"
            />
            <TextField
              name="firstName"
              label="Prénom"
              value={firstName}
              onChange={onChange}
              fullWidth
            />
            <TextField
              name="lastName"
              label="Nom"
              value={lastName}
              onChange={onChange}
              fullWidth
            />
            <TextField
              name="email"
              label="Email"
              type="email"
              value={email}
              onChange={onChange}
              fullWidth
            />
            <TextField
              name="pseudo"
              label="Pseudo"
              value={pseudo}
              onChange={onChange}
              fullWidth
            />
          </Box>
        </CardContent>
      </Card>

      <Card className={styles.card}>
        <CardContent>
          <Typography variant="h6" gutterBottom>
            Préférences de notifications
          </Typography>
          <Box className={styles.checkboxContainer}>
            <FormControlLabel
              control={
                <Checkbox
                  name="notifyListEdit"
                  checked={notifyListEdit}
                  onChange={onChange}
                />
              }
              label="M'envoyer un email quand quelqu'un modifie sa liste"
            />
            <FormControlLabel
              control={
                <Checkbox
                  name="notifyGiftTaken"
                  checked={notifyGiftTaken}
                  onChange={onChange}
                />
              }
              label="M'envoyer un email quand quelqu'un réserve un cadeau"
            />
            <FormControlLabel
              control={
                <Checkbox
                  name="displayPopup"
                  checked={displayPopup}
                  onChange={onChange}
                />
              }
              label="Afficher les popups d'information"
            />
          </Box>
        </CardContent>
      </Card>
    </>
  );
});

ProfileForm.displayName = 'ProfileForm';

export default ProfileForm;
