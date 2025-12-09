import { AxiosResponse } from 'axios';

export interface ApiResponse<T = any> {
  data: T;
  message?: string;
}

export interface ApiError {
  message: string;
  correlationId?: string;
  timestamp?: string;
}

export interface PaginatedResponse<T> {
  data: T[];
  total: number;
  page: number;
  pageSize: number;
}

// Re-export Axios types for convenience
export type { AxiosResponse, AxiosError } from 'axios';
