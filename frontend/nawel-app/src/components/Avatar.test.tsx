import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import Avatar from './Avatar';
import type { User } from '../types';

// Mock the LESS module
vi.mock('../css/Avatar.module.less', () => ({
  default: { avatar: 'avatar-mock' },
}));

describe('Avatar Component', () => {
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

  it('should render with user initials when no avatar URL', () => {
    const userNoAvatar = { ...mockUser, avatar: '' };
    render(<Avatar user={userNoAvatar} />);

    // Should show initials JD for John Doe
    expect(screen.getByText('JD')).toBeInTheDocument();
  });

  it('should render with first name initial only when no last name', () => {
    const userFirstNameOnly = { ...mockUser, lastName: '', avatar: '' };
    render(<Avatar user={userFirstNameOnly} />);

    expect(screen.getByText('J')).toBeInTheDocument();
  });

  it('should render with login initial when no first name', () => {
    const userLoginOnly = { ...mockUser, firstName: '', lastName: '', avatar: '' };
    render(<Avatar user={userLoginOnly} />);

    expect(screen.getByText('T')).toBeInTheDocument(); // 'T' from 'testuser'
  });

  it('should render with ? when user is null', () => {
    render(<Avatar user={null} />);

    expect(screen.getByText('?')).toBeInTheDocument();
  });

  it('should render with ? when user is undefined', () => {
    render(<Avatar user={undefined} />);

    expect(screen.getByText('?')).toBeInTheDocument();
  });

  it('should render with custom size', () => {
    const userNoAvatar = { ...mockUser, avatar: '' };
    const { container } = render(<Avatar user={userNoAvatar} size={60} />);

    const avatar = container.querySelector('[class*="MuiAvatar"]');
    expect(avatar).toBeInTheDocument();
  });

  it('should apply custom className', () => {
    const userNoAvatar = { ...mockUser, avatar: '' };
    const { container } = render(<Avatar user={userNoAvatar} className="custom-class" />);

    const avatar = container.querySelector('.custom-class');
    expect(avatar).toBeInTheDocument();
  });

  it('should use alt text from user firstName', () => {
    render(<Avatar user={mockUser} />);

    const avatar = screen.getByAltText('John');
    expect(avatar).toBeInTheDocument();
  });

  it('should use alt text from login when no firstName', () => {
    const userNoFirstName = { ...mockUser, firstName: '' };
    render(<Avatar user={userNoFirstName} />);

    const avatar = screen.getByAltText('testuser');
    expect(avatar).toBeInTheDocument();
  });

  it('should render MuiAvatar component when no user data', () => {
    const { container } = render(<Avatar user={null} />);

    // Avatar component should render
    const avatar = container.querySelector('[class*="MuiAvatar"]');
    expect(avatar).toBeInTheDocument();
    expect(avatar).toHaveTextContent('?');
  });

  it('should render initials in uppercase', () => {
    const userLowerCase = {
      ...mockUser,
      firstName: 'john',
      lastName: 'doe',
      avatar: '',
    };
    render(<Avatar user={userLowerCase} />);

    expect(screen.getByText('JD')).toBeInTheDocument();
  });

  it('should handle user with only login (no names)', () => {
    const userOnlyLogin: User = {
      id: 2,
      login: 'username',
      firstName: '',
      lastName: '',
      avatar: '',
      isChildren: false,
      isAdmin: false,
      familyId: 1,
      familyName: 'Test Family',
      email: 'user@example.com',
      notifyListEdit: false,
      notifyGiftTaken: false,
      displayPopup: true,
      createdAt: '2024-01-01T00:00:00Z',
      updatedAt: '2024-01-01T00:00:00Z',
    };
    render(<Avatar user={userOnlyLogin} />);

    expect(screen.getByText('U')).toBeInTheDocument();
  });
});
