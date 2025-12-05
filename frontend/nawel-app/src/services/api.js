import axios from 'axios';

const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:5284/api';

// Create axios instance
const api = axios.create({
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
  login: (credentials) => api.post('/auth/login', credentials),
  validateToken: (token) => api.get(`/auth/validate-token?token=${token}`),
  requestPasswordReset: (email) => api.post('/auth/reset-password-request', { email }),
  resetPassword: (data) => api.post('/auth/reset-password', data),
};

// Users APIs
export const usersAPI = {
  getMe: () => api.get('/users/me'),
  getById: (id) => api.get(`/users/${id}`),
  updateMe: (data) => api.put('/users/me', data),
  changePassword: (data) => api.post('/users/me/change-password', data),
  uploadAvatar: (file) => {
    const formData = new FormData();
    formData.append('file', file);
    return api.post('/users/me/avatar', formData);
  },
  deleteAvatar: () => api.delete('/users/me/avatar'),
};

// Lists APIs
export const listsAPI = {
  getAll: () => api.get('/lists'),
  getMine: () => api.get('/lists/mine'),
};

// Gifts APIs
export const giftsAPI = {
  getMyGifts: (year) => api.get('/gifts/my-list', { params: { year } }),
  getChildGifts: (childId, year) => api.get(`/gifts/manage-child/${childId}`, { params: { year } }),
  getUserGifts: (userId, year) => api.get(`/gifts/${userId}`, { params: { year } }),
  getAvailableYears: () => api.get('/gifts/years'),
  importFromYear: (year) => api.post(`/gifts/import-from-year/${year}`),
  createGift: (data) => api.post('/gifts', data),
  createGiftForChild: (childId, data) => api.post(`/gifts/manage-child/${childId}`, data),
  updateGift: (id, data) => api.put(`/gifts/${id}`, data),
  deleteGift: (id) => api.delete(`/gifts/${id}`),
  reserveGift: (id, data) => api.post(`/gifts/${id}/reserve`, data),
  unreserveGift: (id) => api.delete(`/gifts/${id}/reserve`),
};

// Products APIs
export const productsAPI = {
  extractInfo: (url) => api.post('/products/extract-info', { url }),
};

// Admin APIs
export const adminAPI = {
  getStats: () => api.get('/admin/stats'),
  // Users
  getUsers: () => api.get('/admin/users'),
  createUser: (data) => api.post('/admin/users', data),
  updateUser: (id, data) => api.put(`/admin/users/${id}`, data),
  deleteUser: (id) => api.delete(`/admin/users/${id}`),
  // Families
  getFamilies: () => api.get('/admin/families'),
  createFamily: (data) => api.post('/admin/families', data),
  updateFamily: (id, data) => api.put(`/admin/families/${id}`, data),
  deleteFamily: (id) => api.delete(`/admin/families/${id}`),
};

export default api;
