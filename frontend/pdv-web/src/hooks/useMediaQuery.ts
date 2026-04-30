import { useSyncExternalStore } from 'react';

/**
 * Subscribes to `window.matchMedia`. SSR / tests: returns `false`.
 */
export function useMediaQuery(query: string): boolean {
  return useSyncExternalStore(
    (onChange) => {
      if (typeof window === 'undefined' || !window.matchMedia) {
        return () => {};
      }
      const mq = window.matchMedia(query);
      const handler = () => onChange();
      mq.addEventListener('change', handler);
      return () => mq.removeEventListener('change', handler);
    },
    () => {
      if (typeof window === 'undefined' || !window.matchMedia) return false;
      return window.matchMedia(query).matches;
    },
    () => false,
  );
}
