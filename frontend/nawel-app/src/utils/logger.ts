/**
 * Logger utility for development debugging
 * Logs are only displayed in development mode
 */

const isDevelopment = import.meta.env.MODE === 'development';

type LogFunction = (...args: any[]) => void;

interface Logger {
  debug: LogFunction;
  info: LogFunction;
  warn: LogFunction;
  error: LogFunction;
}

export const logger: Logger = {
  debug: (...args: any[]): void => {
    if (isDevelopment) {
      console.log('[DEBUG]', ...args);
    }
  },

  info: (...args: any[]): void => {
    if (isDevelopment) {
      console.info('[INFO]', ...args);
    }
  },

  warn: (...args: any[]): void => {
    console.warn('[WARN]', ...args);
  },

  error: (...args: any[]): void => {
    console.error('[ERROR]', ...args);
  }
};
