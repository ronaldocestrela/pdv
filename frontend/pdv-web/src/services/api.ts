import axios, { type AxiosError, type InternalAxiosRequestConfig } from 'axios';
import type { AuthTokenResponse } from '../types/auth';
import { useAuthStore } from '../store/auth';

const baseURL = import.meta.env.VITE_API_URL ?? 'http://localhost:5190';

/**
 * TODO: Describe api.
 */
export const api = axios.create({
  baseURL,
  headers: {
    'Content-Type': 'application/json',
  },
});

api.interceptors.request.use((config) => {
  if (config.url?.includes('/api/auth/login')) {
    return config;
  }
  const token = useAuthStore.getState().accessToken;
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

api.interceptors.response.use(
  (res) => res,
  async (error: AxiosError) => {
    const cfg = error.config as InternalAxiosRequestConfig & { _retry?: boolean };
    if (!cfg || cfg._retry) {
      return Promise.reject(error);
    }
    if (error.response?.status !== 401) {
      return Promise.reject(error);
    }
    if (cfg.url?.includes('/api/auth/')) {
      return Promise.reject(error);
    }

    const newToken = await refreshAccessToken();
    if (!newToken) {
      useAuthStore.getState().logout();
      return Promise.reject(error);
    }

    cfg._retry = true;
    cfg.headers = cfg.headers ?? {};
    cfg.headers.Authorization = `Bearer ${newToken}`;
    return api(cfg);
  },
);

async function refreshAccessToken(): Promise<string | null> {
  const refresh = useAuthStore.getState().refreshToken;
  if (!refresh) {
    return null;
  }

  try {
    const { data } = await axios.post<AuthTokenResponse>(`${baseURL}/api/auth/refresh`, {
      refreshToken: refresh,
    });
    useAuthStore.getState().setSession(data);
    return data.accessToken;
  } catch {
    return null;
  }
}
