import { useCallback, useEffect, useMemo, useState } from 'react';
import { can } from '../hooks/usePermission';
import { PERMISSIONS } from '../constants/permissions';
import * as reportsApi from '../services/reports';
import type { CashFlowReportRowDto, SalesReportDto, StockReportRowDto, TopProductReportDto } from '../types/reports';

function formatBRL(value: number): string {
  return new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(value);
}

function defaultRangeUtc(): { fromUtc: string; toUtc: string } {
  const end = new Date();
  const start = new Date(end);
  start.setUTCDate(start.getUTCDate() - 30);
  start.setUTCHours(0, 0, 0, 0);
  end.setUTCHours(23, 59, 59, 999);
  return { fromUtc: start.toISOString(), toUtc: end.toISOString() };
}

function cashFlowLabel(t: CashFlowReportRowDto['type']): string {
  return t === 0 ? 'Entrada' : 'Saída';
}

export function ReportsPage() {
  const canReports = can(PERMISSIONS.reportView);
  const canCashflow = can(PERMISSIONS.cashflowView);

  const initialRange = useMemo(() => defaultRangeUtc(), []);
  const [fromUtc, setFromUtc] = useState(initialRange.fromUtc.slice(0, 16));
  const [toUtc, setToUtc] = useState(initialRange.toUtc.slice(0, 16));

  const [sales, setSales] = useState<SalesReportDto | null>(null);
  const [topProducts, setTopProducts] = useState<TopProductReportDto[]>([]);
  const [cashFlows, setCashFlows] = useState<CashFlowReportRowDto[]>([]);
  const [stock, setStock] = useState<StockReportRowDto[] | null>(null);

  const [loadingSales, setLoadingSales] = useState(false);
  const [loadingTop, setLoadingTop] = useState(false);
  const [loadingCash, setLoadingCash] = useState(false);
  const [loadingStock, setLoadingStock] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const fromIso = useMemo(() => new Date(fromUtc).toISOString(), [fromUtc]);
  const toIso = useMemo(() => new Date(toUtc).toISOString(), [toUtc]);

  const loadPeriodReports = useCallback(async () => {
    setError(null);
    const tasks: Promise<void>[] = [];

    if (canReports) {
      setLoadingSales(true);
      tasks.push(
        reportsApi
          .getSalesReport(fromIso, toIso)
          .then(setSales)
          .catch(() => {
            setError('Não foi possível carregar o resumo de vendas.');
          })
          .finally(() => setLoadingSales(false)),
      );

      setLoadingTop(true);
      tasks.push(
        reportsApi
          .getTopProductsReport(fromIso, toIso, 20)
          .then(setTopProducts)
          .catch(() => {
            setError('Não foi possível carregar produtos mais vendidos.');
          })
          .finally(() => setLoadingTop(false)),
      );
    }

    if (canCashflow) {
      setLoadingCash(true);
      tasks.push(
        reportsApi
          .getCashFlowReport(fromIso, toIso, 100)
          .then(setCashFlows)
          .catch(() => {
            setError('Não foi possível carregar o fluxo de caixa.');
          })
          .finally(() => setLoadingCash(false)),
      );
    }

    await Promise.all(tasks);
  }, [canReports, canCashflow, fromIso, toIso]);

  const loadStock = useCallback(async () => {
    if (!canReports) return;
    setLoadingStock(true);
    setError(null);
    try {
      const rows = await reportsApi.getStockReport(500);
      setStock(rows);
    } catch {
      setError('Não foi possível carregar o estoque atual.');
    } finally {
      setLoadingStock(false);
    }
  }, [canReports]);

  const loadAll = useCallback(async () => {
    await loadPeriodReports();
    await loadStock();
  }, [loadPeriodReports, loadStock]);

  useEffect(() => {
    if (!canReports && !canCashflow) return;
    void loadAll();
    // Período só atualiza ao clicar nos botões — não incluir loadAll (varia com inputs).
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [canReports, canCashflow]);

  if (!canReports && !canCashflow) {
    return (
      <div className="pdv-main__header">
        <h1>Relatórios</h1>
        <p className="pdv-error">Você não tem permissão para acessar relatórios.</p>
      </div>
    );
  }

  return (
    <>
      <div className="pdv-main__header">
        <div>
          <h1>Relatórios</h1>
          <p style={{ margin: '0.25rem 0 0', color: 'var(--pdv-muted, #45474c)', fontSize: '0.9rem' }}>
            Visão gerencial (Fase 5). Referência Stitch:{' '}
            <code style={{ fontSize: '0.85rem' }}>docs/design/stitch-phase5-reports-ui.md</code>
          </p>
        </div>
      </div>

      {error && <p className="pdv-error">{error}</p>}

      <div className="pdv-card" style={{ marginBottom: '1rem', padding: '1rem 1.25rem' }}>
        <h2 style={{ marginTop: 0, marginBottom: '1rem', fontSize: '1.05rem', fontWeight: 600 }}>Período</h2>
        <div className="pdv-toolbar">
          <div className="pdv-field" style={{ marginBottom: 0, minWidth: 200 }}>
            <label htmlFor="rep-from">De (UTC)</label>
            <input
              id="rep-from"
              type="datetime-local"
              value={fromUtc}
              onChange={(e) => setFromUtc(e.target.value)}
            />
          </div>
          <div className="pdv-field" style={{ marginBottom: 0, minWidth: 200 }}>
            <label htmlFor="rep-to">Até (UTC)</label>
            <input
              id="rep-to"
              type="datetime-local"
              value={toUtc}
              onChange={(e) => setToUtc(e.target.value)}
            />
          </div>
          <button type="button" className="pdv-btn pdv-btn--primary" style={{ alignSelf: 'flex-end' }} onClick={() => void loadPeriodReports()}>
            Atualizar período
          </button>
          {canReports && (
            <button
              type="button"
              className="pdv-btn pdv-btn--ghost"
              style={{ alignSelf: 'flex-end' }}
              onClick={() => void loadStock()}
            >
              Atualizar estoque
            </button>
          )}
          <button type="button" className="pdv-btn pdv-btn--ghost" style={{ alignSelf: 'flex-end' }} onClick={() => void loadAll()}>
            Atualizar tudo
          </button>
        </div>
      </div>

      {canReports && (
        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(260px, 1fr))', gap: '1rem', marginBottom: '1rem' }}>
          <div className="pdv-card" style={{ padding: '1rem 1.25rem' }}>
            <h3 style={{ margin: 0, fontSize: '0.875rem', fontWeight: 600, color: 'var(--pdv-muted)' }}>Vendas no período</h3>
            {loadingSales ? (
              <p className="pdv-empty" style={{ padding: '1rem 0' }}>
                Carregando…
              </p>
            ) : sales ? (
              <>
                <p style={{ margin: '0.5rem 0 0', fontSize: '1.75rem', fontWeight: 700 }}>{sales.saleCount}</p>
                <p style={{ margin: '0.25rem 0 0', color: 'var(--pdv-muted)', fontSize: '0.875rem' }}>quantidade de vendas</p>
                <p style={{ margin: '0.75rem 0 0', fontSize: '1.25rem', fontWeight: 600 }}>{formatBRL(sales.totalAmount)}</p>
                <p style={{ margin: '0.25rem 0 0', color: 'var(--pdv-muted)', fontSize: '0.875rem' }}>faturamento</p>
              </>
            ) : (
              <p className="pdv-empty" style={{ padding: '1rem 0' }}>
                Clique em atualizar.
              </p>
            )}
          </div>
        </div>
      )}

      {canReports && (
        <div className="pdv-card pdv-table-wrap" style={{ marginBottom: '1rem' }}>
          <h2 style={{ margin: 0, padding: '1rem 1.25rem', fontSize: '1.05rem', fontWeight: 600 }}>Produtos mais vendidos</h2>
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
      )}

      {canCashflow && (
        <div className="pdv-card pdv-table-wrap" style={{ marginBottom: '1rem' }}>
          <h2 style={{ margin: 0, padding: '1rem 1.25rem', fontSize: '1.05rem', fontWeight: 600 }}>Fluxo de caixa</h2>
          {loadingCash ? (
            <p className="pdv-empty">Carregando…</p>
          ) : cashFlows.length === 0 ? (
            <p className="pdv-empty">Nenhum lançamento no período.</p>
          ) : (
            <table className="pdv-table">
              <thead>
                <tr>
                  <th>Data</th>
                  <th>Tipo</th>
                  <th className="num">Valor</th>
                  <th>Descrição</th>
                  <th className="num">Venda</th>
                </tr>
              </thead>
              <tbody>
                {cashFlows.map((row) => (
                  <tr key={row.id}>
                    <td>{new Date(row.createdAtUtc).toLocaleString('pt-BR')}</td>
                    <td>
                      <span className={`pdv-chip ${row.type === 0 ? 'pdv-chip--ok' : 'pdv-chip--danger'}`}>
                        {cashFlowLabel(row.type)}
                      </span>
                    </td>
                    <td className="num">{formatBRL(row.amount)}</td>
                    <td>{row.description || '—'}</td>
                    <td className="num">{row.saleId ?? '—'}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        </div>
      )}

      {canReports && (
        <div className="pdv-card pdv-table-wrap">
          <h2 style={{ margin: 0, padding: '1rem 1.25rem', fontSize: '1.05rem', fontWeight: 600 }}>Estoque atual</h2>
          {loadingStock ? (
            <p className="pdv-empty">Carregando…</p>
          ) : stock === null ? (
            <p className="pdv-empty">Carregando estoque…</p>
          ) : stock.length === 0 ? (
            <p className="pdv-empty">Nenhuma variação cadastrada.</p>
          ) : (
            <table className="pdv-table">
              <thead>
                <tr>
                  <th>Produto</th>
                  <th>Variação</th>
                  <th className="num">Estoque</th>
                </tr>
              </thead>
              <tbody>
                {stock.map((row) => (
                  <tr key={row.productVariationId}>
                    <td>{row.productName}</td>
                    <td>{row.variationName}</td>
                    <td className="num">{row.stockQuantity}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        </div>
      )}
    </>
  );
}
