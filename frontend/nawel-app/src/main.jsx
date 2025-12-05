import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import { ThemeProvider, createTheme } from '@mui/material/styles';
import CssBaseline from '@mui/material/CssBaseline';
import App from './App.jsx';

const theme = createTheme({
  palette: {
    mode: 'light',
    primary: {
      main: '#2d5f3f', // Vert sapin plus doux
      light: '#4a7c59',
      dark: '#1a3d2a',
      contrastText: '#fff',
    },
    secondary: {
      main: '#b8860b', // Or/doré chaleureux
      light: '#daa520',
      dark: '#8b6508',
      contrastText: '#fff',
    },
    background: {
      default: '#f8f6f3', // Fond crème/beige très léger
      paper: '#ffffff',
    },
    success: {
      main: '#2d5f3f', // Vert sapin
    },
    error: {
      main: '#a83b3b', // Rouge Noël adouci
    },
    warning: {
      main: '#daa520', // Or pour les warnings
    },
    info: {
      main: '#5b8c9f', // Bleu hivernal doux
    },
  },
  typography: {
    fontFamily: '"Roboto", "Helvetica", "Arial", sans-serif',
    h4: {
      fontWeight: 600,
      color: '#2d5f3f',
    },
    h5: {
      fontWeight: 600,
      color: '#2d5f3f',
    },
    h6: {
      fontWeight: 600,
    },
  },
  components: {
    MuiAppBar: {
      styleOverrides: {
        root: {
          background: 'linear-gradient(135deg, #2d5f3f 0%, #1a3d2a 100%)', // Dégradé vert sapin
        },
      },
    },
    MuiButton: {
      styleOverrides: {
        root: {
          borderRadius: 8,
          textTransform: 'none',
          fontWeight: 500,
        },
        contained: {
          boxShadow: '0 2px 8px rgba(45, 95, 63, 0.2)',
          '&:hover': {
            boxShadow: '0 4px 12px rgba(45, 95, 63, 0.3)',
          },
        },
        containedSecondary: {
          boxShadow: '0 2px 8px rgba(218, 165, 32, 0.25)',
          '&:hover': {
            boxShadow: '0 4px 12px rgba(218, 165, 32, 0.35)',
          },
        },
      },
    },
    MuiCard: {
      styleOverrides: {
        root: {
          borderRadius: 12,
          boxShadow: '0 2px 8px rgba(0, 0, 0, 0.08)',
          border: '1px solid rgba(45, 95, 63, 0.08)',
          '&:hover': {
            boxShadow: '0 4px 16px rgba(0, 0, 0, 0.12)',
            borderColor: 'rgba(45, 95, 63, 0.15)',
          },
        },
      },
    },
    MuiChip: {
      styleOverrides: {
        root: {
          fontWeight: 500,
        },
        colorSuccess: {
          backgroundColor: '#2d5f3f',
          color: '#fff',
        },
        colorWarning: {
          backgroundColor: '#daa520',
          color: '#fff',
        },
      },
    },
    MuiFab: {
      styleOverrides: {
        primary: {
          boxShadow: '0 3px 12px rgba(45, 95, 63, 0.3)',
        },
        secondary: {
          boxShadow: '0 3px 12px rgba(218, 165, 32, 0.3)',
        },
      },
    },
  },
});

createRoot(document.getElementById('root')).render(
  <StrictMode>
    <ThemeProvider theme={theme}>
      <CssBaseline />
      <App />
    </ThemeProvider>
  </StrictMode>
);
