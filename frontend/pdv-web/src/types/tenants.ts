/** Dados de um tenant retornados pela listagem administrativa. */
export interface TenantAdminDto {
  id: string;
  name: string;
  isActive: boolean;
  createdAtUtc: string;
}

/** Payload para registro ou criação de um novo tenant. */
export interface RegisterTenantPayload {
  name: string;
  adminEmail: string;
  adminPassword: string;
}
