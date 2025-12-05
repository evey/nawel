import { createContext, useState, useContext, useEffect } from 'react';
import { authAPI } from '../services/api';

const AuthContext = createContext(null);

export const AuthProvider = ({ children }) => {
  const [user, setUser] = useState(null);
  const [loading, setLoading] = useState(true);
  const [token, setToken] = useState(localStorage.getItem('token'));
  const [managingChild, setManagingChild] = useState(null); // { userId, userName }

  useEffect(() => {
    // Check if user is already logged in
    const storedUser = localStorage.getItem('user');
    const storedToken = localStorage.getItem('token');
    const storedManagingChild = localStorage.getItem('managingChild');

    if (storedUser && storedToken) {
      setUser(JSON.parse(storedUser));
      setToken(storedToken);
    }

    if (storedManagingChild) {
      setManagingChild(JSON.parse(storedManagingChild));
    }

    setLoading(false);
  }, []);

  const login = async (credentials) => {
    try {
      const response = await authAPI.login(credentials);
      const { token, user } = response.data;

      localStorage.setItem('token', token);
      localStorage.setItem('user', JSON.stringify(user));

      setToken(token);
      setUser(user);

      return { success: true };
    } catch (error) {
      console.error('Login error:', error);
      return {
        success: false,
        error: error.response?.data?.message || 'Login failed',
      };
    }
  };

  const logout = () => {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    localStorage.removeItem('managingChild');
    setToken(null);
    setUser(null);
    setManagingChild(null);
  };

  const updateUser = (updatedUser) => {
    setUser(updatedUser);
    localStorage.setItem('user', JSON.stringify(updatedUser));
  };

  const startManagingChild = (childInfo) => {
    // childInfo: { userId, userName, avatarUrl }
    console.log('[AuthContext] startManagingChild called with:', childInfo);
    setManagingChild(childInfo);
    localStorage.setItem('managingChild', JSON.stringify(childInfo));
    console.log('[AuthContext] managingChild state updated and saved to localStorage');
  };

  const stopManagingChild = () => {
    setManagingChild(null);
    localStorage.removeItem('managingChild');
  };

  const value = {
    user,
    token,
    loading,
    managingChild,
    login,
    logout,
    updateUser,
    startManagingChild,
    stopManagingChild,
    isAuthenticated: !!token,
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
};

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};
