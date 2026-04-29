import { render, screen } from '@testing-library/react';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import { describe, it, expect } from 'vitest';
import { AppShell } from '../components/AppShell';

describe('AppShell', () => {
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
});
