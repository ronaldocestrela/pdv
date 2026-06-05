import { useCallback, useEffect, useState } from 'react';
import { Link, useNavigate, useParams } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { can } from '../hooks/usePermission';
import { PERMISSIONS } from '../constants/permissions';
import * as productsApi from '../services/products';
import type { ProductDetailDto, ProductVariationDto } from '../types/products';

type VariationForm = { name: string; barcode: string; stockQuantity: number; unitPrice: number };

function formatBRL(value: number): string {
  return new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(value);
}

function hasAnyVariationPermission(): boolean {
  return [
    PERMISSIONS.variationView,
    PERMISSIONS.variationCreate,
    PERMISSIONS.variationUpdate,
    PERMISSIONS.variationDelete,
  ].some((p) => can(p));
}

/**
 * TODO: Describe ProductVariationsPage.
 */
export function ProductVariationsPage() {
  const { productId } = useParams<{ productId: string }>();
  const id = Number(productId);
  const navigate = useNavigate();
  const [detail, setDetail] = useState<ProductDetailDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [modal, setModal] = useState<{ mode: 'create' | 'edit'; variation?: ProductVariationDto } | null>(null);
  const [deleting, setDeleting] = useState<ProductVariationDto | null>(null);

  const load = useCallback(async () => {
    if (!Number.isFinite(id) || id <= 0) {
      setError('Produto inválido.');
      setLoading(false);
      return;
    }
    setError(null);
    setLoading(true);
    try {
      const d = await productsApi.getProduct(id);
      setDetail(d);
      if (!d) {
        setError('Produto não encontrado.');
      }
    } catch {
      setError('Não foi possível carregar o produto.');
    } finally {
      setLoading(false);
    }
  }, [id]);

  useEffect(() => {
    if (!hasAnyVariationPermission()) {
      return;
    }
    void load();
  }, [load]);

  const canCreate = can(PERMISSIONS.variationCreate);
  const canUpdate = can(PERMISSIONS.variationUpdate);
  const canDelete = can(PERMISSIONS.variationDelete);

  if (!hasAnyVariationPermission()) {
    return (
      <p className="pdv-error">
        Sem permissão para variações.{' '}
        <Link to="/products">Voltar para produtos</Link>
      </p>
    );
  }

  if (loading) {
    return <p className="pdv-empty">Carregando…</p>;
  }

  if (!detail) {
    return (
      <>
        <p className="pdv-error">{error ?? 'Produto não encontrado.'}</p>
        <Link to="/products">Voltar para produtos</Link>
      </>
    );
  }

  return (
    <>
      <nav className="pdv-breadcrumb" aria-label="Breadcrumb">
        <Link to="/products">Produtos</Link>
        {' / '}
        <span>{detail.name}</span>
        {' / '}
        <span>Variações</span>
      </nav>
      <button type="button" className="pdv-btn pdv-btn--ghost pdv-btn--sm" style={{ marginBottom: 12 }} onClick={() => navigate('/products')}>
        ← Voltar
      </button>

      <div className="pdv-main__header">
        <h1>Variações</h1>
        <div className="pdv-toolbar">
          {canCreate && (
            <button type="button" className="pdv-btn pdv-btn--primary" onClick={() => setModal({ mode: 'create' })}>
              Nova variação
            </button>
          )}
        </div>
      </div>

      {error && <p className="pdv-error">{error}</p>}

      <div className="pdv-card pdv-table-wrap">
        {detail.variations.length === 0 ? (
          <p className="pdv-empty">Nenhuma variação cadastrada.</p>
        ) : (
          <table className="pdv-table">
            <thead>
              <tr>
                <th>Nome</th>
                <th>Código de barras</th>
                <th className="num" style={{ width: '8rem' }}>
                  Preço
                </th>
                <th style={{ width: '7rem' }}>Estoque</th>
                <th style={{ width: '10rem' }}>Ações</th>
              </tr>
            </thead>
            <tbody>
              {detail.variations.map((v) => (
                <tr key={v.id}>
                  <td>{v.name}</td>
                  <td>{v.barcode ?? '—'}</td>
                  <td className="num">{formatBRL(v.unitPrice)}</td>
                  <td className="num">{v.stockQuantity}</td>
                  <td>
                    {canUpdate && (
                      <button
                        type="button"
                        className="pdv-btn pdv-btn--ghost pdv-btn--sm"
                        style={{ marginRight: 6 }}
                        onClick={() => setModal({ mode: 'edit', variation: v })}
                      >
                        Editar
                      </button>
                    )}
                    {canDelete && (
                      <button type="button" className="pdv-btn pdv-btn--danger pdv-btn--sm" onClick={() => setDeleting(v)}>
                        Excluir
                      </button>
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>

      {modal && (
        <VariationModal
          productId={detail.id}
          mode={modal.mode}
          initial={modal.variation}
          onClose={() => setModal(null)}
          onSaved={async () => {
            setModal(null);
            await load();
          }}
        />
      )}

      {deleting && (
        <ConfirmDelete
          title="Excluir variação"
          message={`Excluir «${deleting.name}»?`}
          onCancel={() => setDeleting(null)}
          onConfirm={async () => {
            try {
              await productsApi.deleteVariation(deleting.id);
              setDeleting(null);
              await load();
            } catch {
              setError('Falha ao excluir variação.');
              setDeleting(null);
            }
          }}
        />
      )}
    </>
  );
}

function VariationModal({
  productId,
  mode,
  initial,
  onClose,
  onSaved,
}: {
  productId: number;
  mode: 'create' | 'edit';
  initial?: ProductVariationDto;
  onClose: () => void;
  onSaved: () => Promise<void>;
}) {
  const [err, setErr] = useState<string | null>(null);
  const { register, handleSubmit, reset } = useForm<VariationForm>({
    defaultValues: {
      name: initial?.name ?? '',
      barcode: initial?.barcode ?? '',
      stockQuantity: initial?.stockQuantity ?? 0,
      unitPrice: initial?.unitPrice ?? 0,
    },
  });

  useEffect(() => {
    reset({
      name: initial?.name ?? '',
      barcode: initial?.barcode ?? '',
      stockQuantity: initial?.stockQuantity ?? 0,
      unitPrice: initial?.unitPrice ?? 0,
    });
  }, [initial, reset]);

  const submit = handleSubmit(async (values) => {
    setErr(null);
    const barcodeTrim = values.barcode.trim();
    const barcode = barcodeTrim.length > 0 ? barcodeTrim : null;
    try {
      if (mode === 'create') {
        await productsApi.createVariation({
          productId,
          name: values.name,
          barcode,
          stockQuantity: values.stockQuantity,
          unitPrice: values.unitPrice,
        });
      } else if (initial) {
        await productsApi.updateVariation(initial.id, {
          name: values.name,
          barcode,
          stockQuantity: values.stockQuantity,
          unitPrice: values.unitPrice,
        });
      }
      await onSaved();
    } catch {
      setErr('Não foi possível salvar (verifique duplicidade de código de barras).');
    }
  });

  return (
    <div className="pdv-modal-overlay" role="dialog" aria-modal="true">
      <div className="pdv-modal">
        <h2>{mode === 'create' ? 'Nova variação' : 'Editar variação'}</h2>
        <form onSubmit={submit}>
          <div className="pdv-field">
            <label htmlFor="v-name">Nome</label>
            <input id="v-name" type="text" {...register('name', { required: true })} placeholder="ex: Verde GG" />
          </div>
          <div className="pdv-field">
            <label htmlFor="v-bc">Código de barras (opcional)</label>
            <input id="v-bc" type="text" {...register('barcode')} />
          </div>
          <div className="pdv-field">
            <label htmlFor="v-price">Preço unitário (R$)</label>
            <input
              id="v-price"
              type="number"
              min={0}
              step="0.01"
              {...register('unitPrice', { valueAsNumber: true, min: 0 })}
            />
          </div>
          <div className="pdv-field">
            <label htmlFor="v-stock">Estoque</label>
            <input
              id="v-stock"
              type="number"
              min={0}
              step={1}
              {...register('stockQuantity', { valueAsNumber: true, min: 0 })}
            />
          </div>
          {err && <p className="pdv-error">{err}</p>}
          <div className="pdv-modal__actions">
            <button type="button" className="pdv-btn pdv-btn--ghost" onClick={onClose}>
              Cancelar
            </button>
            <button type="submit" className="pdv-btn pdv-btn--primary">
              Salvar
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}

function ConfirmDelete({
  title,
  message,
  onCancel,
  onConfirm,
}: {
  title: string;
  message: string;
  onCancel: () => void;
  onConfirm: () => void;
}) {
  return (
    <div className="pdv-modal-overlay">
      <div className="pdv-modal">
        <h2>{title}</h2>
        <p style={{ margin: 0 }}>{message}</p>
        <div className="pdv-modal__actions">
          <button type="button" className="pdv-btn pdv-btn--ghost" onClick={onCancel}>
            Cancelar
          </button>
          <button type="button" className="pdv-btn pdv-btn--danger" onClick={onConfirm}>
            Excluir
          </button>
        </div>
      </div>
    </div>
  );
}
