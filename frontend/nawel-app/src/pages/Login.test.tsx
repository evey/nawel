import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { BrowserRouter } from 'react-router-dom';
import Login from './Login';
import { useAuth } from '../contexts/AuthContext';
import * as api from '../services/api';

// Mock the navigate function
const mockNavigate = vi.fn();
const mockLogin = vi.fn();

vi.mock('react-router-dom', async () => {
  const actual = await vi.importActual('react-router-dom');
  return {
    ...actual,
    useNavigate: () => mockNavigate,
  };
});

// Mock the AuthContext
vi.mock('../contexts/AuthContext', () => ({
  useAuth: vi.fn(),
}));

// Mock the api module for requestMigrationReset
vi.mock('../services/api', () => ({
  authAPI: {
    requestMigrationReset: vi.fn(),
  },
}));

const renderLogin = () => {
  return render(
    <BrowserRouter>
      <Login />
    </BrowserRouter>
  );
};

describe('Login - MD5 Migration Flow', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    localStorage.clear();

    // Setup default useAuth mock
    vi.mocked(useAuth).mockReturnValue({
      user: null,
      token: null,
      loading: false,
      managingChild: null,
      login: mockLogin,
      logout: vi.fn(),
      updateUser: vi.fn(),
      startManagingChild: vi.fn(),
      stopManagingChild: vi.fn(),
      isAuthenticated: false,
    });
  });

  it('should display migration warning when LEGACY_PASSWORD error is returned', async () => {
    const user = userEvent.setup();

    // Mock login to return LEGACY_PASSWORD error
    mockLogin.mockResolvedValueOnce({
      success: false,
      errorCode: 'LEGACY_PASSWORD',
      email: 'user@example.com',
      error: 'Votre mot de passe doit être réinitialisé pour des raisons de sécurité',
    });

    renderLogin();

    // Fill in login form
    const loginInput = screen.getByLabelText(/identifiant/i);
    const passwordInput = screen.getByLabelText(/mot de passe/i);

    await user.type(loginInput, 'testuser');
    await user.type(passwordInput, 'password123');

    // Submit form
    const submitButton = screen.getByRole('button', { name: /se connecter/i });
    await user.click(submitButton);

    // Check that migration warning is displayed
    await waitFor(() => {
      expect(screen.getByText(/mise à jour de sécurité requise/i)).toBeInTheDocument();
    });

    // Check that explanation is shown
    expect(
      screen.getByText(/pour améliorer la sécurité de votre compte/i)
    ).toBeInTheDocument();

    // Check that reset button is present
    expect(
      screen.getByRole('button', { name: /recevoir un email de réinitialisation/i })
    ).toBeInTheDocument();
  });

  it('should display user email in migration warning', async () => {
    const user = userEvent.setup();
    const userEmail = 'user@example.com';

    mockLogin.mockResolvedValueOnce({
      success: false,
      errorCode: 'LEGACY_PASSWORD',
      email: userEmail,
    });

    renderLogin();

    const loginInput = screen.getByLabelText(/identifiant/i);
    const passwordInput = screen.getByLabelText(/mot de passe/i);

    await user.type(loginInput, 'testuser');
    await user.type(passwordInput, 'password123');

    const submitButton = screen.getByRole('button', { name: /se connecter/i });
    await user.click(submitButton);

    await waitFor(() => {
      expect(screen.getByText(new RegExp(userEmail, 'i'))).toBeInTheDocument();
    });
  });

  it('should call requestMigrationReset when reset button is clicked', async () => {
    const user = userEvent.setup();

    // Mock login to return LEGACY_PASSWORD error
    mockLogin.mockResolvedValueOnce({
      success: false,
      errorCode: 'LEGACY_PASSWORD',
      email: 'user@example.com',
    });

    // Mock requestMigrationReset to succeed
    vi.mocked(api.authAPI.requestMigrationReset).mockResolvedValueOnce({
      data: { message: 'Email envoyé' },
      status: 200,
      statusText: 'OK',
      headers: {},
      config: {} as any,
    });

    renderLogin();

    // Login with legacy password
    const loginInput = screen.getByLabelText(/identifiant/i);
    const passwordInput = screen.getByLabelText(/mot de passe/i);

    await user.type(loginInput, 'testuser');
    await user.type(passwordInput, 'password123');

    const submitButton = screen.getByRole('button', { name: /se connecter/i });
    await user.click(submitButton);

    // Wait for migration warning
    await waitFor(() => {
      expect(screen.getByText(/mise à jour de sécurité requise/i)).toBeInTheDocument();
    });

    // Click reset button
    const resetButton = screen.getByRole('button', {
      name: /recevoir un email de réinitialisation/i,
    });
    await user.click(resetButton);

    // Verify API was called with correct login
    await waitFor(() => {
      expect(api.authAPI.requestMigrationReset).toHaveBeenCalledWith({
        login: 'testuser',
      });
    });
  });

  it('should display success message after requesting migration reset', async () => {
    const user = userEvent.setup();
    const userEmail = 'user@example.com';

    mockLogin.mockResolvedValueOnce({
      success: false,
      errorCode: 'LEGACY_PASSWORD',
      email: userEmail,
    });

    vi.mocked(api.authAPI.requestMigrationReset).mockResolvedValueOnce({
      data: { message: 'Email envoyé' },
      status: 200,
      statusText: 'OK',
      headers: {},
      config: {} as any,
    });

    renderLogin();

    // Login with legacy password
    const loginInput = screen.getByLabelText(/identifiant/i);
    const passwordInput = screen.getByLabelText(/mot de passe/i);

    await user.type(loginInput, 'testuser');
    await user.type(passwordInput, 'password123');

    const submitButton = screen.getByRole('button', { name: /se connecter/i });
    await user.click(submitButton);

    // Wait for migration warning and click reset button
    await waitFor(() => {
      expect(screen.getByText(/mise à jour de sécurité requise/i)).toBeInTheDocument();
    });

    const resetButton = screen.getByRole('button', {
      name: /recevoir un email de réinitialisation/i,
    });
    await user.click(resetButton);

    // Check success message
    await waitFor(() => {
      expect(screen.getByText(/email envoyé !/i)).toBeInTheDocument();
    });

    // Check that email is displayed in success message
    expect(screen.getByText(new RegExp(userEmail, 'i'))).toBeInTheDocument();

    // Check that migration warning is hidden
    expect(
      screen.queryByRole('button', { name: /recevoir un email de réinitialisation/i })
    ).not.toBeInTheDocument();
  });

  it('should show loading state while sending migration email', async () => {
    const user = userEvent.setup();

    mockLogin.mockResolvedValueOnce({
      success: false,
      errorCode: 'LEGACY_PASSWORD',
      email: 'user@example.com',
    });

    // Mock requestMigrationReset with a delay
    vi.mocked(api.authAPI.requestMigrationReset).mockImplementationOnce(
      () =>
        new Promise((resolve) =>
          setTimeout(
            () =>
              resolve({
                data: { message: 'Email envoyé' },
                status: 200,
                statusText: 'OK',
                headers: {},
                config: {} as any,
              }),
            100
          )
        )
    );

    renderLogin();

    // Login with legacy password
    const loginInput = screen.getByLabelText(/identifiant/i);
    const passwordInput = screen.getByLabelText(/mot de passe/i);

    await user.type(loginInput, 'testuser');
    await user.type(passwordInput, 'password123');

    const submitButton = screen.getByRole('button', { name: /se connecter/i });
    await user.click(submitButton);

    await waitFor(() => {
      expect(screen.getByText(/mise à jour de sécurité requise/i)).toBeInTheDocument();
    });

    const resetButton = screen.getByRole('button', {
      name: /recevoir un email de réinitialisation/i,
    });

    await user.click(resetButton);

    // Button should be disabled during loading
    expect(resetButton).toBeDisabled();
  });

  it('should display error message if migration reset fails', async () => {
    const user = userEvent.setup();

    mockLogin.mockResolvedValueOnce({
      success: false,
      errorCode: 'LEGACY_PASSWORD',
      email: 'user@example.com',
    });

    // Mock requestMigrationReset to fail
    vi.mocked(api.authAPI.requestMigrationReset).mockRejectedValueOnce({
      response: {
        data: {
          message: 'Erreur serveur',
        },
      },
    });

    renderLogin();

    // Login with legacy password
    const loginInput = screen.getByLabelText(/identifiant/i);
    const passwordInput = screen.getByLabelText(/mot de passe/i);

    await user.type(loginInput, 'testuser');
    await user.type(passwordInput, 'password123');

    const submitButton = screen.getByRole('button', { name: /se connecter/i });
    await user.click(submitButton);

    await waitFor(() => {
      expect(screen.getByText(/mise à jour de sécurité requise/i)).toBeInTheDocument();
    });

    const resetButton = screen.getByRole('button', {
      name: /recevoir un email de réinitialisation/i,
    });
    await user.click(resetButton);

    // Check error message
    await waitFor(() => {
      expect(screen.getByText(/erreur/i)).toBeInTheDocument();
    });
  });

  it('should reset migration states when user types in form after legacy password error', async () => {
    const user = userEvent.setup();

    mockLogin.mockResolvedValueOnce({
      success: false,
      errorCode: 'LEGACY_PASSWORD',
      email: 'user@example.com',
    });

    renderLogin();

    // Login with legacy password
    const loginInput = screen.getByLabelText(/identifiant/i);
    const passwordInput = screen.getByLabelText(/mot de passe/i);

    await user.type(loginInput, 'testuser');
    await user.type(passwordInput, 'password123');

    const submitButton = screen.getByRole('button', { name: /se connecter/i });
    await user.click(submitButton);

    // Wait for migration warning
    await waitFor(() => {
      expect(screen.getByText(/mise à jour de sécurité requise/i)).toBeInTheDocument();
    });

    // Type in form again
    await user.type(loginInput, 'x');

    // Migration warning should disappear
    await waitFor(() => {
      expect(
        screen.queryByText(/mise à jour de sécurité requise/i)
      ).not.toBeInTheDocument();
    });
  });
});
