import { useApiHealth } from '../hooks';
import { useAuthStore } from '../store/auth';

export function HomePage() {
  const { status, detail } = useApiHealth();
  const logout = useAuthStore((s) => s.logout);

  return (
    <main className="pdv-shell">
      <header className="home-header">
        <h1>PDV + Estoque</h1>
        <button type="button" className="logout-btn" onClick={() => logout()}>
          Sair
        </button>
      </header>
      <p className="muted">Fase 1 — autenticação</p>
      <section className="card">
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
    </main>
  );
}
