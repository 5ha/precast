import { ApiClient } from './generated/api-client';

// Create singleton API client instance with base URL from environment
const baseUrl = import.meta.env.VITE_API_URL || '';
export const apiClient = new ApiClient(baseUrl);
