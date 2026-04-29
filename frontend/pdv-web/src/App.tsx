import './App.css';
import { useApiHealth } from './hooks';

function App() {
  const { status, detail } = useApiHealth();

  return (
    <main className="pdv-shell">
      <h1>PDV + Estoque</h1>
      <p className="muted">Fase 0 — fundação</p>
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

export default App;
