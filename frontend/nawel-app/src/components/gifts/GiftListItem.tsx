import { memo } from 'react';
import { ListItem, ListItemText, Box, Typography, IconButton } from '@mui/material';
import { Edit as EditIcon, Delete as DeleteIcon } from '@mui/icons-material';
import type { Gift } from '../../types';
import styles from '../../css/GiftListItem.module.less';

interface GiftListItemProps {
  gift: Gift;
  isLast: boolean;
  isPastYear: boolean;
  onEdit: (gift: Gift) => void;
  onDelete: (id: number) => void;
}

const GiftListItem = memo(({
  gift,
  isLast,
  isPastYear,
  onEdit,
  onDelete,
}: GiftListItemProps): JSX.Element => {
  return (
    <ListItem
      divider={!isLast}
      className={styles.listItem}
    >
      {gift.imageUrl && (
        <Box
          component="img"
          src={gift.imageUrl}
          alt={gift.name}
          className={styles.giftImage}
          onError={(e: React.SyntheticEvent<HTMLImageElement, Event>) => {
            e.currentTarget.style.display = 'none';
          }}
        />
      )}
      <ListItemText
        className={styles.listItemText}
        primary={
          <Box className={styles.titleContainer}>
            <Typography variant="h6" className={styles.giftTitle}>
              {gift.name}
            </Typography>
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
          </Box>
        }
      />
      {!isPastYear && (
        <Box className={styles.actions}>
          <IconButton
            onClick={() => onEdit(gift)}
            size="medium"
            className={styles.actionButton}
          >
            <EditIcon />
          </IconButton>
          <IconButton
            onClick={() => onDelete(gift.id)}
            color="error"
            size="medium"
            className={styles.actionButton}
          >
            <DeleteIcon />
          </IconButton>
        </Box>
      )}
    </ListItem>
  );
});

GiftListItem.displayName = 'GiftListItem';

export default GiftListItem;
