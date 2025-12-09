import axios, { AxiosInstance, AxiosResponse } from 'axios';
import type {
  LoginCredentials,
  LoginResponse,
  User,
  UpdateProfileData,
  ChangePasswordData,
  GiftList,
  Gift,
  CreateGiftData,
  UpdateGiftData,
  ProductInfo,
  ReserveGiftData,
  Family,
  CreateFamilyData,
  UpdateFamilyData,
  AllListsResponse,
} from '../types';

const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:5284/api';

// Create axios instance
const api: AxiosInstance = axios.create({
  baseURL: API_URL,
});

// Request interceptor to add auth token
api.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('token');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

// Response interceptor to handle errors
api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      // Token expired or invalid, logout user
      localStorage.removeItem('token');
      localStorage.removeItem('user');
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);

// Auth APIs
export const authAPI = {
  login: (credentials: LoginCredentials): Promise<AxiosResponse<LoginResponse>> =>
    api.post('/auth/login', credentials),
  validateToken: (token: string): Promise<AxiosResponse<{ valid: boolean }>> =>
    api.get(`/auth/validate-token?token=${token}`),
  requestPasswordReset: (email: string): Promise<AxiosResponse<{ message: string }>> =>
    api.post('/auth/reset-password-request', { email }),
  resetPassword: (data: { token: string; newPassword: string }): Promise<AxiosResponse<{ message: string }>> =>
    api.post('/auth/reset-password', data),
  requestMigrationReset: (data: { login: string }): Promise<AxiosResponse<{ message: string }>> =>
    api.post('/auth/request-migration-reset', data),
};

// Users APIs
export const usersAPI = {
  getMe: (): Promise<AxiosResponse<User>> => api.get('/users/me'),
  getById: (id: number): Promise<AxiosResponse<User>> => api.get(`/users/${id}`),
  updateMe: (data: UpdateProfileData): Promise<AxiosResponse<User>> => api.put('/users/me', data),
  changePassword: (data: ChangePasswordData): Promise<AxiosResponse<{ message: string }>> =>
    api.post('/users/me/change-password', data),
  uploadAvatar: (file: File): Promise<AxiosResponse<{ avatarUrl: string }>> => {
    const formData = new FormData();
    formData.append('file', file);
    return api.post('/users/me/avatar', formData);
  },
  deleteAvatar: (): Promise<AxiosResponse<{ message: string }>> => api.delete('/users/me/avatar'),
};

// Lists APIs
export const listsAPI = {
  getAll: (): Promise<AxiosResponse<AllListsResponse>> => api.get('/lists'),
  getMine: (): Promise<AxiosResponse<GiftList>> => api.get('/lists/mine'),
};

// Gifts APIs
export const giftsAPI = {
  getMyGifts: (year: number): Promise<AxiosResponse<Gift[]>> =>
    api.get('/gifts/my-list', { params: { year } }),
  getChildGifts: (childId: number, year: number): Promise<AxiosResponse<Gift[]>> =>
    api.get(`/gifts/manage-child/${childId}`, { params: { year } }),
  getUserGifts: (userId: number, year: number): Promise<AxiosResponse<Gift[]>> =>
    api.get(`/gifts/${userId}`, { params: { year } }),
  getAvailableYears: (): Promise<AxiosResponse<number[]>> => api.get('/gifts/years'),
  importGifts: (fromYear: number, toYear: number): Promise<AxiosResponse<{ message: string }>> =>
    api.post(`/gifts/import/${fromYear}/${toYear}`),
  createGift: (data: CreateGiftData): Promise<AxiosResponse<Gift>> => api.post('/gifts', data),
  createGiftForChild: (childId: number, data: CreateGiftData): Promise<AxiosResponse<Gift>> =>
    api.post(`/gifts/manage-child/${childId}`, data),
  updateGift: (id: number, data: UpdateGiftData): Promise<AxiosResponse<Gift>> =>
    api.put(`/gifts/${id}`, data),
  deleteGift: (id: number): Promise<AxiosResponse<{ message: string }>> => api.delete(`/gifts/${id}`),
  reserveGift: (id: number, data: ReserveGiftData): Promise<AxiosResponse<{ message: string }>> =>
    api.post(`/gifts/${id}/reserve`, data),
  unreserveGift: (id: number): Promise<AxiosResponse<{ message: string }>> =>
    api.delete(`/gifts/${id}/reserve`),
};

// Products APIs
export const productsAPI = {
  extractInfo: (url: string): Promise<AxiosResponse<ProductInfo>> =>
    api.post('/products/extract-info', { url }),
};

// Admin APIs
interface MonthlyRequest {
  month: number;
  year: number;
  count: number;
  successCount: number;
}

interface AdminStats {
  totalUsers: number;
  totalFamilies: number;
  totalGifts: number;
  totalReservedGifts: number;
  openGraphRequestsThisMonth: number;
  requestsByMonth: MonthlyRequest[];
}

export type { AdminStats };

export const adminAPI = {
  getStats: (): Promise<AxiosResponse<AdminStats>> => api.get('/admin/stats'),
  // Users
  getUsers: (): Promise<AxiosResponse<User[]>> => api.get('/admin/users'),
  createUser: (data: Partial<User>): Promise<AxiosResponse<User>> => api.post('/admin/users', data),
  updateUser: (id: number, data: Partial<User>): Promise<AxiosResponse<User>> =>
    api.put(`/admin/users/${id}`, data),
  deleteUser: (id: number): Promise<AxiosResponse<{ message: string }>> => api.delete(`/admin/users/${id}`),
  // Families
  getFamilies: (): Promise<AxiosResponse<Family[]>> => api.get('/admin/families'),
  createFamily: (data: CreateFamilyData): Promise<AxiosResponse<Family>> => api.post('/admin/families', data),
  updateFamily: (id: number, data: UpdateFamilyData): Promise<AxiosResponse<Family>> =>
    api.put(`/admin/families/${id}`, data),
  deleteFamily: (id: number): Promise<AxiosResponse<{ message: string }>> =>
    api.delete(`/admin/families/${id}`),
};

export default api;
