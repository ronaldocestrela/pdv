import { api } from './api';
import type { UserAdminDto } from '../types/users';

export async function listUsers(): Promise<UserAdminDto[]> {
  const { data } = await api.get<UserAdminDto[]>('/api/users');
  return data;
}

export async function createUser(email: string, password: string, isActive = true): Promise<number> {
  const { data } = await api.post<{ id: number }>('/api/users', { email, password, isActive });
  return data.id;
}

export async function setUserRoles(userId: number, roleIds: number[]): Promise<void> {
  await api.put(`/api/users/${userId}/roles`, { roleIds });
}
