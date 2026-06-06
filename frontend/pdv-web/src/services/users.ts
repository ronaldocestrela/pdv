import { api } from './api';
import type { UserAdminDto } from '../types/users';

export async function listUsers(): Promise<UserAdminDto[]> {
  const { data } = await api.get<UserAdminDto[]>('/api/users');
  return data;
}

export async function createUser(email: string, password: string, isActive = true): Promise<string> {
  const { data } = await api.post<{ id: string }>('/api/users', { email, password, isActive });
  return data.id;
}

export async function setUserRoles(userId: string, roleIds: string[]): Promise<void> {
  await api.put(`/api/users/${userId}/roles`, { roleIds });
}
