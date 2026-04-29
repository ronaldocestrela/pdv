import { useCallback, useEffect, useState } from 'react';
import { useForm } from 'react-hook-form';
import { can } from '../hooks/usePermission';
import { PERMISSIONS } from '../constants/permissions';
import * as productsApi from '../services/products';
import * as stockApi from '../services/stock';
import type { ProductDetailDto, ProductSummaryDto } from '../types/products';
import type { StockMovementDto } from '../types/stock';

type AdjustForm = { quantity: number; reason: string };

export function StockAdjustPage() {
  const canAdjust = can(PERMISSIONS.stockAdjust);
  const canViewHistory = can(PERMISSIONS.stockView);
  const canListProducts = can(PERMISSIONS.productView);

  const [products, setProducts] = useState<ProductSummaryDto[]>([]);
  const [productId, setProductId] = useState<number | ''>('');
  const [detail, setDetail] = useState<ProductDetailDto | null>(null);
  const [variationId, setVariationId] = useState<number | ''>('');
  const [movements, setMovements] = useState<StockMovementDto[]>([]);
  const [loadingProducts, setLoadingProducts] = useState(false);
  const [loadingDetail, setLoadingDetail] = useState(false);
  const [loadingMovements, setLoadingMovements] = useState(false);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [notice, setNotice] = useState<string | null>(null);

  const form = useForm<AdjustForm>({
    defaultValues: { quantity: 1, reason: '' },
  });

  const loadProducts = useCallback(async () => {
    if (!canListProducts) return;
    setLoadingProducts(true);
    setError(null);
    try {
      const data = await productsApi.listProducts();
      setProducts(data);
    } catch {
      setError('Não foi possível carregar produtos.');
    } finally {
      setLoadingProducts(false);
    }
  }, [canListProducts]);

  const loadDetail = useCallback(async (id: number) => {
    setLoadingDetail(true);
    setError(null);
    try {
      const d = await productsApi.getProduct(id);
      setDetail(d);
      setVariationId('');
    } catch {
      setError('Não foi possível carregar o produto.');
      setDetail(null);
    } finally {
      setLoadingDetail(false);
    }
  }, []);

  const loadMovements = useCallback(async () => {
    if (!canViewHistory) return;
    setLoadingMovements(true);
    setError(null);
    try {
      const vid = typeof variationId === 'number' ? variationId : undefined;
      const rows = await stockApi.listStockMovements({
        variationId: vid,
        take: 100,
      });
      setMovements(rows);
    } catch {
      setError('Não foi possível carregar o histórico de movimentações.');
    } finally {
      setLoadingMovements(false);
    }
  }, [canViewHistory, variationId]);

  useEffect(() => {
    void loadProducts();
  }, [loadProducts]);

  useEffect(() => {
    if (productId === '') {
      setDetail(null);
      setVariationId('');
      return;
    }
    void loadDetail(productId);
  }, [productId, loadDetail]);

  useEffect(() => {
    if (!canViewHistory) return;
    void loadMovements();
  }, [canViewHistory, loadMovements]);

  if (!canAdjust && !canViewHistory) {
    return (
      <div className="pdv-main__header">
        <h1>Estoque</h1>
        <p className="pdv-error">Você não tem permissão para acessar esta área.</p>
      </div>
    );
  }

  const onSubmit = form.handleSubmit(async (vals) => {
    if (!canAdjust) return;
    if (variationId === '') {
      setError('Selecione uma variação.');
      return;
    }
    setSaving(true);
    setNotice(null);
    setError(null);
    try {
      const reasonTrim = vals.reason?.trim() ?? '';
      await stockApi.adjustStock({
        productVariationId: variationId,
        quantity: vals.quantity,
        reason: reasonTrim.length > 0 ? reasonTrim : null,
      });
      setNotice('Entrada registrada com sucesso.');
      form.reset({ quantity: 1, reason: '' });
      if (typeof productId === 'number') {
        await loadDetail(productId);
      }
      await loadMovements();
    } catch {
      setError('Não foi possível registrar a entrada. Verifique os dados e tente novamente.');
    } finally {
      setSaving(false);
    }
  });

  return (
    <>
      <div className="pdv-main__header">
        <div>
          <h1>Movimentação de estoque</h1>
          <p style={{ margin: '0.25rem 0 0', color: 'var(--pdv-muted, #45474c)', fontSize: '0.9rem' }}>
            Ajuste de estoque e histórico (Fase 3)
          </p>
        </div>
      </div>

      {notice && <p className="pdv-empty" style={{ color: 'var(--pdv-secondary, #0058be)' }}>{notice}</p>}
      {error && <p className="pdv-error">{error}</p>}

      {!canListProducts && (
        <p className="pdv-error">
          É necessária a permissão de visualização de produtos para selecionar produto/variação aqui.
        </p>
      )}

      {canAdjust && (
        <div className="pdv-card" style={{ marginBottom: '1rem' }}>
          <h2 style={{ marginTop: 0, marginBottom: '1rem', fontSize: '1.05rem', fontWeight: 600 }}>
            Registrar entrada de estoque
          </h2>
          <form onSubmit={onSubmit}>
            <div className="pdv-field">
              <label htmlFor="stock-product">Produto</label>
              <select
                id="stock-product"
                value={productId === '' ? '' : String(productId)}
                disabled={!canListProducts || loadingProducts}
                onChange={(e) => {
                  const v = e.target.value;
                  setProductId(v === '' ? '' : Number(v));
                  setNotice(null);
                }}
              >
                <option value="">Selecione…</option>
                {products.map((p) => (
                  <option key={p.id} value={p.id}>
                    {p.name}
                  </option>
                ))}
              </select>
            </div>

            <div className="pdv-field">
              <label htmlFor="stock-variation">Variação</label>
              <select
                id="stock-variation"
                value={variationId === '' ? '' : String(variationId)}
                disabled={detail === null || loadingDetail || (detail?.variations.length ?? 0) === 0}
                onChange={(e) => {
                  const v = e.target.value;
                  setVariationId(v === '' ? '' : Number(v));
                  setNotice(null);
                }}
              >
                <option value="">Selecione…</option>
                {detail?.variations.map((v) => (
                  <option key={v.id} value={v.id}>
                    {v.name} — estoque: {v.stockQuantity}
                  </option>
                ))}
              </select>
            </div>

            <div className="pdv-field">
              <label htmlFor="stock-qty">Quantidade</label>
              <input
                id="stock-qty"
                type="number"
                min={1}
                step={1}
                disabled={!canAdjust}
                {...form.register('quantity', { valueAsNumber: true, min: 1 })}
              />
            </div>

            <div className="pdv-field">
              <label htmlFor="stock-reason">Motivo / observação (opcional)</label>
              <input id="stock-reason" type="text" maxLength={512} {...form.register('reason')} />
            </div>

            <div>
              <button type="submit" className="pdv-btn pdv-btn--primary" disabled={saving || !canListProducts}>
                {saving ? 'Salvando…' : 'Registrar entrada'}
              </button>
            </div>
          </form>
        </div>
      )}

      {canViewHistory && (
        <div className="pdv-card pdv-table-wrap">
          <h2 style={{ marginTop: 0, marginBottom: '1rem', fontSize: '1.05rem', fontWeight: 600 }}>
            Histórico de movimentações
          </h2>
          {loadingMovements ? (
            <p className="pdv-empty">Carregando…</p>
          ) : movements.length === 0 ? (
            <p className="pdv-empty">Nenhuma movimentação registrada ainda.</p>
          ) : (
            <table className="pdv-table">
              <thead>
                <tr>
                  <th>Data/hora</th>
                  <th>Produto</th>
                  <th>Variação</th>
                  <th>Tipo</th>
                  <th className="num">Qtd</th>
                  <th>Motivo</th>
                </tr>
              </thead>
              <tbody>
                {movements.map((m) => (
                  <tr key={m.id}>
                    <td>{new Date(m.createdAtUtc).toLocaleString('pt-BR')}</td>
                    <td>{m.productName}</td>
                    <td>{m.variationName}</td>
                    <td>
                      <span className={`pdv-chip ${m.type === 'IN' ? 'pdv-chip--ok' : 'pdv-chip--danger'}`}>
                        {m.type}
                      </span>
                    </td>
                    <td className="num">{m.quantity}</td>
                    <td>{m.reason ?? '—'}</td>
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
