import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import { AxiosError } from 'axios';
import { afterEach, describe, expect, it, vi } from 'vitest';

/**
 * Mock do serviço de tenants para isolar os testes de UI.
 */
const registerTenantMock = vi.hoisted(() => vi.fn());

vi.mock('../services/tenants', () => ({
  registerTenant: registerTenantMock,
}));

import { RegisterTenantPage } from './RegisterTenantPage';

describe('RegisterTenantPage', () => {
  afterEach(() => {
    cleanup();
    vi.clearAllMocks();
  });

  /**
   * Renderiza a página dentro de um MemoryRouter com rotas necessárias para testes.
   */
  function renderPage() {
    return render(
      <MemoryRouter initialEntries={['/register']}>
        <Routes>
          <Route path="/register" element={<RegisterTenantPage />} />
          <Route path="/login" element={<div>Página de login</div>} />
        </Routes>
      </MemoryRouter>,
    );
  }

  it('renderiza os campos obrigatórios do formulário', () => {
    renderPage();
    expect(screen.getByLabelText(/nome da empresa/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/e-mail do administrador/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/^senha$/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/confirmar senha/i)).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /criar conta/i })).toBeInTheDocument();
  });

  it('exibe erros de validação quando campos obrigatórios estão vazios', async () => {
    renderPage();
    fireEvent.click(screen.getByRole('button', { name: /criar conta/i }));

    await waitFor(() => {
      expect(screen.getByText(/nome da empresa é obrigatório/i)).toBeInTheDocument();
    });
  });

  it('exibe erro quando a senha é muito curta', async () => {
    registerTenantMock.mockResolvedValue(1);
    renderPage();

    fireEvent.change(screen.getByLabelText(/nome da empresa/i), { target: { value: 'Loja Teste' } });
    fireEvent.change(screen.getByLabelText(/e-mail do administrador/i), { target: { value: 'admin@loja.com' } });
    fireEvent.change(screen.getByLabelText(/^senha$/i), { target: { value: '123' } });
    fireEvent.change(screen.getByLabelText(/confirmar senha/i), { target: { value: '123' } });
    fireEvent.click(screen.getByRole('button', { name: /criar conta/i }));

    await waitFor(() => {
      expect(screen.getByText(/mínimo 6 caracteres/i)).toBeInTheDocument();
    });
  });

  it('exibe erro quando as senhas não coincidem', async () => {
    renderPage();

    fireEvent.change(screen.getByLabelText(/nome da empresa/i), { target: { value: 'Loja Teste' } });
    fireEvent.change(screen.getByLabelText(/e-mail do administrador/i), { target: { value: 'admin@loja.com' } });
    fireEvent.change(screen.getByLabelText(/^senha$/i), { target: { value: 'senha123' } });
    fireEvent.change(screen.getByLabelText(/confirmar senha/i), { target: { value: 'senha456' } });
    fireEvent.click(screen.getByRole('button', { name: /criar conta/i }));

    await waitFor(() => {
      expect(screen.getByText(/senhas não coincidem/i)).toBeInTheDocument();
    });
  });

  it('chama registerTenant com os dados corretos e exibe mensagem de sucesso', async () => {
    registerTenantMock.mockResolvedValue(42);
    renderPage();

    fireEvent.change(screen.getByLabelText(/nome da empresa/i), { target: { value: 'Empresa OK' } });
    fireEvent.change(screen.getByLabelText(/e-mail do administrador/i), { target: { value: 'admin@empresa.com' } });
    fireEvent.change(screen.getByLabelText(/^senha$/i), { target: { value: 'senha123' } });
    fireEvent.change(screen.getByLabelText(/confirmar senha/i), { target: { value: 'senha123' } });
    fireEvent.click(screen.getByRole('button', { name: /criar conta/i }));

    await waitFor(() => {
      expect(registerTenantMock).toHaveBeenCalledWith({
        name: 'Empresa OK',
        adminEmail: 'admin@empresa.com',
        adminPassword: 'senha123',
      });
      expect(screen.getByText(/conta criada/i)).toBeInTheDocument();
    });
  });

  it('exibe mensagem de erro da API quando o registro falha', async () => {
    const err = new AxiosError('Bad Request');
    err.response = {
      status: 400,
      data: { title: 'Já existe um tenant com este nome.' },
    } as typeof err.response;
    registerTenantMock.mockRejectedValue(err);
    renderPage();

    fireEvent.change(screen.getByLabelText(/nome da empresa/i), { target: { value: 'Loja Duplicada' } });
    fireEvent.change(screen.getByLabelText(/e-mail do administrador/i), { target: { value: 'admin@dup.com' } });
    fireEvent.change(screen.getByLabelText(/^senha$/i), { target: { value: 'senha123' } });
    fireEvent.change(screen.getByLabelText(/confirmar senha/i), { target: { value: 'senha123' } });
    fireEvent.click(screen.getByRole('button', { name: /criar conta/i }));

    await waitFor(() => {
      expect(screen.getByRole('alert')).toHaveTextContent(/já existe um tenant/i);
    });
  });
});
