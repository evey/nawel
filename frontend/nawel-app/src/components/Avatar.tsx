import { Avatar as MuiAvatar, SxProps, Theme } from '@mui/material';
import { useState } from 'react';
import type { User } from '../types';
import styles from '../css/Avatar.module.less';

const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:5284/api';

interface AvatarProps {
  user: User | null | undefined;
  size?: number;
  sx?: SxProps<Theme>;
  className?: string;
}

const Avatar = ({ user, size = 40, sx = {}, className }: AvatarProps) => {
  const [imageError, setImageError] = useState(false);

  const getInitials = (): string => {
    if (!user) return '?';

    if (user.firstName && user.lastName) {
      return `${user.firstName[0]}${user.lastName[0]}`.toUpperCase();
    }
    if (user.firstName) {
      return user.firstName[0].toUpperCase();
    }
    if (user.login) {
      return user.login[0].toUpperCase();
    }
    return '?';
  };

  const getAvatarUrl = (): string | null => {
    if (!user?.avatar || imageError) return null;

    // If avatar starts with /, it's a relative path from the API
    if (user.avatar.startsWith('/')) {
      return `${API_URL.replace('/api', '')}${user.avatar}`;
    }

    // If it's already a full URL
    if (user.avatar.startsWith('http')) {
      return user.avatar;
    }

    // Otherwise, construct the URL
    return `${API_URL.replace('/api', '')}/${user.avatar}`;
  };

  const avatarUrl = getAvatarUrl();

  return (
    <MuiAvatar
      src={avatarUrl || undefined}
      alt={user?.firstName || user?.login || 'User'}
      onError={() => setImageError(true)}
      className={`${styles.avatar}${className ? ` ${className}` : ''}`}
      sx={{
        width: size,
        height: size,
        bgcolor: 'primary.main',
        fontSize: size * 0.4,
        ...sx,
      }}
    >
      {!avatarUrl && getInitials()}
    </MuiAvatar>
  );
};

export default Avatar;
