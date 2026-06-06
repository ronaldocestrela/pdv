/**
 * @file SuppliersPage.test.tsx
 * @description Tests for the SuppliersPage component covering permission-based visibility,
 * data listing, and CRUD modal interactions.
 */

import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import { afterEach, describe, expect, it, vi } from 'vitest';
import { useAuthStore } from '../store/auth';
import { PERMISSIONS } from '../constants/permissions';

const listSuppliersMock = vi.hoisted(() => vi.fn());
const createSupplierMock = vi.hoisted(() => vi.fn());
const updateSupplierMock = vi.hoisted(() => vi.fn());
const deleteSupplierMock = vi.hoisted(() => vi.fn());

vi.mock('../services/suppliers', () => ({
  listSuppliers: listSuppliersMock,
  createSupplier: createSupplierMock,
  updateSupplier: updateSupplierMock,
  deleteSupplier: deleteSupplierMock,
}));

import { SuppliersPage } from './SuppliersPage';

const mockSuppliers = [
  {
    id: '1',
    name: 'Fornecedor Alpha',
    document: '12.345.678/0001-90',
    email: 'alpha@email.com',
    phone: '11999999999',
    isActive: true,
  },
  {
    id: '2',
    name: 'Fornecedor Beta',
    document: null,
    email: null,
    phone: null,
    isActive: false,
  },
];

function renderPage(permissions: string[]) {
  useAuthStore.getState().setSession({
    accessToken: 't',
    refreshToken: 'r',
    userId: '1',
    email: 'a@b.com',
    permissions,
    expiresAtUtc: new Date(Date.now() + 60_000).toISOString(),
  });

  return render(
    <MemoryRouter initialEntries={['/suppliers']}>
      <Routes>
        <Route path="/suppliers" element={<SuppliersPage />} />
      </Routes>
    </MemoryRouter>,
  );
}

describe('SuppliersPage', () => {
  afterEach(() => {
    cleanup();
    useAuthStore.getState().logout();
    vi.clearAllMocks();
  });

  it('exibe mensagem de carregamento e depois a tabela', async () => {
    listSuppliersMock.mockResolvedValue(mockSuppliers);

    renderPage([PERMISSIONS.supplierView]);

    expect(screen.getByText(/carregando/i)).toBeInTheDocument();

    expect(await screen.findByText('Fornecedor Alpha')).toBeInTheDocument();
    expect(screen.getByText('Fornecedor Beta')).toBeInTheDocument();
    expect(screen.getByText('12.345.678/0001-90')).toBeInTheDocument();
    expect(screen.getByText('alpha@email.com')).toBeInTheDocument();
    expect(screen.getByText('11999999999')).toBeInTheDocument();
  });

  it('exibe mensagem de lista vazia quando não há fornecedores', async () => {
    listSuppliersMock.mockResolvedValue([]);

    renderPage([PERMISSIONS.supplierView]);

    expect(await screen.findByText(/nenhum fornecedor cadastrado/i)).toBeInTheDocument();
  });

  it('não exibe botão "Novo fornecedor" sem permissão supplier.create', async () => {
    listSuppliersMock.mockResolvedValue([]);

    renderPage([PERMISSIONS.supplierView]);

    await screen.findByText(/nenhum fornecedor cadastrado/i);
    expect(screen.queryByRole('button', { name: /novo fornecedor/i })).not.toBeInTheDocument();
  });

  it('exibe botão "Novo fornecedor" com permissão supplier.create', async () => {
    listSuppliersMock.mockResolvedValue([]);

    renderPage([PERMISSIONS.supplierView, PERMISSIONS.supplierCreate]);

    await screen.findByText(/nenhum fornecedor cadastrado/i);
    expect(screen.getByRole('button', { name: /novo fornecedor/i })).toBeInTheDocument();
  });

  it('não exibe botão "Editar" sem permissão supplier.update', async () => {
    listSuppliersMock.mockResolvedValue(mockSuppliers);

    renderPage([PERMISSIONS.supplierView]);

    await screen.findByText('Fornecedor Alpha');
    expect(screen.queryByRole('button', { name: /editar/i })).not.toBeInTheDocument();
  });

  it('exibe botão "Editar" com permissão supplier.update', async () => {
    listSuppliersMock.mockResolvedValue(mockSuppliers);

    renderPage([PERMISSIONS.supplierView, PERMISSIONS.supplierUpdate]);

    await screen.findByText('Fornecedor Alpha');
    const editButtons = screen.getAllByRole('button', { name: /editar/i });
    expect(editButtons).toHaveLength(2);
  });

  it('não exibe botão "Excluir" sem permissão supplier.delete', async () => {
    listSuppliersMock.mockResolvedValue(mockSuppliers);

    renderPage([PERMISSIONS.supplierView]);

    await screen.findByText('Fornecedor Alpha');
    expect(screen.queryByRole('button', { name: /excluir/i })).not.toBeInTheDocument();
  });

  it('abre modal ao clicar em "Novo fornecedor" e cria fornecedor', async () => {
    listSuppliersMock.mockResolvedValue([]);
    createSupplierMock.mockResolvedValue('new-id');

    renderPage([PERMISSIONS.supplierView, PERMISSIONS.supplierCreate]);

    await screen.findByText(/nenhum fornecedor cadastrado/i);
    fireEvent.click(screen.getByRole('button', { name: /novo fornecedor/i }));

    expect(screen.getByRole('dialog')).toBeInTheDocument();
    expect(screen.getByLabelText(/nome \/ razão social/i)).toBeInTheDocument();

    fireEvent.change(screen.getByLabelText(/nome \/ razão social/i), { target: { value: 'Novo Fornecedor' } });
    fireEvent.click(screen.getByRole('button', { name: /salvar/i }));

    await waitFor(() => {
      expect(createSupplierMock).toHaveBeenCalledWith(
        expect.objectContaining({ name: 'Novo Fornecedor' }),
      );
    });
  });

  it('abre confirmação ao clicar em "Excluir" e executa exclusão', async () => {
    listSuppliersMock.mockResolvedValue(mockSuppliers);
    deleteSupplierMock.mockResolvedValue(undefined);

    renderPage([PERMISSIONS.supplierView, PERMISSIONS.supplierDelete]);

    await screen.findByText('Fornecedor Alpha');
    const deleteButtons = screen.getAllByRole('button', { name: /excluir/i });
    fireEvent.click(deleteButtons[0]!);

    expect(screen.getByText(/excluir o fornecedor «fornecedor alpha»/i)).toBeInTheDocument();

    const confirmButton = screen.getAllByRole('button', { name: /excluir/i }).find(
      (btn) => btn.className.includes('pdv-btn--danger') && !btn.closest('td'),
    );
    fireEvent.click(confirmButton!);

    await waitFor(() => {
      expect(deleteSupplierMock).toHaveBeenCalledWith('1');
    });
  });
});
