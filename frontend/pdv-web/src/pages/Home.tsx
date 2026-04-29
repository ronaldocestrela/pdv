import { Link } from 'react-router-dom';
import { useApiHealth } from '../hooks';
import { PERMISSIONS } from '../constants/permissions';
import { can } from '../hooks/usePermission';

export function HomePage() {
  const { status, detail } = useApiHealth();
  const showProducts = can(PERMISSIONS.productView);

  return (
    <>
      <header className="pdv-main__header" style={{ marginBottom: '0.5rem' }}>
        <h1>Início</h1>
      </header>
      <p className="muted" style={{ marginTop: 0 }}>
        Fase 2 — produtos e variações disponíveis na barra lateral.
      </p>
      {showProducts && (
        <p style={{ marginTop: '0.75rem' }}>
          <Link className="pdv-breadcrumb" style={{ fontSize: '1rem' }} to="/products">
            Ir para Produtos →
          </Link>
        </p>
      )}
      <section className="card" style={{ marginTop: '1.25rem' }}>
        <h2>API</h2>
        <p>
          Status:{' '}
          <strong data-status={status}>
            {status === 'loading' && 'Carregando…'}
            {status === 'ok' && `OK (${detail})`}
            {status === 'error' && `Erro: ${detail}`}
          </strong>
        </p>
      </section>
    </>
  );
}
