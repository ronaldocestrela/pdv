import { cleanup, render, screen } from '@testing-library/react';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import { afterEach, describe, expect, it } from 'vitest';
import { useAuthStore } from '../store/auth';
import { ProtectedRoute } from './ProtectedRoute';

describe('ProtectedRoute', () => {
  afterEach(() => {
    cleanup();
    useAuthStore.getState().logout();
  });

  it('redireciona para /login quando não há accessToken', () => {
    render(
      <MemoryRouter initialEntries={['/secret']}>
        <Routes>
          <Route path="/login" element={<div>Página de login</div>} />
          <Route
            path="/secret"
            element={
              <ProtectedRoute>
                <div>Área protegida</div>
              </ProtectedRoute>
            }
          />
        </Routes>
      </MemoryRouter>,
    );

    expect(screen.getByText(/página de login/i)).toBeInTheDocument();
    expect(screen.queryByText(/área protegida/i)).not.toBeInTheDocument();
  });

  it('renderiza children quando há accessToken', () => {
    useAuthStore.getState().setSession({
      accessToken: 'token',
      refreshToken: 'r',
      userId: '1',
      email: 'z@z.com',
      permissions: [],
      expiresAtUtc: new Date(Date.now() + 60_000).toISOString(),
    });

    render(
      <MemoryRouter initialEntries={['/secret']}>
        <Routes>
          <Route path="/login" element={<div>Página de login</div>} />
          <Route
            path="/secret"
            element={
              <ProtectedRoute>
                <div>Área protegida</div>
              </ProtectedRoute>
            }
          />
        </Routes>
      </MemoryRouter>,
    );

    expect(screen.queryByText(/página de login/i)).not.toBeInTheDocument();
    expect(screen.getByText(/área protegida/i)).toBeInTheDocument();
  });
});
