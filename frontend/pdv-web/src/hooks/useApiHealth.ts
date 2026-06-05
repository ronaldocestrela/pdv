import { useEffect, useState } from 'react';
import { api } from '../services/api';

export type ApiHealthStatus = 'loading' | 'ok' | 'error';

/**
 * TODO: Describe useApiHealth.
 */
export function useApiHealth() {
  const [status, setStatus] = useState<ApiHealthStatus>('loading');
  const [detail, setDetail] = useState<string>('');

  useEffect(() => {
    let cancelled = false;

    api
      .get<{ status: string }>('/api/health')
      .then((res) => {
        if (!cancelled) {
          setStatus('ok');
          setDetail(res.data?.status ?? 'ok');
        }
      })
      .catch((err: unknown) => {
        if (!cancelled) {
          setStatus('error');
          setDetail(err instanceof Error ? err.message : 'Erro ao contatar API');
        }
      });

    return () => {
      cancelled = true;
    };
  }, []);

  return { status, detail };
}
