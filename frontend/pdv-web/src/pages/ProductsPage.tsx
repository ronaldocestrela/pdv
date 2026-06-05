import { useCallback, useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { can } from '../hooks/usePermission';
import { PERMISSIONS } from '../constants/permissions';
import * as productsApi from '../services/products';
import type { ProductSummaryDto } from '../types/products';

type ProductForm = { name: string; isActive: boolean };

/**
 * TODO: Describe ProductsPage.
 */
export function ProductsPage() {
  const navigate = useNavigate();
  const [rows, setRows] = useState<ProductSummaryDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [modal, setModal] = useState<{ mode: 'create' | 'edit'; product?: ProductSummaryDto } | null>(null);
  const [deleting, setDeleting] = useState<ProductSummaryDto | null>(null);

  const load = useCallback(async () => {
    setError(null);
    setLoading(true);
    try {
      const data = await productsApi.listProducts();
      setRows(data);
    } catch {
      setError('Não foi possível carregar os produtos.');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    void load();
  }, [load]);

  const canCreate = can(PERMISSIONS.productCreate);
  const canUpdate = can(PERMISSIONS.productUpdate);
  const canDelete = can(PERMISSIONS.productDelete);
  const canSeeVariations = [
    PERMISSIONS.variationView,
    PERMISSIONS.variationCreate,
    PERMISSIONS.variationUpdate,
    PERMISSIONS.variationDelete,
  ].some((p) => can(p));

  return (
    <>
      <div className="pdv-main__header">
        <h1>Produtos</h1>
        <div className="pdv-toolbar">
          {canCreate && (
            <button type="button" className="pdv-btn pdv-btn--primary" onClick={() => setModal({ mode: 'create' })}>
              Novo produto
            </button>
          )}
        </div>
      </div>

      {error && <p className="pdv-error">{error}</p>}

      <div className="pdv-card pdv-table-wrap">
        {loading ? (
          <p className="pdv-empty">Carregando…</p>
        ) : rows.length === 0 ? (
          <p className="pdv-empty">Nenhum produto cadastrado.</p>
        ) : (
          <table className="pdv-table">
            <thead>
              <tr>
                <th>Nome</th>
                <th>Ativo</th>
                <th style={{ width: '8rem' }}>Variações</th>
                <th style={{ width: '14rem' }}>Ações</th>
              </tr>
            </thead>
            <tbody>
              {rows.map((p) => (
                <tr key={p.id}>
                  <td>{p.name}</td>
                  <td>
                    <span className={`pdv-chip ${p.isActive ? 'pdv-chip--ok' : 'pdv-chip--off'}`}>
                      {p.isActive ? 'Sim' : 'Não'}
                    </span>
                  </td>
                  <td className="num">{p.variationCount}</td>
                  <td>
                    {canSeeVariations && (
                      <button
                        type="button"
                        className="pdv-btn pdv-btn--ghost pdv-btn--sm"
                        style={{ marginRight: 6 }}
                        onClick={() => navigate(`/products/${p.id}/variations`)}
                      >
                        Variações
                      </button>
                    )}
                    {canUpdate && (
                      <button
                        type="button"
                        className="pdv-btn pdv-btn--ghost pdv-btn--sm"
                        style={{ marginRight: 6 }}
                        onClick={() => setModal({ mode: 'edit', product: p })}
                      >
                        Editar
                      </button>
                    )}
                    {canDelete && (
                      <button type="button" className="pdv-btn pdv-btn--danger pdv-btn--sm" onClick={() => setDeleting(p)}>
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
        <ProductModal
          mode={modal.mode}
          initial={modal.product}
          onClose={() => setModal(null)}
          onSaved={async () => {
            setModal(null);
            await load();
          }}
        />
      )}

      {deleting && (
        <ConfirmDelete
          title="Excluir produto"
          message={`Excluir «${deleting.name}» e todas as variações?`}
          onCancel={() => setDeleting(null)}
          onConfirm={async () => {
            try {
              await productsApi.deleteProduct(deleting.id);
              setDeleting(null);
              await load();
            } catch {
              setError('Falha ao excluir produto.');
              setDeleting(null);
            }
          }}
        />
      )}
    </>
  );
}

function ProductModal({
  mode,
  initial,
  onClose,
  onSaved,
}: {
  mode: 'create' | 'edit';
  initial?: ProductSummaryDto;
  onClose: () => void;
  onSaved: () => Promise<void>;
}) {
  const [err, setErr] = useState<string | null>(null);
  const { register, handleSubmit, reset } = useForm<ProductForm>({
    defaultValues: {
      name: initial?.name ?? '',
      isActive: initial?.isActive ?? true,
    },
  });

  useEffect(() => {
    reset({
      name: initial?.name ?? '',
      isActive: initial?.isActive ?? true,
    });
  }, [initial, reset]);

  const submit = handleSubmit(async (values) => {
    setErr(null);
    try {
      if (mode === 'create') {
        await productsApi.createProduct({ name: values.name, isActive: values.isActive });
      } else if (initial) {
        await productsApi.updateProduct(initial.id, { name: values.name, isActive: values.isActive });
      }
      await onSaved();
    } catch {
      setErr('Não foi possível salvar.');
    }
  });

  return (
    <div className="pdv-modal-overlay" role="dialog" aria-modal="true" aria-labelledby="product-modal-title">
      <div className="pdv-modal">
        <h2 id="product-modal-title">{mode === 'create' ? 'Novo produto' : 'Editar produto'}</h2>
        <form onSubmit={submit}>
          <div className="pdv-field">
            <label htmlFor="p-name">Nome</label>
            <input id="p-name" type="text" autoComplete="off" {...register('name', { required: true })} />
          </div>
          <div className="pdv-field pdv-toggle">
            <input id="p-active" type="checkbox" {...register('isActive')} />
            <label htmlFor="p-active">Ativo</label>
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
        <p style={{ margin: 0, fontSize: '0.9rem' }}>{message}</p>
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
