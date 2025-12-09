import { useState, useEffect } from 'react';
import {
  Container,
  Typography,
  Paper,
  Box,
  Tabs,
  Tab,
  CircularProgress,
  Alert,
} from '@mui/material';
import MenuBookIcon from '@mui/icons-material/MenuBook';
import AutoStoriesIcon from '@mui/icons-material/AutoStories';
import ReactMarkdown from 'react-markdown';
import remarkGfm from 'remark-gfm';
import NavigationBar from '../components/NavigationBar';

interface TabPanelProps {
  children?: React.ReactNode;
  index: number;
  value: number;
}

function TabPanel(props: TabPanelProps) {
  const { children, value, index, ...other } = props;

  return (
    <div
      role="tabpanel"
      hidden={value !== index}
      id={`help-tabpanel-${index}`}
      aria-labelledby={`help-tab-${index}`}
      {...other}
    >
      {value === index && <Box sx={{ p: 3 }}>{children}</Box>}
    </div>
  );
}

function a11yProps(index: number) {
  return {
    id: `help-tab-${index}`,
    'aria-controls': `help-tabpanel-${index}`,
  };
}

export default function Help() {
  const [tabValue, setTabValue] = useState(0);
  const [gettingStartedContent, setGettingStartedContent] = useState('');
  const [featuresContent, setFeaturesContent] = useState('');
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    const loadGuides = async () => {
      try {
        setLoading(true);
        setError('');

        // Load both guides in parallel
        const [gettingStartedResponse, featuresResponse] = await Promise.all([
          fetch('/guides/GETTING-STARTED.md'),
          fetch('/guides/FEATURES.md'),
        ]);

        if (!gettingStartedResponse.ok || !featuresResponse.ok) {
          throw new Error('Impossible de charger les guides');
        }

        const gettingStartedText = await gettingStartedResponse.text();
        const featuresText = await featuresResponse.text();

        setGettingStartedContent(gettingStartedText);
        setFeaturesContent(featuresText);
      } catch (err) {
        console.error('Error loading guides:', err);
        setError('Erreur lors du chargement des guides. Veuillez réessayer.');
      } finally {
        setLoading(false);
      }
    };

    loadGuides();
  }, []);

  const handleTabChange = (_event: React.SyntheticEvent, newValue: number) => {
    setTabValue(newValue);
  };

  return (
    <>
      <NavigationBar />
      <Container maxWidth="lg" sx={{ py: 4 }}>
        <Paper elevation={3} sx={{ mb: 4 }}>
          <Box sx={{ borderBottom: 1, borderColor: 'divider' }}>
            <Tabs
              value={tabValue}
              onChange={handleTabChange}
              aria-label="help guides tabs"
              variant="fullWidth"
            >
              <Tab
                icon={<AutoStoriesIcon />}
                label="Guide de Démarrage"
                {...a11yProps(0)}
              />
              <Tab
                icon={<MenuBookIcon />}
                label="Guide des Fonctionnalités"
                {...a11yProps(1)}
              />
            </Tabs>
          </Box>

          {loading && (
            <Box sx={{ display: 'flex', justifyContent: 'center', p: 8 }}>
              <CircularProgress />
            </Box>
          )}

          {error && (
            <Box sx={{ p: 3 }}>
              <Alert severity="error">{error}</Alert>
            </Box>
          )}

          {!loading && !error && (
            <>
              <TabPanel value={tabValue} index={0}>
                <Box
                  sx={{
                    '& h1': {
                      fontSize: '2rem',
                      fontWeight: 600,
                      mb: 2,
                      color: 'primary.main',
                    },
                    '& h2': {
                      fontSize: '1.5rem',
                      fontWeight: 600,
                      mt: 4,
                      mb: 2,
                      color: 'primary.dark',
                    },
                    '& h3': {
                      fontSize: '1.25rem',
                      fontWeight: 600,
                      mt: 3,
                      mb: 1.5,
                    },
                    '& p': {
                      mb: 2,
                      lineHeight: 1.7,
                    },
                    '& ul, & ol': {
                      mb: 2,
                      pl: 3,
                    },
                    '& li': {
                      mb: 1,
                    },
                    '& code': {
                      backgroundColor: '#f5f5f5',
                      padding: '2px 6px',
                      borderRadius: '4px',
                      fontFamily: 'monospace',
                    },
                    '& pre': {
                      backgroundColor: '#f5f5f5',
                      padding: 2,
                      borderRadius: 1,
                      overflow: 'auto',
                      mb: 2,
                    },
                    '& blockquote': {
                      borderLeft: '4px solid #2d5f3f',
                      pl: 2,
                      ml: 0,
                      fontStyle: 'italic',
                      color: 'text.secondary',
                    },
                    '& table': {
                      borderCollapse: 'collapse',
                      width: '100%',
                      mb: 2,
                    },
                    '& th, & td': {
                      border: '1px solid #ddd',
                      padding: '8px 12px',
                      textAlign: 'left',
                    },
                    '& th': {
                      backgroundColor: '#f5f5f5',
                      fontWeight: 600,
                    },
                    '& hr': {
                      my: 3,
                      border: 0,
                      borderTop: '1px solid #ddd',
                    },
                  }}
                >
                  <ReactMarkdown remarkPlugins={[remarkGfm]}>
                    {gettingStartedContent}
                  </ReactMarkdown>
                </Box>
              </TabPanel>

              <TabPanel value={tabValue} index={1}>
                <Box
                  sx={{
                    '& h1': {
                      fontSize: '2rem',
                      fontWeight: 600,
                      mb: 2,
                      color: 'primary.main',
                    },
                    '& h2': {
                      fontSize: '1.5rem',
                      fontWeight: 600,
                      mt: 4,
                      mb: 2,
                      color: 'primary.dark',
                    },
                    '& h3': {
                      fontSize: '1.25rem',
                      fontWeight: 600,
                      mt: 3,
                      mb: 1.5,
                    },
                    '& p': {
                      mb: 2,
                      lineHeight: 1.7,
                    },
                    '& ul, & ol': {
                      mb: 2,
                      pl: 3,
                    },
                    '& li': {
                      mb: 1,
                    },
                    '& code': {
                      backgroundColor: '#f5f5f5',
                      padding: '2px 6px',
                      borderRadius: '4px',
                      fontFamily: 'monospace',
                    },
                    '& pre': {
                      backgroundColor: '#f5f5f5',
                      padding: 2,
                      borderRadius: 1,
                      overflow: 'auto',
                      mb: 2,
                    },
                    '& blockquote': {
                      borderLeft: '4px solid #2d5f3f',
                      pl: 2,
                      ml: 0,
                      fontStyle: 'italic',
                      color: 'text.secondary',
                    },
                    '& table': {
                      borderCollapse: 'collapse',
                      width: '100%',
                      mb: 2,
                    },
                    '& th, & td': {
                      border: '1px solid #ddd',
                      padding: '8px 12px',
                      textAlign: 'left',
                    },
                    '& th': {
                      backgroundColor: '#f5f5f5',
                      fontWeight: 600,
                    },
                    '& hr': {
                      my: 3,
                      border: 0,
                      borderTop: '1px solid #ddd',
                    },
                  }}
                >
                  <ReactMarkdown remarkPlugins={[remarkGfm]}>
                    {featuresContent}
                  </ReactMarkdown>
                </Box>
              </TabPanel>
            </>
          )}
        </Paper>
      </Container>
    </>
  );
}
