import { api } from './api';

export async function getPermissionCatalog(): Promise<string[]> {
  const { data } = await api.get<string[]>('/api/permissions');
  return data;
}
