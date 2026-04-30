import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import { AxiosError } from 'axios';
import { afterEach, describe, expect, it, vi } from 'vitest';
import { useAuthStore } from '../store/auth';

const loginMock = vi.hoisted(() => vi.fn());

vi.mock('../services/auth', () => ({
  login: loginMock,
}));

import { LoginPage } from './Login';

describe('LoginPage', () => {
  afterEach(() => {
    cleanup();
    useAuthStore.getState().logout();
    vi.clearAllMocks();
  });

  it('navega para state.from após login bem-sucedido', async () => {
    loginMock.mockResolvedValue(undefined);

    render(
      <MemoryRouter initialEntries={[{ pathname: '/login', state: { from: '/reports' } }]}>
        <Routes>
          <Route path="/login" element={<LoginPage />} />
          <Route path="/reports" element={<div>Destino relatórios</div>} />
        </Routes>
      </MemoryRouter>,
    );

    fireEvent.change(screen.getByLabelText(/e-mail/i), { target: { value: 'user@test.com' } });
    fireEvent.change(screen.getByLabelText(/^senha$/i), { target: { value: 'segredo12' } });
    fireEvent.click(screen.getByRole('button', { name: /^entrar$/i }));

    await waitFor(() => {
      expect(screen.getByText(/destino relatórios/i)).toBeInTheDocument();
    });
    expect(loginMock).toHaveBeenCalledWith('user@test.com', 'segredo12');
  });

  it('exibe mensagem amigável em 401', async () => {
    const err = new AxiosError('Unauthorized');
    err.response = { status: 401, data: {} } as typeof err.response;
    loginMock.mockRejectedValue(err);

    render(
      <MemoryRouter initialEntries={['/login']}>
        <Routes>
          <Route path="/login" element={<LoginPage />} />
        </Routes>
      </MemoryRouter>,
    );

    fireEvent.change(screen.getByLabelText(/e-mail/i), { target: { value: 'a@b.com' } });
    fireEvent.change(screen.getByLabelText(/^senha$/i), { target: { value: 'wrong' } });
    fireEvent.click(screen.getByRole('button', { name: /^entrar$/i }));

    await waitFor(() => {
      expect(screen.getByRole('alert')).toHaveTextContent(/e-mail ou senha inválidos/i);
    });
  });
});
