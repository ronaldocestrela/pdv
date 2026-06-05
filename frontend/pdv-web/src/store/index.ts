import { create } from 'zustand';

/** Estado global base — expandir nas próximas fases (ex.: auth). */
interface AppState {
  ready: boolean;
}

/**
 * TODO: Describe useAppStore.
 */
export const useAppStore = create<AppState>(() => ({
  ready: true,
}));
