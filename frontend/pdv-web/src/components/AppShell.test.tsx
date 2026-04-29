import { cleanup, render, screen, within } from '@testing-library/react';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import { afterEach, describe, it, expect } from 'vitest';
import { AppShell } from '../components/AppShell';
import { useAuthStore } from '../store/auth';
import { PERMISSIONS } from '../constants/permissions';

describe('AppShell', () => {
  afterEach(() => {
    cleanup();
    useAuthStore.getState().logout();
  });

  it('renderiza links de navegação principal', () => {
    render(
      <MemoryRouter initialEntries={['/products']}>
        <Routes>
          <Route element={<AppShell />}>
            <Route index element={<div>Início</div>} />
            <Route path="products" element={<div>Produtos lista</div>} />
          </Route>
        </Routes>
      </MemoryRouter>,
    );

    expect(screen.getByRole('link', { name: /início/i })).toBeInTheDocument();
    expect(screen.getByRole('link', { name: /produtos/i })).toBeInTheDocument();
  });

  it('mostra Estoque quando há permissão stock.view', () => {
    useAuthStore.getState().setSession({
      accessToken: 't',
      refreshToken: 'r',
      userId: 1,
      email: 'a@b.com',
      permissions: [PERMISSIONS.stockView, PERMISSIONS.productView],
      expiresAtUtc: new Date(Date.now() + 60_000).toISOString(),
    });

    render(
      <MemoryRouter initialEntries={['/stock']}>
        <Routes>
          <Route element={<AppShell />}>
            <Route path="stock" element={<div>Estoque</div>} />
          </Route>
        </Routes>
      </MemoryRouter>,
    );

    const nav = screen.getByRole('navigation', { name: /principal/i });
    expect(within(nav).getByRole('link', { name: /^estoque$/i })).toHaveAttribute('href', '/stock');
  });
});
