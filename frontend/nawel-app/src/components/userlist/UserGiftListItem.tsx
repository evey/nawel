import { memo } from 'react';
import {
  ListItem,
  ListItemText,
  Box,
  Typography,
  Chip,
  Button,
  Tooltip,
  Alert,
} from '@mui/material';
import {
  ShoppingCart as ShoppingCartIcon,
  Group as GroupIcon,
  CheckCircle as CheckCircleIcon,
} from '@mui/icons-material';
import { isParticipating } from '../../utils/giftHelpers';
import type { Gift, User } from '../../types';
import styles from '../../css/UserGiftListItem.module.less';

interface UserGiftListItemProps {
  gift: Gift;
  index: number;
  totalGifts: number;
  currentUser: User | null;
  isPastYear: boolean;
  onReserve: (gift: Gift) => void;
  onUnreserve: (giftId: number) => void;
}

const UserGiftListItem = memo(({
  gift,
  index,
  totalGifts,
  currentUser,
  isPastYear,
  onReserve,
  onUnreserve,
}: UserGiftListItemProps): JSX.Element => {
  const isReservedByMe = (): boolean => {
    if (gift.isGroupGift) {
      return gift.participantCount > 0 && gift.takenByUserId === currentUser?.id;
    }
    return gift.isTaken && gift.takenByUserId === currentUser?.id;
  };

  const reservedByMe = isReservedByMe();
  const participatingInGroup = gift.isGroupGift && isParticipating(gift, currentUser);
  const reservedBySomeoneElse = gift.isTaken && !reservedByMe && !gift.isGroupGift;
  const canReserve = !isPastYear && (!gift.isTaken || (gift.isGroupGift && !participatingInGroup));

  return (
    <ListItem
      divider={index < totalGifts - 1}
      className={styles.listItem}
    >
      <ListItemText
        primary={
          <Box className={styles.titleContainer}>
            <Typography variant="h6" className={styles.giftTitle}>{gift.name}</Typography>
            {gift.isGroupGift && (
              <Tooltip
                title={gift.participantNames && gift.participantNames.length > 0
                  ? `Participants : ${gift.participantNames.join(', ')}`
                  : 'Cadeau groupé'}
                arrow
              >
                <Chip
                  icon={<GroupIcon />}
                  label={`Cadeau groupé (${gift.participantCount} participant${gift.participantCount !== 1 ? 's' : ''})`}
                  color={isParticipating(gift, currentUser) ? 'success' : 'warning'}
                  size="small"
                />
              </Tooltip>
            )}
            {gift.isTaken && !gift.isGroupGift && (
              <Chip
                icon={<CheckCircleIcon />}
                label={reservedByMe ? 'Réservé par vous' : `Réservé par ${gift.takenByUserName || 'quelqu\'un'}`}
                color={reservedByMe ? 'success' : 'warning'}
                size="small"
              />
            )}
          </Box>
        }
        secondary={
          <Box className={styles.secondaryInfo}>
            {gift.description && (
              <Typography variant="body2" color="text.secondary" className={styles.description}>
                {gift.description}
              </Typography>
            )}
            {gift.url && (
              <Typography variant="body2" className={styles.link}>
                <a href={gift.url} target="_blank" rel="noopener noreferrer">
                  Voir le lien
                </a>
              </Typography>
            )}
            {gift.price && (
              <Typography variant="body2" className={styles.price}>
                Prix: {gift.price.toFixed(2)} €
              </Typography>
            )}
            {gift.comment && gift.isTaken && (
              <Alert severity="info" className={styles.commentAlert}>
                <strong>Commentaire :</strong>
                <Box component="span" className={styles.commentContent}>
                  {gift.comment}
                </Box>
              </Alert>
            )}
          </Box>
        }
      />
      {!isPastYear && (
        <Box className={styles.actions}>
          {reservedByMe || participatingInGroup ? (
            <Button
              variant="outlined"
              color="error"
              onClick={() => onUnreserve(gift.id)}
              size="small"
              fullWidth
              className={styles.actionButton}
            >
              Annuler
            </Button>
          ) : canReserve ? (
            <Button
              variant="contained"
              color={gift.isGroupGift ? 'secondary' : 'primary'}
              onClick={() => onReserve(gift)}
              size="small"
              fullWidth
              startIcon={gift.isGroupGift ? <GroupIcon /> : <ShoppingCartIcon />}
              className={styles.actionButton}
            >
              {gift.isGroupGift ? 'Participer' : 'Réserver'}
            </Button>
          ) : reservedBySomeoneElse ? (
            <Button
              variant="contained"
              color="secondary"
              onClick={() => onReserve(gift)}
              size="small"
              fullWidth
              startIcon={<GroupIcon />}
              className={styles.actionButton}
            >
              Participer
            </Button>
          ) : (
            <Chip label="Non disponible" size="small" />
          )}
        </Box>
      )}
    </ListItem>
  );
});

UserGiftListItem.displayName = 'UserGiftListItem';

export default UserGiftListItem;
