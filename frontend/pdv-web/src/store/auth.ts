import { create } from 'zustand';
import { persist } from 'zustand/middleware';
import type { AuthTokenResponse } from '../types/auth';

export interface AuthState {
  accessToken: string | null;
  refreshToken: string | null;
  userId: number | null;
  email: string | null;
  tenantId: number | null;
  permissions: string[];
  expiresAtUtc: string | null;
  setSession: (data: AuthTokenResponse) => void;
  clearSession: () => void;
  logout: () => void;
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set) => ({
      accessToken: null,
      refreshToken: null,
      userId: null,
      email: null,
      tenantId: null,
      permissions: [],
      expiresAtUtc: null,
      setSession: (data) =>
        set({
          accessToken: data.accessToken,
          refreshToken: data.refreshToken,
          userId: data.userId,
          email: data.email,
          tenantId: data.tenantId ?? null,
          permissions: data.permissions,
          expiresAtUtc: data.expiresAtUtc,
        }),
      clearSession: () =>
        set({
          accessToken: null,
          refreshToken: null,
          userId: null,
          email: null,
          tenantId: null,
          permissions: [],
          expiresAtUtc: null,
        }),
      logout: () =>
        set({
          accessToken: null,
          refreshToken: null,
          userId: null,
          email: null,
          tenantId: null,
          permissions: [],
          expiresAtUtc: null,
        }),
    }),
    {
      name: 'pdv-auth',
      partialize: (s) => ({
        accessToken: s.accessToken,
        refreshToken: s.refreshToken,
        userId: s.userId,
        email: s.email,
        tenantId: s.tenantId,
        permissions: s.permissions,
        expiresAtUtc: s.expiresAtUtc,
      }),
    },
  ),
);
