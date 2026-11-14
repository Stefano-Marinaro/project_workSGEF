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

// All requests in the session share one refresh-token rotation.
let refreshInFlight = null;

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
      originalRequest &&
      !originalRequest._retriedAfterRefresh &&
      !originalRequest.url?.startsWith('/auth/');

    if (shouldTryRefresh) {
      originalRequest._retriedAfterRefresh = true;

      try {
        const currentToken = await getItem('accessToken');
        // A late 401 may belong to a request sent before an already completed refresh.
        if (currentToken && originalRequest.headers?.Authorization !== `Bearer ${currentToken}`) {
          originalRequest.headers = { ...originalRequest.headers, Authorization: `Bearer ${currentToken}` };
          return api(originalRequest);
        }
        if (!refreshInFlight) {
          refreshInFlight = (async () => {
            const refreshToken = await getItem('refreshToken');
            if (!refreshToken) throw { sessionExpired: true };
            const response = await axios.post(`${API_BASE_URL}/auth/refresh`, { refreshToken }, { timeout: 10000 });
            await setItem('accessToken', response.data.accessToken);
            await setItem('refreshToken', response.data.refreshToken);
            return response.data.accessToken;
          })().finally(() => { refreshInFlight = null; });
        }
        const newToken = await refreshInFlight;
        originalRequest.headers = { ...originalRequest.headers, Authorization: `Bearer ${newToken}` };
        return api(originalRequest);
      } catch (refreshError) {
        // A temporary connection/server failure must not erase a valid refresh token.
        if (refreshError.sessionExpired || [401, 403].includes(refreshError.response?.status)) {
          await deleteItem('accessToken');
          await deleteItem('refreshToken');
          router.replace('/login');
        }
        return Promise.reject(error);
      }
    }

    return Promise.reject(error);
  }
);

export default api;
