import { useAuthStore } from '../store/auth';

/** Hook: checagem de permissão na UI reativa ao store. */
export function usePermission(): { can: (permission: string) => boolean } {
  const permissions = useAuthStore((s) => s.permissions);
  return {
    can: (permission: string) => permissions.includes(permission),
  };
}

/** Helper fora de componentes React (ex.: callbacks). */
export function can(permission: string): boolean {
  return useAuthStore.getState().permissions.includes(permission);
}
