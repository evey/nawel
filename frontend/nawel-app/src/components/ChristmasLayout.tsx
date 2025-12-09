import { ReactNode } from 'react';
import { Box } from '@mui/material';
import styles from '../css/ChristmasLayout.module.less';

interface ChristmasLayoutProps {
  children: ReactNode;
}

const ChristmasLayout = ({ children }: ChristmasLayoutProps): JSX.Element => {
  return (
    <Box className={styles.background}>
      <Box className={styles.centeredContainer}>
        <Box className={styles.contentWrapper}>
          {children}
        </Box>
      </Box>
    </Box>
  );
};

export default ChristmasLayout;
