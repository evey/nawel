import { describe, it, expect } from 'vitest';
import { isParticipating, formatPrice, getGiftStatus, isValidUrl } from './giftHelpers';
import type { Gift, User } from '../types';

describe('giftHelpers', () => {
  describe('isParticipating', () => {
    it('should return false when gift is null or undefined', () => {
      const user: User = {
        id: 1,
        login: 'testuser',
        firstName: 'Test',
        lastName: 'User',
        avatar: 'avatar.png',
        isChildren: false,
        isAdmin: false,
        familyId: 1,
        familyName: 'Test Family',
        email: 'test@example.com',
        notifyListEdit: false,
        notifyGiftTaken: false,
        displayPopup: true,
        createdAt: '2024-01-01T00:00:00Z',
        updatedAt: '2024-01-01T00:00:00Z',
      };

      expect(isParticipating(null, user)).toBe(false);
      expect(isParticipating(undefined, user)).toBe(false);
    });

    it('should return false when user is null or undefined', () => {
      const gift: Gift = {
        id: 1,
        name: 'Test Gift',
        year: 2025,
        isTaken: false,
        isGroupGift: true,
        participantCount: 0,
        participantNames: [],
      };

      expect(isParticipating(gift, null)).toBe(false);
      expect(isParticipating(gift, undefined)).toBe(false);
    });

    it('should return false when gift is not a group gift', () => {
      const gift: Gift = {
        id: 1,
        name: 'Test Gift',
        year: 2025,
        isTaken: false,
        isGroupGift: false,
        participantCount: 0,
        participantNames: [],
      };

      const user: User = {
        id: 1,
        login: 'testuser',
        firstName: 'Test',
        lastName: 'User',
        avatar: 'avatar.png',
        isChildren: false,
        isAdmin: false,
        familyId: 1,
        familyName: 'Test Family',
        email: 'test@example.com',
        notifyListEdit: false,
        notifyGiftTaken: false,
        displayPopup: true,
        createdAt: '2024-01-01T00:00:00Z',
        updatedAt: '2024-01-01T00:00:00Z',
      };

      expect(isParticipating(gift, user)).toBe(false);
    });

    it('should return true when user is participating in group gift using firstName', () => {
      const gift: Gift = {
        id: 1,
        name: 'Test Gift',
        year: 2025,
        isTaken: false,
        isGroupGift: true,
        participantCount: 2,
        participantNames: ['Test', 'John'],
      };

      const user: User = {
        id: 1,
        login: 'testuser',
        firstName: 'Test',
        lastName: 'User',
        avatar: 'avatar.png',
        isChildren: false,
        isAdmin: false,
        familyId: 1,
        familyName: 'Test Family',
        email: 'test@example.com',
        notifyListEdit: false,
        notifyGiftTaken: false,
        displayPopup: true,
        createdAt: '2024-01-01T00:00:00Z',
        updatedAt: '2024-01-01T00:00:00Z',
      };

      expect(isParticipating(gift, user)).toBe(true);
    });

    it('should return true when user is participating using login (no firstName)', () => {
      const gift: Gift = {
        id: 1,
        name: 'Test Gift',
        year: 2025,
        isTaken: false,
        isGroupGift: true,
        participantCount: 1,
        participantNames: ['testuser'],
      };

      const user: User = {
        id: 1,
        login: 'testuser',
        firstName: '',
        lastName: 'User',
        avatar: 'avatar.png',
        isChildren: false,
        isAdmin: false,
        familyId: 1,
        familyName: 'Test Family',
        email: 'test@example.com',
        notifyListEdit: false,
        notifyGiftTaken: false,
        displayPopup: true,
        createdAt: '2024-01-01T00:00:00Z',
        updatedAt: '2024-01-01T00:00:00Z',
      };

      expect(isParticipating(gift, user)).toBe(true);
    });

    it('should return false when user is not participating', () => {
      const gift: Gift = {
        id: 1,
        name: 'Test Gift',
        year: 2025,
        isTaken: false,
        isGroupGift: true,
        participantCount: 2,
        participantNames: ['John', 'Jane'],
      };

      const user: User = {
        id: 1,
        login: 'testuser',
        firstName: 'Test',
        lastName: 'User',
        avatar: 'avatar.png',
        isChildren: false,
        isAdmin: false,
        familyId: 1,
        familyName: 'Test Family',
        email: 'test@example.com',
        notifyListEdit: false,
        notifyGiftTaken: false,
        displayPopup: true,
        createdAt: '2024-01-01T00:00:00Z',
        updatedAt: '2024-01-01T00:00:00Z',
      };

      expect(isParticipating(gift, user)).toBe(false);
    });
  });

  describe('formatPrice', () => {
    it('should format a price with default currency', () => {
      expect(formatPrice(49.99)).toBe('49.99 €');
      expect(formatPrice(100)).toBe('100.00 €');
      expect(formatPrice(9.5)).toBe('9.50 €');
    });

    it('should format a price with custom currency', () => {
      expect(formatPrice(49.99, '$')).toBe('49.99 $');
      expect(formatPrice(100, 'USD')).toBe('100.00 USD');
    });

    it('should handle null price', () => {
      expect(formatPrice(null)).toBe('Prix non renseigné');
    });

    it('should handle undefined price', () => {
      expect(formatPrice(undefined)).toBe('Prix non renseigné');
    });

    it('should format zero price', () => {
      expect(formatPrice(0)).toBe('0.00 €');
    });

    it('should format decimal prices correctly', () => {
      expect(formatPrice(12.345)).toBe('12.35 €'); // Rounded
      expect(formatPrice(12.344)).toBe('12.34 €'); // Rounded
    });
  });

  describe('getGiftStatus', () => {
    const user: User = {
      id: 1,
      login: 'testuser',
      firstName: 'Test',
      lastName: 'User',
      avatar: 'avatar.png',
      isChildren: false,
      isAdmin: false,
      familyId: 1,
      familyName: 'Test Family',
      email: 'test@example.com',
      notifyListEdit: false,
      notifyGiftTaken: false,
      displayPopup: true,
      createdAt: '2024-01-01T00:00:00Z',
      updatedAt: '2024-01-01T00:00:00Z',
    };

    it('should return "Disponible" for available gifts', () => {
      const gift: Gift = {
        id: 1,
        name: 'Test Gift',
        year: 2025,
        isTaken: false,
        isGroupGift: false,
        participantCount: 0,
        participantNames: [],
      };

      const status = getGiftStatus(gift, user);
      expect(status.label).toBe('Disponible');
      expect(status.color).toBe('success');
    });

    it('should return "Réservé" for taken non-group gifts', () => {
      const gift: Gift = {
        id: 1,
        name: 'Test Gift',
        year: 2025,
        isTaken: true,
        isGroupGift: false,
        participantCount: 0,
        participantNames: [],
      };

      const status = getGiftStatus(gift, user);
      expect(status.label).toBe('Réservé');
      expect(status.color).toBe('error');
    });

    it('should return "Vous participez" when user is participating in group gift', () => {
      const gift: Gift = {
        id: 1,
        name: 'Test Gift',
        year: 2025,
        isTaken: true,
        isGroupGift: true,
        participantCount: 2,
        participantNames: ['Test', 'John'],
      };

      const status = getGiftStatus(gift, user);
      expect(status.label).toBe('Vous participez');
      expect(status.color).toBe('success');
    });

    it('should return "Cadeau partagé" when user is not participating in group gift', () => {
      const gift: Gift = {
        id: 1,
        name: 'Test Gift',
        year: 2025,
        isTaken: true,
        isGroupGift: true,
        participantCount: 2,
        participantNames: ['John', 'Jane'],
      };

      const status = getGiftStatus(gift, user);
      expect(status.label).toBe('Cadeau partagé (2)');
      expect(status.color).toBe('warning');
    });

    it('should handle null user for group gifts', () => {
      const gift: Gift = {
        id: 1,
        name: 'Test Gift',
        year: 2025,
        isTaken: true,
        isGroupGift: true,
        participantCount: 3,
        participantNames: ['John', 'Jane', 'Bob'],
      };

      const status = getGiftStatus(gift, null);
      expect(status.label).toBe('Cadeau partagé (3)');
      expect(status.color).toBe('warning');
    });
  });

  describe('isValidUrl', () => {
    it('should return true for valid URLs', () => {
      expect(isValidUrl('https://example.com')).toBe(true);
      expect(isValidUrl('http://example.com')).toBe(true);
      expect(isValidUrl('https://example.com/path/to/page')).toBe(true);
      expect(isValidUrl('https://example.com:8080')).toBe(true);
      expect(isValidUrl('https://example.com?param=value')).toBe(true);
      expect(isValidUrl('https://subdomain.example.com')).toBe(true);
    });

    it('should return false for invalid URLs', () => {
      expect(isValidUrl('not a url')).toBe(false);
      expect(isValidUrl('example.com')).toBe(false);
      expect(isValidUrl('//example.com')).toBe(false);
      expect(isValidUrl('just text')).toBe(false);
    });

    it('should return false for null or undefined', () => {
      expect(isValidUrl(null)).toBe(false);
      expect(isValidUrl(undefined)).toBe(false);
    });

    it('should return false for empty string', () => {
      expect(isValidUrl('')).toBe(false);
    });

    it('should return true for ftp URLs', () => {
      expect(isValidUrl('ftp://example.com')).toBe(true);
    });
  });
});
