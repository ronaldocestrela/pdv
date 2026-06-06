import { useCallback, useEffect, useMemo, useRef, useState } from 'react';
import { can } from '../hooks/usePermission';
import { PERMISSIONS } from '../constants/permissions';
import * as productsApi from '../services/products';
import * as salesApi from '../services/sales';
import type { ProductDetailDto, ProductSummaryDto } from '../types/products';
import type { PaymentMethodDto, SaleListItemDto } from '../types/sales';
import { getApiErrorMessage } from '../utils/apiError';

type CartLine = {
  productVariationId: string;
  productName: string;
  variationName: string;
  quantity: number;
  unitPrice: number;
  stockQuantity: number;
};

function formatBRL(value: number): string {
  return new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(value);
}

/**
 * TODO: Describe PdvPage.
 */
export function PdvPage() {
  const canFinalize = can(PERMISSIONS.saleCreate);
  const canViewSalesList = can(PERMISSIONS.saleView);
  const canCatalog = can(PERMISSIONS.productView);
  const searchInputRef = useRef<HTMLInputElement>(null);

  const [products, setProducts] = useState<ProductSummaryDto[]>([]);
  const [search, setSearch] = useState('');
  const [selectedProductId, setSelectedProductId] = useState<string | ''>('');
  const [detail, setDetail] = useState<ProductDetailDto | null>(null);
  const [variationId, setVariationId] = useState<string | ''>('');
  const [qtyToAdd, setQtyToAdd] = useState(1);
  const [cart, setCart] = useState<CartLine[]>([]);
  const [paymentMethod, setPaymentMethod] = useState<PaymentMethodDto>('cash');
  const [recentSales, setRecentSales] = useState<SaleListItemDto[]>([]);

  const [loadingProducts, setLoadingProducts] = useState(false);
  const [loadingDetail, setLoadingDetail] = useState(false);
  const [loadingSales, setLoadingSales] = useState(false);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [notice, setNotice] = useState<string | null>(null);

  const filteredProducts = useMemo(() => {
    const q = search.trim().toLowerCase();
    if (!q) return products;
    return products.filter((p) => p.name.toLowerCase().includes(q));
  }, [products, search]);

  const cartTotal = useMemo(
    () => cart.reduce((acc, line) => acc + line.quantity * line.unitPrice, 0),
    [cart],
  );

  const loadProducts = useCallback(async () => {
    setLoadingProducts(true);
    setError(null);
    try {
      const data = await productsApi.listProducts();
      setProducts(data);
    } catch {
      setError('Não foi possível carregar o catálogo.');
    } finally {
      setLoadingProducts(false);
    }
  }, []);

  const loadDetail = useCallback(async (id: string) => {
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

  const loadRecentSales = useCallback(async () => {
    if (!canViewSalesList) return;
    setLoadingSales(true);
    try {
      const rows = await salesApi.listSales(15);
      setRecentSales(rows);
    } catch {
      /* silent — secondary panel */
    } finally {
      setLoadingSales(false);
    }
  }, [canViewSalesList]);

  useEffect(() => {
    if (!canCatalog) return;
    void loadProducts();
  }, [canCatalog, loadProducts]);

  useEffect(() => {
    void loadRecentSales();
  }, [loadRecentSales]);

  useEffect(() => {
    if (selectedProductId === '') {
      setDetail(null);
      setVariationId('');
      return;
    }
    void loadDetail(selectedProductId);
  }, [selectedProductId, loadDetail]);

  const finalizeSale = useCallback(async () => {
    if (!canFinalize) return;
    if (cart.length === 0) {
      setError('Adicione itens ao carrinho.');
      return;
    }
    setSubmitting(true);
    setError(null);
    setNotice(null);
    try {
      const result = await salesApi.createSale({
        items: cart.map((l) => ({
          productVariationId: l.productVariationId,
          quantity: l.quantity,
        })),
        paymentMethod,
      });
      setNotice(`Venda #${result.saleId} registrada — total ${formatBRL(result.totalAmount)}.`);
      setCart([]);
      await loadProducts();
      if (typeof selectedProductId === 'string' && selectedProductId !== '') {
        await loadDetail(selectedProductId);
      }
      await loadRecentSales();
    } catch (err) {
      setError(
        getApiErrorMessage(
          err,
          'Não foi possível finalizar a venda. Verifique estoque e tente novamente.',
        ),
      );
    } finally {
      setSubmitting(false);
    }
  }, [
    canFinalize,
    cart,
    paymentMethod,
    loadProducts,
    selectedProductId,
    loadDetail,
    loadRecentSales,
  ]);

  useEffect(() => {
    const onKey = (e: KeyboardEvent) => {
      if ((e.ctrlKey || e.metaKey) && e.key === 'Enter') {
        if (!canFinalize || cart.length === 0 || submitting) return;
        e.preventDefault();
        void finalizeSale();
        return;
      }
      if (e.key === 'Escape') {
        setSearch((s) => (s ? '' : s));
        return;
      }
      if (e.key === '/' && !e.ctrlKey && !e.metaKey && !e.altKey) {
        const t = e.target as HTMLElement | null;
        const tag = t?.tagName;
        if (tag === 'INPUT' || tag === 'SELECT' || tag === 'TEXTAREA') return;
        e.preventDefault();
        searchInputRef.current?.focus();
      }
    };
    window.addEventListener('keydown', onKey);
    return () => window.removeEventListener('keydown', onKey);
  }, [canFinalize, cart.length, submitting, finalizeSale]);

  const paymentLabel = (p: PaymentMethodDto) =>
    p === 'cash' ? 'Dinheiro' : p === 'card' ? 'Cartão' : 'PIX';

  const addToCart = () => {
    if (!canFinalize) return;
    if (variationId === '' || detail === null) {
      setError('Selecione uma variação.');
      return;
    }
    if (qtyToAdd < 1) {
      setError('Quantidade deve ser pelo menos 1.');
      return;
    }

    const v = detail.variations.find((x) => x.id === variationId);
    if (!v) {
      setError('Variação não encontrada.');
      return;
    }
    const price = v.unitPrice;
    if (Number.isNaN(price) || price < 0) {
      setError('Preço da variação inválido — cadastre o preço nas variações do produto.');
      return;
    }
    if (v.stockQuantity < qtyToAdd) {
      setError(`Estoque insuficiente para "${v.name}" (disponível: ${v.stockQuantity}).`);
      return;
    }

    setError(null);
    setNotice(null);

    setCart((prev) => {
      const idx = prev.findIndex((l) => l.productVariationId === variationId);
      if (idx === -1) {
        return [
          ...prev,
          {
            productVariationId: variationId,
            productName: detail.name,
            variationName: v.name,
            quantity: qtyToAdd,
            unitPrice: price,
            stockQuantity: v.stockQuantity,
          },
        ];
      }
      const row = prev[idx];
      const nextQty = row.quantity + qtyToAdd;
      if (v.stockQuantity < nextQty) {
        setError(`Estoque insuficiente para "${v.name}" (disponível: ${v.stockQuantity}).`);
        return prev;
      }
      const next = [...prev];
      next[idx] = { ...row, quantity: nextQty, stockQuantity: v.stockQuantity, unitPrice: price };
      return next;
    });

    setQtyToAdd(1);
  };

  const updateQty = (variationKey: string, delta: number) => {
    if (!canFinalize) return;
    const line = cart.find((l) => l.productVariationId === variationKey);
    if (!line) return;
    const nextQty = line.quantity + delta;
    if (nextQty <= 0) {
      setError(null);
      setCart((prev) => prev.filter((l) => l.productVariationId !== variationKey));
      return;
    }
    if (nextQty > line.stockQuantity) {
      setError(
        `Estoque insuficiente para "${line.variationName}" (disponível: ${line.stockQuantity}).`,
      );
      return;
    }
    setError(null);
    setCart((prev) =>
      prev.map((l) => (l.productVariationId === variationKey ? { ...l, quantity: nextQty } : l)),
    );
  };

  const removeLine = (variationKey: string) => {
    if (!canFinalize) return;
    setCart((prev) => prev.filter((l) => l.productVariationId !== variationKey));
  };

  const clearCart = () => {
    if (!canFinalize) return;
    setCart([]);
    setNotice(null);
    setError(null);
  };

  if (!canFinalize && !canViewSalesList) {
    return (
      <div className="pdv-main__header">
        <h1>PDV</h1>
        <p className="pdv-error">Você não tem permissão para acessar o PDV.</p>
      </div>
    );
  }

  if (!canCatalog) {
    return (
      <div className="pdv-main__header">
        <h1>PDV</h1>
        <p className="pdv-error">É necessária a permissão de visualização de produtos para usar o catálogo no PDV.</p>
      </div>
    );
  }

  const selectedVariation =
    variationId !== '' && detail ? detail.variations.find((x) => x.id === variationId) : undefined;

  return (
    <>
      <div className="pdv-main__header">
        <div>
          <h1>PDV — Checkout</h1>
          <p style={{ margin: '0.25rem 0 0', color: 'var(--pdv-muted, #45474c)', fontSize: '0.9rem' }}>
            Busca, carrinho e finalização. Atalhos: <kbd>/</kbd> foca a busca ·{' '}
            <kbd>Ctrl</kbd>+<kbd>Enter</kbd> finaliza · <kbd>Esc</kbd> limpa a busca. Referência Stitch:{' '}
            <code style={{ fontSize: '0.85rem' }}>docs/design/stitch-phase4-pdv-ui.md</code>
          </p>
        </div>
      </div>

      <div role="status" aria-live="polite" aria-atomic="true">
        {notice && (
          <p className="pdv-empty" style={{ color: 'var(--pdv-secondary, #0058be)' }}>
            {notice}
          </p>
        )}
        {error && <p className="pdv-error">{error}</p>}
      </div>

      <div className="pdv-pdv-layout">
        <div className="pdv-card" style={{ marginBottom: 0 }}>
          <h2 style={{ marginTop: 0, marginBottom: '1rem', fontSize: '1.05rem', fontWeight: 600 }}>
            Produtos
          </h2>
          <div className="pdv-field">
            <label htmlFor="pdv-search">Buscar</label>
            <input
              ref={searchInputRef}
              id="pdv-search"
              type="search"
              placeholder="Nome do produto…"
              autoFocus
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              disabled={loadingProducts}
            />
          </div>

          <div className="pdv-field">
            <label htmlFor="pdv-product">Produto</label>
            <select
              id="pdv-product"
              value={selectedProductId === '' ? '' : String(selectedProductId)}
              disabled={loadingProducts}
              onChange={(e) => {
                const v = e.target.value;
                setSelectedProductId(v);
              }}
            >
              <option value="">Selecione…</option>
              {filteredProducts.map((p) => (
                <option key={p.id} value={p.id}>
                  {p.name}
                </option>
              ))}
            </select>
          </div>

          <div className="pdv-field">
            <label htmlFor="pdv-variation">Variação</label>
            <select
              id="pdv-variation"
              value={variationId === '' ? '' : String(variationId)}
              disabled={detail === null || loadingDetail || (detail?.variations.length ?? 0) === 0}
              onChange={(e) => {
                const v = e.target.value;
                setVariationId(v);
              }}
            >
              <option value="">Selecione…</option>
              {detail?.variations.map((v) => (
                <option key={v.id} value={v.id}>
                  {v.name} — {formatBRL(v.unitPrice)} — estoque: {v.stockQuantity}
                </option>
              ))}
            </select>
          </div>

          <div className="pdv-subgrid-price-qty">
            <div className="pdv-field" style={{ marginBottom: 0 }}>
              <span id="pdv-price-label">Preço unitário (catálogo)</span>
              <div
                id="pdv-price-display"
                role="status"
                aria-labelledby="pdv-price-label"
                style={{
                  padding: '0.5rem 0.65rem',
                  minHeight: '2.25rem',
                  display: 'flex',
                  alignItems: 'center',
                  fontWeight: 600,
                  border: '1px solid var(--pdv-border, #dadce0)',
                  borderRadius: 6,
                  background: 'var(--pdv-surface-muted, #f8f9fa)',
                  color: selectedVariation ? undefined : 'var(--pdv-muted)',
                }}
              >
                {selectedVariation ? formatBRL(selectedVariation.unitPrice) : '—'}
              </div>
            </div>
            <div className="pdv-field" style={{ marginBottom: 0 }}>
              <label htmlFor="pdv-qty-add">Qtd.</label>
              <input
                id="pdv-qty-add"
                type="number"
                min={1}
                step={1}
                disabled={!canFinalize}
                value={qtyToAdd}
                onChange={(e) => setQtyToAdd(Number(e.target.value))}
              />
            </div>
          </div>

          {selectedVariation && (
            <p style={{ margin: '0.75rem 0 0', fontSize: '0.875rem', color: 'var(--pdv-muted)' }}>
              Estoque disponível: <strong>{selectedVariation.stockQuantity}</strong>
            </p>
          )}

          <div style={{ marginTop: '1rem' }}>
            <button type="button" className="pdv-btn pdv-btn--primary" disabled={!canFinalize} onClick={addToCart}>
              Adicionar ao carrinho
            </button>
          </div>

          {filteredProducts.length > 0 && (
            <div className="pdv-table-wrap" style={{ marginTop: '1.25rem' }}>
              <p style={{ margin: '0 0 0.5rem', fontSize: '0.875rem', fontWeight: 600 }}>Resultados da busca</p>
              <table className="pdv-table">
                <thead>
                  <tr>
                    <th>Produto</th>
                    <th>Variações</th>
                  </tr>
                </thead>
                <tbody>
                  {filteredProducts.slice(0, 25).map((p) => (
                    <tr key={p.id}>
                      <td>
                        <button
                          type="button"
                          className="pdv-btn pdv-btn--ghost"
                          style={{ padding: '0.25rem 0.5rem', fontWeight: 600 }}
                          onClick={() => setSelectedProductId(p.id)}
                        >
                          {p.name}
                        </button>
                      </td>
                      <td>{p.variationCount}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
              {filteredProducts.length > 25 && (
                <p className="pdv-empty" style={{ marginTop: '0.5rem' }}>
                  Mostrando 25 de {filteredProducts.length} resultados — refine a busca.
                </p>
              )}
            </div>
          )}
        </div>

        <div>
          <div className="pdv-card" style={{ marginBottom: '1rem' }}>
            <h2 style={{ marginTop: 0, marginBottom: '1rem', fontSize: '1.05rem', fontWeight: 600 }}>
              Carrinho
            </h2>
            {cart.length === 0 ? (
              <p className="pdv-empty">Carrinho vazio.</p>
            ) : (
              <div className="pdv-table-wrap">
                <table className="pdv-table">
                  <thead>
                    <tr>
                      <th>Item</th>
                      <th className="num">Un.</th>
                      <th className="num">Qtd</th>
                      <th className="num">Subtotal</th>
                      <th />
                    </tr>
                  </thead>
                  <tbody>
                    {cart.map((line) => (
                      <tr key={line.productVariationId}>
                        <td>
                          <div style={{ fontWeight: 600 }}>{line.productName}</div>
                          <div style={{ fontSize: '0.8rem', color: 'var(--pdv-muted)' }}>{line.variationName}</div>
                        </td>
                        <td className="num">{formatBRL(line.unitPrice)}</td>
                        <td className="num">
                          <div style={{ display: 'inline-flex', alignItems: 'center', gap: 6 }}>
                            <button
                              type="button"
                              className="pdv-btn pdv-btn--ghost"
                              style={{ padding: '0.15rem 0.45rem', minWidth: 0 }}
                              disabled={!canFinalize}
                              onClick={() => updateQty(line.productVariationId, -1)}
                            >
                              −
                            </button>
                            <span>{line.quantity}</span>
                            <button
                              type="button"
                              className="pdv-btn pdv-btn--ghost"
                              style={{ padding: '0.15rem 0.45rem', minWidth: 0 }}
                              disabled={!canFinalize}
                              onClick={() => updateQty(line.productVariationId, 1)}
                            >
                              +
                            </button>
                          </div>
                        </td>
                        <td className="num">{formatBRL(line.quantity * line.unitPrice)}</td>
                        <td>
                          <button
                            type="button"
                            className="pdv-btn pdv-btn--danger"
                            style={{ padding: '0.25rem 0.5rem' }}
                            disabled={!canFinalize}
                            onClick={() => removeLine(line.productVariationId)}
                          >
                            Remover
                          </button>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}

            <div style={{ marginTop: '1rem', display: 'flex', justifyContent: 'space-between', alignItems: 'baseline' }}>
              <span style={{ fontWeight: 600 }}>Total</span>
              <span style={{ fontSize: '1.35rem', fontWeight: 700 }}>{formatBRL(cartTotal)}</span>
            </div>

            <div style={{ marginTop: '1rem' }}>
              <p style={{ margin: '0 0 0.5rem', fontSize: '0.8rem', fontWeight: 600, color: 'var(--pdv-muted)' }}>
                Pagamento
              </p>
              <div style={{ display: 'flex', gap: 8, flexWrap: 'wrap' }}>
                {(['cash', 'card', 'pix'] as const).map((p) => (
                  <button
                    key={p}
                    type="button"
                    className={`pdv-btn ${paymentMethod === p ? 'pdv-btn--primary' : 'pdv-btn--ghost'}`}
                    disabled={!canFinalize}
                    onClick={() => setPaymentMethod(p)}
                  >
                    {paymentLabel(p)}
                  </button>
                ))}
              </div>
            </div>

            <div style={{ marginTop: '1rem', display: 'flex', flexDirection: 'column', gap: 8 }}>
              <button
                type="button"
                className="pdv-btn pdv-btn--primary"
                style={{ width: '100%' }}
                disabled={!canFinalize || submitting || cart.length === 0}
                onClick={() => void finalizeSale()}
              >
                {submitting ? 'Finalizando…' : 'Finalizar venda'}
              </button>
              <button
                type="button"
                className="pdv-btn pdv-btn--ghost"
                style={{ width: '100%', border: '1px solid var(--pdv-border)' }}
                disabled={!canFinalize || cart.length === 0}
                onClick={clearCart}
              >
                Limpar carrinho
              </button>
            </div>
          </div>

          {canViewSalesList && (
            <div className="pdv-card pdv-table-wrap">
              <h2 style={{ marginTop: 0, marginBottom: '1rem', fontSize: '1.05rem', fontWeight: 600 }}>
                Últimas vendas
              </h2>
              {loadingSales ? (
                <p className="pdv-empty">Carregando…</p>
              ) : recentSales.length === 0 ? (
                <p className="pdv-empty">Nenhuma venda registrada.</p>
              ) : (
                <table className="pdv-table">
                  <thead>
                    <tr>
                      <th>Data</th>
                      <th className="num">Itens</th>
                      <th className="num">Total</th>
                      <th>Pagamento</th>
                    </tr>
                  </thead>
                  <tbody>
                    {recentSales.map((s) => (
                      <tr key={s.id}>
                        <td>{new Date(s.createdAtUtc).toLocaleString('pt-BR')}</td>
                        <td className="num">{s.itemCount}</td>
                        <td className="num">{formatBRL(s.totalAmount)}</td>
                        <td>{paymentLabel(s.paymentMethod)}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              )}
            </div>
          )}
        </div>
      </div>
    </>
  );
}
