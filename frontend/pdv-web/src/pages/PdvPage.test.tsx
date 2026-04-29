import { cleanup, render, screen } from '@testing-library/react';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import { afterEach, describe, expect, it, vi } from 'vitest';
import { PdvPage } from './PdvPage';
import { useAuthStore } from '../store/auth';
import { PERMISSIONS } from '../constants/permissions';

vi.mock('../services/products', () => ({
  listProducts: vi.fn(async () => [
    { id: 1, name: 'Produto A', isActive: true, variationCount: 1 },
  ]),
  getProduct: vi.fn(async () => ({
    id: 1,
    name: 'Produto A',
    isActive: true,
    variations: [{ id: 10, productId: 1, name: 'Única', barcode: null, stockQuantity: 5, unitPrice: 29.9 }],
  })),
}));

vi.mock('../services/sales', () => ({
  listSales: vi.fn(async () => []),
  createSale: vi.fn(),
}));

describe('PdvPage', () => {
  afterEach(() => {
    cleanup();
    useAuthStore.getState().logout();
    vi.clearAllMocks();
  });

  it('renderiza checkout quando há sale.create e product.view', async () => {
    useAuthStore.getState().setSession({
      accessToken: 't',
      refreshToken: 'r',
      userId: 1,
      email: 'a@b.com',
      permissions: [PERMISSIONS.saleCreate, PERMISSIONS.saleView, PERMISSIONS.productView],
      expiresAtUtc: new Date(Date.now() + 60_000).toISOString(),
    });

    render(
      <MemoryRouter initialEntries={['/pdv']}>
        <Routes>
          <Route path="/pdv" element={<PdvPage />} />
        </Routes>
      </MemoryRouter>,
    );

    expect(await screen.findByRole('heading', { name: /pdv — checkout/i })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /finalizar venda/i })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /dinheiro/i })).toBeInTheDocument();
  });
});
