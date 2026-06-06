import { cleanup, fireEvent, render, screen, within } from '@testing-library/react';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import { afterEach, describe, it, expect, vi } from 'vitest';
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
      userId: '1',
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

  it('mostra PDV quando há permissão sale.create', () => {
    useAuthStore.getState().setSession({
      accessToken: 't',
      refreshToken: 'r',
      userId: '1',
      email: 'a@b.com',
      permissions: [PERMISSIONS.saleCreate, PERMISSIONS.productView],
      expiresAtUtc: new Date(Date.now() + 60_000).toISOString(),
    });

    render(
      <MemoryRouter initialEntries={['/pdv']}>
        <Routes>
          <Route element={<AppShell />}>
            <Route path="pdv" element={<div>PDV página</div>} />
          </Route>
        </Routes>
      </MemoryRouter>,
    );

    const nav = screen.getByRole('navigation', { name: /principal/i });
    expect(within(nav).getByRole('link', { name: /^pdv$/i })).toHaveAttribute('href', '/pdv');
  });

  it('mostra Relatórios quando há permissão report.view', () => {
    useAuthStore.getState().setSession({
      accessToken: 't',
      refreshToken: 'r',
      userId: '1',
      email: 'a@b.com',
      permissions: [PERMISSIONS.reportView],
      expiresAtUtc: new Date(Date.now() + 60_000).toISOString(),
    });

    render(
      <MemoryRouter initialEntries={['/reports']}>
        <Routes>
          <Route element={<AppShell />}>
            <Route path="reports" element={<div>Relatórios página</div>} />
          </Route>
        </Routes>
      </MemoryRouter>,
    );

    const nav = screen.getByRole('navigation', { name: /principal/i });
    expect(within(nav).getByRole('link', { name: /^relatórios$/i })).toHaveAttribute('href', '/reports');
  });

  it('mostra Usuários e Roles quando há permissão user.manage', () => {
    useAuthStore.getState().setSession({
      accessToken: 't',
      refreshToken: 'r',
      userId: '1',
      email: 'a@b.com',
      permissions: [PERMISSIONS.userManage],
      expiresAtUtc: new Date(Date.now() + 60_000).toISOString(),
    });

    render(
      <MemoryRouter initialEntries={['/users']}>
        <Routes>
          <Route element={<AppShell />}>
            <Route path="users" element={<div>Usuários página</div>} />
            <Route path="roles" element={<div>Roles</div>} />
          </Route>
        </Routes>
      </MemoryRouter>,
    );

    const nav = screen.getByRole('navigation', { name: /principal/i });
    expect(within(nav).getByRole('link', { name: /^usuários$/i })).toHaveAttribute('href', '/users');
    expect(within(nav).getByRole('link', { name: /^roles$/i })).toHaveAttribute('href', '/roles');
  });

  it('mostra Roles quando há permissão role.manage', () => {
    useAuthStore.getState().setSession({
      accessToken: 't',
      refreshToken: 'r',
      userId: '1',
      email: 'a@b.com',
      permissions: [PERMISSIONS.roleManage],
      expiresAtUtc: new Date(Date.now() + 60_000).toISOString(),
    });

    render(
      <MemoryRouter initialEntries={['/roles']}>
        <Routes>
          <Route element={<AppShell />}>
            <Route path="roles" element={<div>Roles página</div>} />
          </Route>
        </Routes>
      </MemoryRouter>,
    );

    const nav = screen.getByRole('navigation', { name: /principal/i });
    expect(within(nav).getByRole('link', { name: /^roles$/i })).toHaveAttribute('href', '/roles');
  });

  it('em layout estreito exibe botão de menu e alterna aria-expanded', () => {
    const prevMatchMedia = window.matchMedia;
    window.matchMedia = (query: string) =>
      ({
        matches: query.includes('899px'),
        media: query,
        onchange: null,
        addListener: vi.fn(),
        removeListener: vi.fn(),
        addEventListener: vi.fn(),
        removeEventListener: vi.fn(),
        dispatchEvent: vi.fn(),
      }) as MediaQueryList;

    try {
      render(
        <MemoryRouter initialEntries={['/']}>
          <Routes>
            <Route element={<AppShell />}>
              <Route index element={<div>Início</div>} />
            </Route>
          </Routes>
        </MemoryRouter>,
      );

      const menuBtn = screen.getByRole('button', { name: /menu/i });
      expect(menuBtn).toHaveAttribute('aria-expanded', 'false');
      fireEvent.click(menuBtn);
      expect(menuBtn).toHaveAttribute('aria-expanded', 'true');
      fireEvent.click(screen.getByRole('button', { name: /fechar menu de navegação/i }));
      expect(menuBtn).toHaveAttribute('aria-expanded', 'false');
    } finally {
      window.matchMedia = prevMatchMedia;
    }
  });
});
