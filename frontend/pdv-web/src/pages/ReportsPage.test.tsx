import { cleanup, render, screen } from '@testing-library/react';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import { afterEach, describe, expect, it, vi } from 'vitest';
import { ReportsPage } from './ReportsPage';
import { useAuthStore } from '../store/auth';
import { PERMISSIONS } from '../constants/permissions';

vi.mock('../services/reports', () => ({
  getSalesReport: vi.fn(async () => ({ saleCount: 1, totalAmount: 10 })),
  getTopProductsReport: vi.fn(async () => []),
  getCashFlowReport: vi.fn(async () => []),
  getStockReport: vi.fn(async () => []),
}));

describe('ReportsPage', () => {
  afterEach(() => {
    cleanup();
    useAuthStore.getState().logout();
    vi.clearAllMocks();
  });

  it('nega acesso sem permissões de relatório', () => {
    useAuthStore.getState().setSession({
      accessToken: 't',
      refreshToken: 'r',
      userId: '1',
      email: 'a@b.com',
      permissions: [PERMISSIONS.productView],
      expiresAtUtc: new Date(Date.now() + 60_000).toISOString(),
    });

    render(
      <MemoryRouter initialEntries={['/reports']}>
        <Routes>
          <Route path="/reports" element={<ReportsPage />} />
        </Routes>
      </MemoryRouter>,
    );

    expect(screen.getByText(/você não tem permissão/i)).toBeInTheDocument();
  });

  it('renderiza relatórios com report.view e cashflow.view', async () => {
    useAuthStore.getState().setSession({
      accessToken: 't',
      refreshToken: 'r',
      userId: '1',
      email: 'a@b.com',
      permissions: [PERMISSIONS.reportView, PERMISSIONS.cashflowView],
      expiresAtUtc: new Date(Date.now() + 60_000).toISOString(),
    });

    render(
      <MemoryRouter initialEntries={['/reports']}>
        <Routes>
          <Route path="/reports" element={<ReportsPage />} />
        </Routes>
      </MemoryRouter>,
    );

    expect(await screen.findByRole('heading', { name: /^relatórios$/i })).toBeInTheDocument();
    expect(screen.getByRole('heading', { name: /produtos mais vendidos/i })).toBeInTheDocument();
    expect(screen.getByRole('heading', { name: /fluxo de caixa/i })).toBeInTheDocument();
    expect(screen.getByRole('heading', { name: /estoque atual/i })).toBeInTheDocument();
  });
});
