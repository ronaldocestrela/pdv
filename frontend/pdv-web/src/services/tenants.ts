/**
 * tenantService.ts
 * Serviço de acesso à API de tenants.
 * Inclui endpoint público de registro e endpoints protegidos para o Super Admin global.
 */
import axios from 'axios';
import { api } from './api';
import type { TenantAdminDto, RegisterTenantPayload } from '../types/tenants';

const baseURL = import.meta.env.VITE_API_URL ?? 'http://localhost:5190';

/**
 * Registra um novo tenant via endpoint público (sem autenticação).
 * Cria a empresa, a role Super Admin local e o primeiro usuário administrador.
 */
export async function registerTenant(payload: RegisterTenantPayload): Promise<string> {
  const { data } = await axios.post<{ id: string }>(
    `${baseURL}/api/tenants/register`,
    payload,
    { headers: { 'Content-Type': 'application/json' } },
  );
  return data.id;
}

/**
 * Lista todos os tenants cadastrados no sistema.
 * Requer token JWT com permissão `tenant.manage`.
 */
export async function listTenants(): Promise<TenantAdminDto[]> {
  const { data } = await api.get<TenantAdminDto[]>('/api/tenants');
  return data;
}

/**
 * Ativa ou desativa um tenant pelo ID.
 * Requer token JWT com permissão `tenant.manage`.
 */
export async function setTenantActive(tenantId: string, isActive: boolean): Promise<void> {
  await api.put(`/api/tenants/${tenantId}/activate`, { isActive });
}
