import { Box } from '@mui/material';

const ChristmasLayout = ({ children }) => {
  return (
    <Box
      sx={{
        minHeight: '100vh',
        background: `
          linear-gradient(rgba(248, 246, 243, 0.3), rgba(248, 246, 243, 0.3)),
          url('https://images.unsplash.com/photo-1543589077-47d81606c1bf?w=1920&q=80')
        `,
        backgroundSize: 'cover',
        backgroundPosition: 'center',
        backgroundAttachment: 'fixed',
        display: 'flex',
        flexDirection: 'column',
      }}
    >
      <Box
        sx={{
          flexGrow: 1,
          display: 'flex',
          justifyContent: 'center',
        }}
      >
        <Box
          sx={{
            width: '100%',
            maxWidth: '1200px',
            backgroundColor: 'rgba(248, 246, 243, 0.96)',
            boxShadow: '0 0 40px rgba(0, 0, 0, 0.2)',
            minHeight: '100vh',
            display: 'flex',
            flexDirection: 'column',
          }}
        >
          {children}
        </Box>
      </Box>
    </Box>
  );
};

export default ChristmasLayout;
