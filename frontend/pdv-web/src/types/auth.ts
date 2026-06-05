/** Resposta de POST /api/auth/login e /api/auth/refresh */
export interface AuthTokenResponse {
  accessToken: string;
  refreshToken: string;
  expiresAtUtc: string;
  permissions: string[];
  userId: number;
  email: string;
  tenantId?: number;
}
