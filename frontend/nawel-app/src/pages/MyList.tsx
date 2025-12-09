import { useState, useEffect, useCallback, ChangeEvent } from 'react';
import {
  Container,
  Box,
  Typography,
  Card,
  CardContent,
  Button,
  CircularProgress,
  Alert,
  List,
  Select,
  MenuItem,
  FormControl,
  InputLabel,
  Chip,
  SelectChangeEvent,
} from '@mui/material';
import { Add as AddIcon, Download as DownloadIcon } from '@mui/icons-material';
import NavigationBar from '../components/NavigationBar';
import ManagingChildBanner from '../components/ManagingChildBanner';
import GiftFormDialog from '../components/gifts/GiftFormDialog';
import ImportDialog from '../components/gifts/ImportDialog';
import GiftListItem from '../components/gifts/GiftListItem';
import { useAuth } from '../contexts/AuthContext';
import { giftsAPI, productsAPI } from '../services/api';
import type { Gift } from '../types';
import styles from '../css/MyList.module.less';
import commonStyles from '../css/common.module.less';

interface GiftFormData {
  name: string;
  description: string;
  url: string;
  imageUrl: string;
  price: string;
}

interface ProductInfoExtracted {
  name?: string;
  description?: string;
  imageUrl?: string;
  price?: number;
}

const MyList = (): JSX.Element => {
  const { user, managingChild } = useAuth();
  const [gifts, setGifts] = useState<Gift[]>([]);
  const [availableYears, setAvailableYears] = useState<number[]>([]);
  const [selectedYear, setSelectedYear] = useState<number>(new Date().getFullYear());
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string>('');
  const [successMessage, setSuccessMessage] = useState<string>('');
  const [openDialog, setOpenDialog] = useState<boolean>(false);
  const [openImportDialog, setOpenImportDialog] = useState<boolean>(false);
  const [editingGift, setEditingGift] = useState<Gift | null>(null);
  const [formData, setFormData] = useState<GiftFormData>({
    name: '',
    description: '',
    url: '',
    imageUrl: '',
    price: '',
  });
  const [extractingInfo, setExtractingInfo] = useState<boolean>(false);
  const [extractError, setExtractError] = useState<string>('');

  useEffect(() => {
    fetchYears();
  }, []);

  useEffect(() => {
    fetchGifts();
  }, [selectedYear, managingChild]);

  const fetchYears = async (): Promise<void> => {
    try {
      const response = await giftsAPI.getAvailableYears();
      const years: number[] = response.data;
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

  const handleYearChange = (event: SelectChangeEvent<number>): void => {
    setSelectedYear(event.target.value as number);
  };

  const handleOpenDialog = useCallback((gift: Gift | null = null): void => {
    if (gift) {
      setEditingGift(gift);
      setFormData({
        name: gift.name,
        description: gift.description || '',
        url: gift.url || '',
        imageUrl: gift.imageUrl || '',
        price: gift.price ? gift.price.toString() : '',
      });
    } else {
      setEditingGift(null);
      setFormData({
        name: '',
        description: '',
        url: '',
        imageUrl: '',
        price: '',
      });
    }
    setOpenDialog(true);
  }, []);

  const handleCloseDialog = useCallback((): void => {
    setOpenDialog(false);
    setEditingGift(null);
    setExtractError('');
  }, []);

  const handleInputChange = useCallback((e: ChangeEvent<HTMLInputElement>): void => {
    const { name, value, checked, type } = e.target;
    setFormData((prev) => ({
      ...prev,
      [name]: type === 'checkbox' ? checked : value,
    }));
  }, []);

  const handleExtractInfo = async (): Promise<void> => {
    if (!formData.url || !formData.url.trim()) {
      setExtractError('Veuillez entrer une URL valide');
      return;
    }

    try {
      setExtractingInfo(true);
      setExtractError('');

      const response = await productsAPI.extractInfo(formData.url);
      const productInfo: ProductInfoExtracted = response.data;

      setFormData((prev) => ({
        ...prev,
        name: prev.name || productInfo.name || prev.name,
        description: prev.description || productInfo.description || prev.description,
        imageUrl: prev.imageUrl || productInfo.imageUrl || prev.imageUrl,
        price: prev.price || (productInfo.price ? productInfo.price.toString() : '') || prev.price,
      }));

      setSuccessMessage('Informations extraites avec succès !');
      setTimeout(() => setSuccessMessage(''), 3000);
    } catch (err: any) {
      console.error('Error extracting product info:', err);
      setExtractError(err.response?.data?.message || 'Impossible d\'extraire les informations. Vous pouvez les remplir manuellement.');
    } finally {
      setExtractingInfo(false);
    }
  };

  const handleSubmit = async (): Promise<void> => {
    try {
      if (editingGift) {
        const updateData = {
          id: editingGift.id,
          name: formData.name,
          description: formData.description || undefined,
          url: formData.url || undefined,
          imageUrl: formData.imageUrl || undefined,
          price: formData.price ? parseFloat(formData.price) : undefined,
          year: selectedYear,
        };
        await giftsAPI.updateGift(editingGift.id, updateData);
        setSuccessMessage('Cadeau modifié avec succès');
      } else {
        const createData = {
          name: formData.name,
          description: formData.description || undefined,
          url: formData.url || undefined,
          imageUrl: formData.imageUrl || undefined,
          price: formData.price ? parseFloat(formData.price) : undefined,
          year: selectedYear,
        };
        if (managingChild) {
          await giftsAPI.createGiftForChild(managingChild.userId, createData);
        } else {
          await giftsAPI.createGift(createData);
        }
        setSuccessMessage('Cadeau ajouté avec succès');
      }

      await fetchGifts();
      await fetchYears();
      handleCloseDialog();
      setError('');
      setTimeout(() => setSuccessMessage(''), 3000);
    } catch (err) {
      console.error('Error saving gift:', err);
      setError('Erreur lors de l\'enregistrement du cadeau');
    }
  };

  const handleDelete = useCallback(async (id: number): Promise<void> => {
    if (!window.confirm('Êtes-vous sûr de vouloir supprimer ce cadeau ?')) {
      return;
    }

    try {
      await giftsAPI.deleteGift(id);
      await fetchGifts();
      setSuccessMessage('Cadeau supprimé avec succès');
      setError('');
      setTimeout(() => setSuccessMessage(''), 3000);
    } catch (err) {
      console.error('Error deleting gift:', err);
      setError('Erreur lors de la suppression du cadeau');
    }
  }, [fetchGifts]);

  const handleImport = async (year: number): Promise<void> => {
    try {
      const response = await giftsAPI.importGifts(year, selectedYear);
      await fetchGifts();
      setOpenImportDialog(false);
      setSuccessMessage(`Cadeaux importés avec succès`);
      setTimeout(() => setSuccessMessage(''), 3000);
    } catch (err: any) {
      console.error('Error importing gifts:', err);
      setError(err.response?.data?.message || 'Erreur lors de l\'import');
    }
  };

  const isPastYear = selectedYear < new Date().getFullYear();
  const pastYearsForImport = availableYears.filter(y => y < new Date().getFullYear());

  if (loading && availableYears.length === 0) {
    return (
      <Box display="flex" justifyContent="center" alignItems="center" minHeight="100vh">
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Box className={commonStyles.pageRoot}>
      <NavigationBar title={managingChild ? `Liste de ${managingChild.userName}` : "Ma liste de cadeaux"} />

      <Container maxWidth="md" className={styles.container}>
        <ManagingChildBanner />

        <Box className={styles.header}>
          <Box className={styles.headerLeft}>
            <Typography variant="h4" className={styles.title}>
              {managingChild ? `Cadeaux de ${managingChild.userName}` : 'Mes cadeaux'}
            </Typography>
            <FormControl size="small" className={styles.yearSelect}>
              <InputLabel>Année</InputLabel>
              <Select
                value={selectedYear}
                label="Année"
                onChange={handleYearChange}
              >
                {availableYears.map((year) => (
                  <MenuItem key={year} value={year}>
                    {year}
                  </MenuItem>
                ))}
              </Select>
            </FormControl>
            {isPastYear && (
              <Chip label="Lecture seule" color="info" size="small" />
            )}
          </Box>
          <Box className={styles.headerRight}>
            {!isPastYear && pastYearsForImport.length > 0 && (
              <Button
                variant="outlined"
                startIcon={<DownloadIcon />}
                onClick={() => setOpenImportDialog(true)}
                fullWidth
                className={styles.actionButton}
              >
                Importer
              </Button>
            )}
            {!isPastYear && (
              <Button
                variant="contained"
                startIcon={<AddIcon />}
                onClick={() => handleOpenDialog()}
                fullWidth
                className={styles.actionButton}
              >
                Ajouter
              </Button>
            )}
          </Box>
        </Box>

        {successMessage && (
          <Alert severity="success" className={styles.alertContainer} onClose={() => setSuccessMessage('')}>
            {successMessage}
          </Alert>
        )}

        {error && (
          <Alert severity="error" className={styles.alertContainer} onClose={() => setError('')}>
            {error}
          </Alert>
        )}

        {loading ? (
          <Box display="flex" justifyContent="center" py={4}>
            <CircularProgress />
          </Box>
        ) : gifts.length === 0 ? (
          <Card>
            <CardContent className={styles.emptyState}>
              <Typography variant="body1" color="text.secondary">
                {isPastYear
                  ? `Vous n'aviez pas de cadeaux pour ${selectedYear}.`
                  : 'Vous n\'avez pas encore ajouté de cadeaux à votre liste.'}
              </Typography>
              {!isPastYear && (
                <Button
                  variant="contained"
                  startIcon={<AddIcon />}
                  onClick={() => handleOpenDialog()}
                  className={commonStyles.mt2}
                >
                  Ajouter un premier cadeau
                </Button>
              )}
            </CardContent>
          </Card>
        ) : (
          <Card>
            <List>
              {gifts.map((gift, index) => (
                <GiftListItem
                  key={gift.id}
                  gift={gift}
                  isLast={index === gifts.length - 1}
                  isPastYear={isPastYear}
                  onEdit={handleOpenDialog}
                  onDelete={handleDelete}
                />
              ))}
            </List>
          </Card>
        )}
      </Container>

      <GiftFormDialog
        open={openDialog}
        isEditing={!!editingGift}
        formData={formData}
        extractingInfo={extractingInfo}
        extractError={extractError}
        onClose={handleCloseDialog}
        onSubmit={handleSubmit}
        onInputChange={handleInputChange}
        onExtractInfo={handleExtractInfo}
      />

      <ImportDialog
        open={openImportDialog}
        availableYears={pastYearsForImport}
        onClose={() => setOpenImportDialog(false)}
        onImport={handleImport}
      />
    </Box>
  );
};

export default MyList;
