import { api } from './api';
import type { RoleAdminDto } from '../types/roles';

export async function listRoles(): Promise<RoleAdminDto[]> {
  const { data } = await api.get<RoleAdminDto[]>('/api/roles');
  return data;
}

export async function getRole(id: string): Promise<RoleAdminDto> {
  const { data } = await api.get<RoleAdminDto>(`/api/roles/${id}`);
  return data;
}

export async function createRole(name: string): Promise<string> {
  const { data } = await api.post<{ id: string }>('/api/roles', { name });
  return data.id;
}

export async function updateRole(id: string, name: string): Promise<void> {
  await api.put(`/api/roles/${id}`, { name });
}

export async function deleteRole(id: string): Promise<void> {
  await api.delete(`/api/roles/${id}`);
}

export async function setRolePermissions(id: string, permissionNames: string[]): Promise<void> {
  await api.put(`/api/roles/${id}/permissions`, { permissionNames });
}
