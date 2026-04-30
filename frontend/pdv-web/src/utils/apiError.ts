import axios from 'axios';

/** Reads message from RFC 7807 ProblemDetails or legacy API shapes. */
export function getApiErrorMessage(err: unknown, fallback: string): string {
  if (!axios.isAxiosError(err)) return fallback;

  const data = err.response?.data as Record<string, unknown> | undefined;
  if (!data) return fallback;

  if (typeof data.detail === 'string' && data.detail.trim()) return data.detail;

  const errs = data.errors;
  if (Array.isArray(errs) && errs.length > 0) {
    const first = errs[0] as { message?: string; error?: string } | undefined;
    if (first?.message) return first.message;
    if (typeof first?.error === 'string') return first.error;
  }

  if (typeof data.message === 'string' && data.message !== 'Validation failed') return data.message;

  return fallback;
}
