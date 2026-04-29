import { create } from 'zustand';

/** Estado global base — expandir nas próximas fases (ex.: auth). */
interface AppState {
  ready: boolean;
}

export const useAppStore = create<AppState>(() => ({
  ready: true,
}));
