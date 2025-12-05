import { Avatar as MuiAvatar } from '@mui/material';
import { useState } from 'react';

const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:5284/api';

const Avatar = ({ user, size = 40, sx = {} }) => {
  const [imageError, setImageError] = useState(false);

  const getInitials = () => {
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

  const getAvatarUrl = () => {
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
      sx={{
        width: size,
        height: size,
        bgcolor: 'primary.main',
        fontSize: size * 0.4,
        fontWeight: 'bold',
        ...sx,
      }}
    >
      {!avatarUrl && getInitials()}
    </MuiAvatar>
  );
};

export default Avatar;
