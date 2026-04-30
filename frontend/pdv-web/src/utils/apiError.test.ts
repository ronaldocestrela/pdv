import axios from 'axios';
import { describe, expect, it } from 'vitest';
import { getApiErrorMessage } from './apiError';

describe('getApiErrorMessage', () => {
  it('uses ProblemDetails.detail when present', () => {
    const err = new axios.AxiosError(
      'fail',
      'ERR_BAD_REQUEST',
      undefined,
      undefined,
      {
        status: 400,
        data: { detail: 'Estoque insuficiente.', title: 'Validation failed', status: 400 },
        statusText: 'Bad Request',
        headers: {},
        config: {} as never,
      },
    );
    expect(getApiErrorMessage(err, 'fallback')).toBe('Estoque insuficiente.');
  });

  it('uses first errors[].message when detail missing', () => {
    const err = new axios.AxiosError('fail', 'ERR_BAD_REQUEST', undefined, undefined, {
      status: 400,
      data: {
        title: 'Validation failed',
        errors: [{ field: 'items', message: 'Linha inválida.' }],
      },
      statusText: 'Bad Request',
      headers: {},
      config: {} as never,
    });
    expect(getApiErrorMessage(err, 'fallback')).toBe('Linha inválida.');
  });

  it('returns fallback for non-axios errors', () => {
    expect(getApiErrorMessage(new Error('x'), 'fallback')).toBe('fallback');
  });
});
