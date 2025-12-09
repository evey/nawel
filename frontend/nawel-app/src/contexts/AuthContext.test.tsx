import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import { AuthProvider, useAuth } from './AuthContext';
import type { User } from '../types';

// Mock the API module
vi.mock('../services/api', () => ({
  authAPI: {
    login: vi.fn(),
  },
}));

// Test component to access context
const TestComponent = () => {
  const { user, isAuthenticated, logout } = useAuth();
  return (
    <div>
      {isAuthenticated ? (
        <>
          <div data-testid="user-name">{user?.firstName || user?.login}</div>
          <button onClick={logout}>Logout</button>
        </>
      ) : (
        <div data-testid="not-authenticated">Not authenticated</div>
      )}
    </div>
  );
};

describe('AuthContext', () => {
  const mockUser: User = {
    id: 1,
    login: 'testuser',
    firstName: 'John',
    lastName: 'Doe',
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

  beforeEach(() => {
    // Clear localStorage before each test
    localStorage.clear();
    vi.clearAllMocks();
  });

  afterEach(() => {
    localStorage.clear();
  });

  it('should initialize with no user when localStorage is empty', async () => {
    render(
      <AuthProvider>
        <TestComponent />
      </AuthProvider>
    );

    await waitFor(() => {
      expect(screen.getByTestId('not-authenticated')).toBeInTheDocument();
    });
  });

  it('should restore user from localStorage on mount', async () => {
    localStorage.setItem('token', 'test-token');
    localStorage.setItem('user', JSON.stringify(mockUser));

    render(
      <AuthProvider>
        <TestComponent />
      </AuthProvider>
    );

    await waitFor(() => {
      expect(screen.getByTestId('user-name')).toHaveTextContent('John');
    });
  });

  it('should clear localStorage on logout', async () => {
    localStorage.setItem('token', 'test-token');
    localStorage.setItem('user', JSON.stringify(mockUser));

    render(
      <AuthProvider>
        <TestComponent />
      </AuthProvider>
    );

    await waitFor(() => {
      expect(screen.getByTestId('user-name')).toBeInTheDocument();
    });

    // Click logout button
    const logoutButton = screen.getByText('Logout');
    logoutButton.click();

    await waitFor(() => {
      expect(screen.getByTestId('not-authenticated')).toBeInTheDocument();
      expect(localStorage.getItem('token')).toBeNull();
      expect(localStorage.getItem('user')).toBeNull();
    });
  });

  it('should provide isAuthenticated as true when user is logged in', async () => {
    localStorage.setItem('token', 'test-token');
    localStorage.setItem('user', JSON.stringify(mockUser));

    const TestAuthStatus = () => {
      const { isAuthenticated } = useAuth();
      return <div data-testid="auth-status">{isAuthenticated ? 'true' : 'false'}</div>;
    };

    render(
      <AuthProvider>
        <TestAuthStatus />
      </AuthProvider>
    );

    await waitFor(() => {
      expect(screen.getByTestId('auth-status')).toHaveTextContent('true');
    });
  });

  it('should provide isAuthenticated as false when no user', async () => {
    const TestAuthStatus = () => {
      const { isAuthenticated } = useAuth();
      return <div data-testid="auth-status">{isAuthenticated ? 'true' : 'false'}</div>;
    };

    render(
      <AuthProvider>
        <TestAuthStatus />
      </AuthProvider>
    );

    await waitFor(() => {
      expect(screen.getByTestId('auth-status')).toHaveTextContent('false');
    });
  });

  it('should update user data with updateUser', async () => {
    localStorage.setItem('token', 'test-token');
    localStorage.setItem('user', JSON.stringify(mockUser));

    const TestUpdateUser = () => {
      const { user, updateUser } = useAuth();
      return (
        <div>
          <div data-testid="user-name">{user?.firstName}</div>
          <button
            onClick={() => updateUser({ ...mockUser, firstName: 'Jane' } as User)}
          >
            Update
          </button>
        </div>
      );
    };

    render(
      <AuthProvider>
        <TestUpdateUser />
      </AuthProvider>
    );

    await waitFor(() => {
      expect(screen.getByTestId('user-name')).toHaveTextContent('John');
    });

    // Click update button
    const updateButton = screen.getByText('Update');
    updateButton.click();

    await waitFor(() => {
      expect(screen.getByTestId('user-name')).toHaveTextContent('Jane');
      const storedUser = JSON.parse(localStorage.getItem('user') || '{}');
      expect(storedUser.firstName).toBe('Jane');
    });
  });

  it('should manage child state with startManagingChild and stopManagingChild', async () => {
    const TestManagingChild = () => {
      const { managingChild, startManagingChild, stopManagingChild } = useAuth();
      return (
        <div>
          <div data-testid="managing-child">
            {managingChild ? managingChild.userName : 'none'}
          </div>
          <button
            onClick={() => startManagingChild({ userId: 2, userName: 'Child User', avatarUrl: '' })}
          >
            Start Managing
          </button>
          <button onClick={stopManagingChild}>Stop Managing</button>
        </div>
      );
    };

    render(
      <AuthProvider>
        <TestManagingChild />
      </AuthProvider>
    );

    await waitFor(() => {
      expect(screen.getByTestId('managing-child')).toHaveTextContent('none');
    });

    // Start managing child
    const startButton = screen.getByText('Start Managing');
    startButton.click();

    await waitFor(() => {
      expect(screen.getByTestId('managing-child')).toHaveTextContent('Child User');
      expect(localStorage.getItem('managingChild')).toBeTruthy();
    });

    // Stop managing child
    const stopButton = screen.getByText('Stop Managing');
    stopButton.click();

    await waitFor(() => {
      expect(screen.getByTestId('managing-child')).toHaveTextContent('none');
      expect(localStorage.getItem('managingChild')).toBeNull();
    });
  });

  it('should restore managingChild from localStorage on mount', async () => {
    const childInfo = { userId: 2, userName: 'Child User', avatarUrl: '' };
    localStorage.setItem('managingChild', JSON.stringify(childInfo));

    const TestManagingChild = () => {
      const { managingChild } = useAuth();
      return (
        <div data-testid="managing-child">
          {managingChild ? managingChild.userName : 'none'}
        </div>
      );
    };

    render(
      <AuthProvider>
        <TestManagingChild />
      </AuthProvider>
    );

    await waitFor(() => {
      expect(screen.getByTestId('managing-child')).toHaveTextContent('Child User');
    });
  });
});
