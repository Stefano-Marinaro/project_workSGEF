import axios from 'axios';
import { router } from 'expo-router';
import { API_BASE_URL } from './api.js';
import { getItem, setItem, deleteItem } from './storage.js';

const api = axios.create({
  baseURL: API_BASE_URL,
  timeout: 10000,
  headers: {
    'Content-Type': 'application/json',
  },
});

api.interceptors.request.use(async (config) => {
  const token = await getItem('accessToken');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

api.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config;

    const shouldTryRefresh =
      error.response?.status === 401 &&
      !originalRequest._retriedAfterRefresh &&
      originalRequest.url !== '/auth/refresh';

    if (shouldTryRefresh) {
      originalRequest._retriedAfterRefresh = true;

      try {
        const refreshToken = await getItem('refreshToken');
        if (!refreshToken) throw error;

        const refreshResponse = await axios.post(`${API_BASE_URL}/auth/refresh`, { refreshToken });

        await setItem('accessToken', refreshResponse.data.accessToken);
        await setItem('refreshToken', refreshResponse.data.refreshToken);

        originalRequest.headers.Authorization = `Bearer ${refreshResponse.data.accessToken}`;
        return api(originalRequest);
      } catch (refreshError) {
        await deleteItem('accessToken');
        await deleteItem('refreshToken');
        router.replace('/login');
        return Promise.reject(error);
      }
    }

    return Promise.reject(error);
  }
);

export default api;
