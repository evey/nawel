import axios from 'axios';

const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:5284/api';

// Create axios instance
const api = axios.create({
  baseURL: API_URL,
  headers: {
    'Content-Type': 'application/json',
  },
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
  updateMe: (data) => api.put('/users/me', data),
  changePassword: (data) => api.post('/users/me/change-password', data),
};

// Lists APIs
export const listsAPI = {
  getAll: () => api.get('/lists'),
  getMine: () => api.get('/lists/mine'),
};

// Gifts APIs
export const giftsAPI = {
  getMyGifts: (year) => api.get('/gifts/my-list', { params: { year } }),
  getUserGifts: (userId, year) => api.get(`/gifts/${userId}`, { params: { year } }),
  getAvailableYears: () => api.get('/gifts/years'),
  importFromYear: (year) => api.post(`/gifts/import-from-year/${year}`),
  createGift: (data) => api.post('/gifts', data),
  updateGift: (id, data) => api.put(`/gifts/${id}`, data),
  deleteGift: (id) => api.delete(`/gifts/${id}`),
  reserveGift: (id) => api.post(`/gifts/${id}/reserve`),
  unreserveGift: (id) => api.delete(`/gifts/${id}/reserve`),
};

export default api;
