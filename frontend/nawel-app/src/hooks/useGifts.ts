import { useState, useEffect } from 'react';
import { giftsAPI, productsAPI } from '../services/api';
import type { Gift, CreateGiftData, UpdateGiftData, ProductInfo, ManagingChild } from '../types';

interface UseGiftsReturn {
  gifts: Gift[];
  availableYears: number[];
  loading: boolean;
  error: string;
  successMessage: string;
  setError: (error: string) => void;
  setSuccessMessage: (message: string) => void;
  fetchGifts: () => Promise<void>;
  extractProductInfo: (url: string) => Promise<ProductInfo>;
  saveGift: (giftData: CreateGiftData | UpdateGiftData, isEditing: boolean) => Promise<boolean>;
  deleteGift: (giftId: number) => Promise<boolean>;
  importGifts: (fromYear: number) => Promise<boolean>;
}

/**
 * Custom hook for managing gifts data and operations
 */
export const useGifts = (selectedYear: number, managingChild: ManagingChild | null): UseGiftsReturn => {
  const [gifts, setGifts] = useState<Gift[]>([]);
  const [availableYears, setAvailableYears] = useState<number[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [successMessage, setSuccessMessage] = useState('');

  useEffect(() => {
    fetchYears();
  }, []);

  useEffect(() => {
    fetchGifts();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [selectedYear, managingChild]);

  const fetchYears = async (): Promise<void> => {
    try {
      const response = await giftsAPI.getAvailableYears();
      const years = response.data;
      if (years.length === 0) {
        years.push(new Date().getFullYear());
      }
      setAvailableYears(years);
    } catch (err) {
      console.error('Error fetching years:', err);
      setAvailableYears([new Date().getFullYear()]);
    }
  };

  const fetchGifts = async (): Promise<void> => {
    try {
      setLoading(true);
      const response = managingChild
        ? await giftsAPI.getChildGifts(managingChild.userId, selectedYear)
        : await giftsAPI.getMyGifts(selectedYear);
      setGifts(response.data);
      setError('');
    } catch (err) {
      console.error('Error fetching gifts:', err);
      setError('Erreur lors du chargement des cadeaux');
    } finally {
      setLoading(false);
    }
  };

  const extractProductInfo = async (url: string): Promise<ProductInfo> => {
    try {
      const response = await productsAPI.extractInfo(url);
      return response.data;
    } catch (err) {
      console.error('Error extracting product info:', err);
      throw err;
    }
  };

  const saveGift = async (giftData: CreateGiftData | UpdateGiftData, isEditing: boolean): Promise<boolean> => {
    try {
      if (isEditing) {
        await giftsAPI.updateGift((giftData as UpdateGiftData).id, giftData as UpdateGiftData);
        setSuccessMessage('Cadeau modifié avec succès');
      } else {
        await giftsAPI.createGift(giftData as CreateGiftData);
        setSuccessMessage('Cadeau ajouté avec succès');
      }
      await fetchGifts();
      setError('');
      setTimeout(() => setSuccessMessage(''), 3000);
      return true;
    } catch (err: any) {
      console.error('Error saving gift:', err);
      setError(err.response?.data?.message || 'Erreur lors de l\'enregistrement');
      return false;
    }
  };

  const deleteGift = async (giftId: number): Promise<boolean> => {
    try {
      await giftsAPI.deleteGift(giftId);
      await fetchGifts();
      setSuccessMessage('Cadeau supprimé avec succès');
      setError('');
      setTimeout(() => setSuccessMessage(''), 3000);
      return true;
    } catch (err: any) {
      console.error('Error deleting gift:', err);
      setError(err.response?.data?.message || 'Erreur lors de la suppression');
      return false;
    }
  };

  const importGifts = async (fromYear: number): Promise<boolean> => {
    try {
      await giftsAPI.importGifts(fromYear, selectedYear);
      await fetchGifts();
      setSuccessMessage(`Cadeaux importés de ${fromYear} avec succès`);
      setError('');
      setTimeout(() => setSuccessMessage(''), 3000);
      return true;
    } catch (err: any) {
      console.error('Error importing gifts:', err);
      setError(err.response?.data?.message || 'Erreur lors de l\'import');
      return false;
    }
  };

  return {
    gifts,
    availableYears,
    loading,
    error,
    successMessage,
    setError,
    setSuccessMessage,
    fetchGifts,
    extractProductInfo,
    saveGift,
    deleteGift,
    importGifts,
  };
};
