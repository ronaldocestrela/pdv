import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import { afterEach, describe, expect, it, vi } from 'vitest';
import { useAuthStore } from '../store/auth';
import { PERMISSIONS } from '../constants/permissions';

const listProductsMock = vi.hoisted(() => vi.fn());
const getProductMock = vi.hoisted(() => vi.fn());
const listStockMovementsMock = vi.hoisted(() => vi.fn());
const adjustStockMock = vi.hoisted(() => vi.fn());

vi.mock('../services/products', () => ({
  listProducts: listProductsMock,
  getProduct: getProductMock,
}));

vi.mock('../services/stock', () => ({
  listStockMovements: listStockMovementsMock,
  adjustStock: adjustStockMock,
}));

import { StockAdjustPage } from './StockAdjustPage';

function setSessionWithStockPerms() {
  useAuthStore.getState().setSession({
    accessToken: 't',
    refreshToken: 'r',
    userId: '1',
    email: 'a@b.com',
    permissions: [PERMISSIONS.stockAdjust, PERMISSIONS.stockView, PERMISSIONS.productView],
    expiresAtUtc: new Date(Date.now() + 60_000).toISOString(),
  });
}

describe('StockAdjustPage', () => {
  afterEach(() => {
    cleanup();
    useAuthStore.getState().logout();
    vi.clearAllMocks();
  });

  it('mostra bloqueio quando não há stock.adjust nem stock.view', () => {
    useAuthStore.getState().setSession({
      accessToken: 't',
      refreshToken: 'r',
      userId: '1',
      email: 'a@b.com',
      permissions: [PERMISSIONS.productView],
      expiresAtUtc: new Date(Date.now() + 60_000).toISOString(),
    });

    render(
      <MemoryRouter initialEntries={['/stock']}>
        <Routes>
          <Route path="/stock" element={<StockAdjustPage />} />
        </Routes>
      </MemoryRouter>,
    );

    expect(screen.getByText(/você não tem permissão para acessar esta área/i)).toBeInTheDocument();
  });

  it('registra entrada e chama adjustStock', async () => {
    setSessionWithStockPerms();
    listProductsMock.mockResolvedValue([{ id: '1', name: 'Produto X', isActive: true, variationCount: 1 }]);
    getProductMock.mockResolvedValue({
      id: '1',
      name: 'Produto X',
      isActive: true,
      variations: [
        { id: '90', productId: '1', name: 'V-Única', barcode: null, stockQuantity: 4, unitPrice: 10 },
      ],
    });
    listStockMovementsMock.mockResolvedValue([]);
    adjustStockMock.mockResolvedValue(undefined);

    render(
      <MemoryRouter initialEntries={['/stock']}>
        <Routes>
          <Route path="/stock" element={<StockAdjustPage />} />
        </Routes>
      </MemoryRouter>,
    );

    await waitFor(() => expect(listProductsMock).toHaveBeenCalled());

    fireEvent.change(screen.getByLabelText(/^produto$/i), { target: { value: '1' } });

    await waitFor(() => expect(getProductMock).toHaveBeenCalledWith('1'));

    fireEvent.change(screen.getByLabelText(/^variação$/i), { target: { value: '90' } });
    fireEvent.change(screen.getByLabelText(/^quantidade$/i), { target: { value: '2' } });
    fireEvent.click(screen.getByRole('button', { name: /registrar entrada/i }));

    await waitFor(() => {
      expect(adjustStockMock).toHaveBeenCalledWith({
        productVariationId: '90',
        quantity: 2,
        reason: null,
      });
    });
    expect(await screen.findByText(/entrada registrada com sucesso/i)).toBeInTheDocument();
  });

  it('exibe tabela de histórico quando há stock.view', async () => {
    setSessionWithStockPerms();
    listProductsMock.mockResolvedValue([]);
    listStockMovementsMock.mockResolvedValue([
      {
        id: '1',
        productVariationId: '1',
        productName: 'P',
        variationName: 'V',
        type: 'IN',
        quantity: 5,
        createdAtUtc: '2024-01-02T12:00:00.000Z',
        reason: null,
      },
    ]);

    render(
      <MemoryRouter initialEntries={['/stock']}>
        <Routes>
          <Route path="/stock" element={<StockAdjustPage />} />
        </Routes>
      </MemoryRouter>,
    );

    expect(await screen.findByRole('columnheader', { name: /tipo/i })).toBeInTheDocument();
    expect(screen.getByText('IN')).toBeInTheDocument();
  });
});
