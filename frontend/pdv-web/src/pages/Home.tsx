import { Link } from 'react-router-dom';
import { useCallback, useEffect, useMemo, useState } from 'react';
import { useApiHealth } from '../hooks';
import { PERMISSIONS } from '../constants/permissions';
import { can } from '../hooks/usePermission';
import * as reportsApi from '../services/reports';
import type { SalesReportDto, TopProductReportDto } from '../types/reports';

function formatBRL(value: number): string {
  return new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(value);
}

function defaultDateRange(): { from: string; to: string } {
  const end = new Date();
  const start = new Date(end);
  start.setUTCDate(start.getUTCDate() - 30);
  return {
    from: start.toISOString().slice(0, 10),
    to: end.toISOString().slice(0, 10),
  };
}

function dateToFromUtc(isoDate: string): string {
  return `${isoDate}T00:00:00.000Z`;
}

function dateToToUtc(isoDate: string): string {
  return `${isoDate}T23:59:59.999Z`;
}

/**
 * TODO: Describe HomePage.
 */
export function HomePage() {
  const { status, detail } = useApiHealth();
  const showProducts = can(PERMISSIONS.productView);
  const canReports = can(PERMISSIONS.reportView);

  const initialRange = useMemo(() => defaultDateRange(), []);
  const [fromDate, setFromDate] = useState(initialRange.from);
  const [toDate, setToDate] = useState(initialRange.to);

  const [sales, setSales] = useState<SalesReportDto | null>(null);
  const [topProducts, setTopProducts] = useState<TopProductReportDto[]>([]);
  const [loadingSales, setLoadingSales] = useState(false);
  const [loadingTop, setLoadingTop] = useState(false);
  const [reportError, setReportError] = useState<string | null>(null);

  const fromUtc = useMemo(() => dateToFromUtc(fromDate), [fromDate]);
  const toUtc = useMemo(() => dateToToUtc(toDate), [toDate]);

  const loadReports = useCallback(async () => {
    setReportError(null);
    setLoadingSales(true);
    setLoadingTop(true);

    const salesTask = reportsApi
      .getSalesReport(fromUtc, toUtc)
      .then(setSales)
      .catch(() => {
        setReportError('Não foi possível carregar o resumo de vendas.');
      })
      .finally(() => setLoadingSales(false));

    const topTask = reportsApi
      .getTopProductsReport(fromUtc, toUtc, 5)
      .then(setTopProducts)
      .catch(() => {
        setReportError((prev) => prev ?? 'Não foi possível carregar produtos mais vendidos.');
      })
      .finally(() => setLoadingTop(false));

    await Promise.all([salesTask, topTask]);
  }, [fromUtc, toUtc]);

  useEffect(() => {
    if (!canReports) return;
    void loadReports();
    // eslint-disable-next-line react-hooks/exhaustive-deps -- período só atualiza no botão "Atualizar período"
  }, [canReports]);

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

      {canReports && (
        <section className="pdv-card" style={{ marginTop: '1.25rem', padding: '1rem 1.25rem' }} aria-labelledby="home-metrics-heading">
          <h2 id="home-metrics-heading" style={{ marginTop: 0, marginBottom: '1rem', fontSize: '1.05rem', fontWeight: 600 }}>
            Resumo do período
          </h2>

          {reportError && <p className="pdv-error">{reportError}</p>}

          <div className="pdv-toolbar" style={{ marginBottom: '1rem', flexWrap: 'wrap' }}>
            <div className="pdv-field" style={{ marginBottom: 0, minWidth: 160 }}>
              <label htmlFor="home-from">Data inicial</label>
              <input
                id="home-from"
                type="date"
                value={fromDate}
                onChange={(e) => setFromDate(e.target.value)}
              />
            </div>
            <div className="pdv-field" style={{ marginBottom: 0, minWidth: 160 }}>
              <label htmlFor="home-to">Data final</label>
              <input
                id="home-to"
                type="date"
                value={toDate}
                onChange={(e) => setToDate(e.target.value)}
              />
            </div>
            <button
              type="button"
              className="pdv-btn pdv-btn--primary"
              style={{ alignSelf: 'flex-end' }}
              onClick={() => void loadReports()}
            >
              Atualizar período
            </button>
          </div>

          <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(260px, 1fr))', gap: '1rem', marginBottom: '1rem' }}>
            <div className="pdv-card" style={{ padding: '1rem 1.25rem' }}>
              <h3 style={{ margin: 0, fontSize: '0.875rem', fontWeight: 600, color: 'var(--pdv-muted)' }}>Quantidade de vendas</h3>
              {loadingSales ? (
                <p className="pdv-empty" style={{ padding: '1rem 0' }}>
                  Carregando…
                </p>
              ) : sales ? (
                <>
                  <p style={{ margin: '0.5rem 0 0', fontSize: '1.75rem', fontWeight: 700 }}>{sales.saleCount}</p>
                  <p style={{ margin: '0.25rem 0 0', color: 'var(--pdv-muted)', fontSize: '0.875rem' }}>no período selecionado</p>
                </>
              ) : (
                <p className="pdv-empty" style={{ padding: '1rem 0' }}>
                  Sem dados.
                </p>
              )}
            </div>
            <div className="pdv-card" style={{ padding: '1rem 1.25rem' }}>
              <h3 style={{ margin: 0, fontSize: '0.875rem', fontWeight: 600, color: 'var(--pdv-muted)' }}>Faturamento</h3>
              {loadingSales ? (
                <p className="pdv-empty" style={{ padding: '1rem 0' }}>
                  Carregando…
                </p>
              ) : sales ? (
                <>
                  <p style={{ margin: '0.5rem 0 0', fontSize: '1.75rem', fontWeight: 600 }}>{formatBRL(sales.totalAmount)}</p>
                  <p style={{ margin: '0.25rem 0 0', color: 'var(--pdv-muted)', fontSize: '0.875rem' }}>total faturado no período</p>
                </>
              ) : (
                <p className="pdv-empty" style={{ padding: '1rem 0' }}>
                  Sem dados.
                </p>
              )}
            </div>
          </div>

          <div className="pdv-card pdv-table-wrap" style={{ marginBottom: 0 }}>
            <h3 style={{ margin: 0, padding: '1rem 1.25rem', fontSize: '1.05rem', fontWeight: 600 }}>Produtos mais vendidos (top 5)</h3>
            {loadingTop ? (
              <p className="pdv-empty">Carregando…</p>
            ) : topProducts.length === 0 ? (
              <p className="pdv-empty">Nenhuma venda no período.</p>
            ) : (
              <table className="pdv-table">
                <thead>
                  <tr>
                    <th>Produto</th>
                    <th>Variação</th>
                    <th className="num">Qtd vendida</th>
                    <th className="num">Receita</th>
                  </tr>
                </thead>
                <tbody>
                  {topProducts.map((row) => (
                    <tr key={row.productVariationId}>
                      <td>{row.productName}</td>
                      <td>{row.variationName}</td>
                      <td className="num">{row.quantitySold}</td>
                      <td className="num">{formatBRL(row.revenue)}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            )}
          </div>
        </section>
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
