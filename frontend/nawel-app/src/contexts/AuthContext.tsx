import { createContext, useState, useContext, useEffect, ReactNode } from 'react';
import { authAPI } from '../services/api';
import type { User, LoginCredentials, ManagingChild } from '../types';

interface LoginResult {
  success: boolean;
  error?: string;
  errorCode?: string;
  email?: string;
}

interface AuthContextType {
  user: User | null;
  token: string | null;
  loading: boolean;
  managingChild: ManagingChild | null;
  login: (credentials: LoginCredentials) => Promise<LoginResult>;
  logout: () => void;
  updateUser: (updatedUser: User) => void;
  startManagingChild: (childInfo: ManagingChild) => void;
  stopManagingChild: () => void;
  isAuthenticated: boolean;
}

const AuthContext = createContext<AuthContextType | null>(null);

interface AuthProviderProps {
  children: ReactNode;
}

export const AuthProvider = ({ children }: AuthProviderProps) => {
  const [user, setUser] = useState<User | null>(null);
  const [loading, setLoading] = useState(true);
  const [token, setToken] = useState<string | null>(localStorage.getItem('token'));
  const [managingChild, setManagingChild] = useState<ManagingChild | null>(null);

  useEffect(() => {
    // Check if user is already logged in
    const storedUser = localStorage.getItem('user');
    const storedToken = localStorage.getItem('token');
    const storedManagingChild = localStorage.getItem('managingChild');

    if (storedUser && storedToken) {
      setUser(JSON.parse(storedUser) as User);
      setToken(storedToken);
    }

    if (storedManagingChild) {
      setManagingChild(JSON.parse(storedManagingChild) as ManagingChild);
    }

    setLoading(false);
  }, []);

  const login = async (credentials: LoginCredentials): Promise<LoginResult> => {
    try {
      const response = await authAPI.login(credentials);
      const { token, user } = response.data;

      localStorage.setItem('token', token);
      localStorage.setItem('user', JSON.stringify(user));

      setToken(token);
      setUser(user);

      return { success: true };
    } catch (error: any) {
      console.error('Login error:', error);
      return {
        success: false,
        error: error.response?.data?.message || 'Login failed',
        errorCode: error.response?.data?.code,
        email: error.response?.data?.email,
      };
    }
  };

  const logout = (): void => {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    localStorage.removeItem('managingChild');
    setToken(null);
    setUser(null);
    setManagingChild(null);
  };

  const updateUser = (updatedUser: User): void => {
    setUser(updatedUser);
    localStorage.setItem('user', JSON.stringify(updatedUser));
  };

  const startManagingChild = (childInfo: ManagingChild): void => {
    setManagingChild(childInfo);
    localStorage.setItem('managingChild', JSON.stringify(childInfo));
  };

  const stopManagingChild = (): void => {
    setManagingChild(null);
    localStorage.removeItem('managingChild');
  };

  const value: AuthContextType = {
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

export const useAuth = (): AuthContextType => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};
