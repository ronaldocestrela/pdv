import axios from 'axios';
import type { AuthTokenResponse } from '../types/auth';
import { useAuthStore } from '../store/auth';

const baseURL = import.meta.env.VITE_API_URL ?? 'http://localhost:5190';

/** Login; persiste sessão no store (zustand persist). */
export async function login(email: string, password: string): Promise<AuthTokenResponse> {
  const { data } = await axios.post<AuthTokenResponse>(`${baseURL}/api/auth/login`, {
    email,
    password,
  });
  useAuthStore.getState().setSession(data);
  return data;
}

/** Refresh manual (o client `api` já renova em 401 automaticamente). */
export async function refresh(refreshToken: string): Promise<AuthTokenResponse> {
  const { data } = await axios.post<AuthTokenResponse>(`${baseURL}/api/auth/refresh`, {
    refreshToken,
  });
  useAuthStore.getState().setSession(data);
  return data;
}
