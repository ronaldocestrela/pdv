import { cleanup, fireEvent, render, screen, waitFor, within } from '@testing-library/react';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import { afterEach, describe, expect, it, vi } from 'vitest';
import { PdvPage } from './PdvPage';
import { useAuthStore } from '../store/auth';
import { PERMISSIONS } from '../constants/permissions';

const createSaleMock = vi.hoisted(() => vi.fn());

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
  createSale: createSaleMock,
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

  it('adiciona ao carrinho e chama createSale ao finalizar', async () => {
    createSaleMock.mockResolvedValue({ saleId: 7, totalAmount: 59.8 });

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

    await waitFor(() => expect(screen.getByLabelText(/^produto$/i)).toBeInTheDocument());

    fireEvent.change(screen.getByLabelText(/^produto$/i), { target: { value: '1' } });
    await waitFor(() => expect(screen.getByLabelText(/^variação$/i)).not.toBeDisabled());

    fireEvent.change(screen.getByLabelText(/^variação$/i), { target: { value: '10' } });
    fireEvent.click(screen.getByRole('button', { name: /adicionar ao carrinho/i }));

    const cartCard = screen.getByRole('heading', { name: /^carrinho$/i }).parentElement!;
    expect(within(cartCard).getByText(/produto a/i)).toBeInTheDocument();
    expect(within(cartCard).getByText(/^única$/i)).toBeInTheDocument();

    fireEvent.click(screen.getByRole('button', { name: /^cartão$/i }));
    fireEvent.click(screen.getByRole('button', { name: /finalizar venda/i }));

    await waitFor(() => {
      expect(createSaleMock).toHaveBeenCalledWith({
        items: [{ productVariationId: 10, quantity: 1 }],
        paymentMethod: 'card',
      });
    });

    expect(await screen.findByText(/venda #7 registrada/i)).toBeInTheDocument();
  });

  it('mostra erro quando quantidade excede estoque disponível', async () => {
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

    await waitFor(() => expect(screen.getByLabelText(/^produto$/i)).toBeInTheDocument());
    fireEvent.change(screen.getByLabelText(/^produto$/i), { target: { value: '1' } });
    await waitFor(() => expect(screen.getByLabelText(/^variação$/i)).not.toBeDisabled());
    fireEvent.change(screen.getByLabelText(/^variação$/i), { target: { value: '10' } });

    fireEvent.change(screen.getByLabelText(/^qtd\.$/i), { target: { value: '99' } });
    fireEvent.click(screen.getByRole('button', { name: /adicionar ao carrinho/i }));

    expect(await screen.findByText(/estoque insuficiente/i)).toBeInTheDocument();
  });

  it('mostra erro quando createSale falha', async () => {
    createSaleMock.mockRejectedValue(new Error('API error'));

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

    await waitFor(() => expect(screen.getByLabelText(/^produto$/i)).toBeInTheDocument());
    fireEvent.change(screen.getByLabelText(/^produto$/i), { target: { value: '1' } });
    await waitFor(() => expect(screen.getByLabelText(/^variação$/i)).not.toBeDisabled());
    fireEvent.change(screen.getByLabelText(/^variação$/i), { target: { value: '10' } });
    fireEvent.click(screen.getByRole('button', { name: /adicionar ao carrinho/i }));

    fireEvent.click(screen.getByRole('button', { name: /finalizar venda/i }));

    expect(
      await screen.findByText(/não foi possível finalizar a venda/i),
    ).toBeInTheDocument();
  });

  it('desabilita checkout sem sale.create quando há apenas sale.view e product.view', async () => {
    useAuthStore.getState().setSession({
      accessToken: 't',
      refreshToken: 'r',
      userId: 1,
      email: 'a@b.com',
      permissions: [PERMISSIONS.saleView, PERMISSIONS.productView],
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
    expect(screen.getByRole('button', { name: /adicionar ao carrinho/i })).toBeDisabled();
    expect(screen.getByRole('button', { name: /finalizar venda/i })).toBeDisabled();
  });
});
