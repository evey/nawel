import type { Gift, User } from '../types';

/**
 * Gift-related helper functions
 */

export interface GiftStatus {
  label: string;
  color: 'success' | 'warning' | 'error' | 'default' | 'primary' | 'secondary' | 'info';
}

/**
 * Check if the current user is participating in a group gift
 */
export const isParticipating = (gift: Gift | null | undefined, user: User | null | undefined): boolean => {
  if (!gift?.isGroupGift || !gift?.participantNames || !user) return false;
  const userDisplayName = user.firstName || user.login;
  return gift.participantNames.includes(userDisplayName);
};

/**
 * Format a price with currency
 */
export const formatPrice = (price: number | null | undefined, currency: string = '€'): string => {
  if (price === null || price === undefined) return 'Prix non renseigné';
  return `${price.toFixed(2)} ${currency}`;
};

/**
 * Get the status label for a gift
 */
export const getGiftStatus = (gift: Gift, user: User | null): GiftStatus => {
  if (gift.isTaken) {
    if (gift.isGroupGift) {
      if (isParticipating(gift, user)) {
        return { label: 'Vous participez', color: 'success' };
      }
      return { label: `Cadeau partagé (${gift.participantCount})`, color: 'warning' };
    }
    return { label: 'Réservé', color: 'error' };
  }
  return { label: 'Disponible', color: 'success' };
};

/**
 * Check if a gift URL is valid
 */
export const isValidUrl = (url: string | null | undefined): boolean => {
  if (!url) return false;
  try {
    new URL(url);
    return true;
  } catch {
    return false;
  }
};
