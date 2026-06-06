import { cleanup, render, screen, waitFor } from '@testing-library/react';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import { afterEach, describe, expect, it, vi } from 'vitest';
import { HomePage } from './Home';
import { useAuthStore } from '../store/auth';
import { PERMISSIONS } from '../constants/permissions';
import * as reportsApi from '../services/reports';

vi.mock('../hooks/useApiHealth', () => ({
  useApiHealth: () => ({ status: 'ok' as const, detail: 'ok' }),
}));

vi.mock('../services/reports', () => ({
  getSalesReport: vi.fn(async () => ({ saleCount: 2, totalAmount: 99.5 })),
  getTopProductsReport: vi.fn(async () => [
    {
      productVariationId: '1',
      productName: 'Produto A',
      variationName: 'Único',
      quantitySold: 5,
      revenue: 50,
    },
  ]),
}));

describe('HomePage', () => {
  afterEach(() => {
    cleanup();
    useAuthStore.getState().logout();
    vi.clearAllMocks();
  });

  it('não exibe resumo gerencial sem permissão report.view', () => {
    useAuthStore.getState().setSession({
      accessToken: 't',
      refreshToken: 'r',
      userId: '1',
      email: 'a@b.com',
      permissions: [PERMISSIONS.productView],
      expiresAtUtc: new Date(Date.now() + 60_000).toISOString(),
    });

    render(
      <MemoryRouter initialEntries={['/']}>
        <Routes>
          <Route path="/" element={<HomePage />} />
        </Routes>
      </MemoryRouter>,
    );

    expect(screen.queryByRole('heading', { name: /resumo do período/i })).not.toBeInTheDocument();
    expect(reportsApi.getSalesReport).not.toHaveBeenCalled();
    expect(reportsApi.getTopProductsReport).not.toHaveBeenCalled();
  });

  it('carrega vendas e top 5 com report.view', async () => {
    useAuthStore.getState().setSession({
      accessToken: 't',
      refreshToken: 'r',
      userId: '1',
      email: 'a@b.com',
      permissions: [PERMISSIONS.reportView],
      expiresAtUtc: new Date(Date.now() + 60_000).toISOString(),
    });

    render(
      <MemoryRouter initialEntries={['/']}>
        <Routes>
          <Route path="/" element={<HomePage />} />
        </Routes>
      </MemoryRouter>,
    );

    expect(await screen.findByRole('heading', { name: /resumo do período/i })).toBeInTheDocument();
    expect(screen.getByRole('heading', { name: /^quantidade de vendas$/i })).toBeInTheDocument();
    expect(screen.getByRole('heading', { name: /^faturamento$/i })).toBeInTheDocument();
    expect(screen.getByRole('heading', { name: /produtos mais vendidos \(top 5\)/i })).toBeInTheDocument();

    await waitFor(() => {
      expect(reportsApi.getSalesReport).toHaveBeenCalledTimes(1);
      expect(reportsApi.getTopProductsReport).toHaveBeenCalledTimes(1);
    });

    const salesCall = vi.mocked(reportsApi.getSalesReport).mock.calls[0];
    const topCall = vi.mocked(reportsApi.getTopProductsReport).mock.calls[0];
    expect(salesCall?.[0]).toMatch(/T00:00:00\.000Z$/);
    expect(salesCall?.[1]).toMatch(/T23:59:59\.999Z$/);
    expect(topCall?.[2]).toBe(5);

    expect(await screen.findByText('2')).toBeInTheDocument();
    expect(screen.getByText(/R\$\s*99,50/)).toBeInTheDocument();
    expect(screen.getByText('Produto A')).toBeInTheDocument();
    expect(screen.getByText('Único')).toBeInTheDocument();
  });

  it('mostra mensagem quando não há vendas no top 5', async () => {
    vi.mocked(reportsApi.getTopProductsReport).mockResolvedValueOnce([]);

    useAuthStore.getState().setSession({
      accessToken: 't',
      refreshToken: 'r',
      userId: '1',
      email: 'a@b.com',
      permissions: [PERMISSIONS.reportView],
      expiresAtUtc: new Date(Date.now() + 60_000).toISOString(),
    });

    render(
      <MemoryRouter initialEntries={['/']}>
        <Routes>
          <Route path="/" element={<HomePage />} />
        </Routes>
      </MemoryRouter>,
    );

    expect(await screen.findByText(/nenhuma venda no período/i)).toBeInTheDocument();
  });
});
