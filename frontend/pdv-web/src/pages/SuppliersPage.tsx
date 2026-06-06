/**
 * @file SuppliersPage.tsx
 * @description Renders the suppliers management interface (list, create, edit, delete).
 */

import { useCallback, useEffect, useState } from 'react';
import { useForm } from 'react-hook-form';
import { can } from '../hooks/usePermission';
import { PERMISSIONS } from '../constants/permissions';
import * as suppliersApi from '../services/suppliers';
import type { SupplierSummaryDto } from '../types/suppliers';

type SupplierForm = {
  name: string;
  document: string;
  email: string;
  phone: string;
  isActive: boolean;
};

/**
 * SuppliersPage component for administering suppliers.
 */
export function SuppliersPage() {
  const [rows, setRows] = useState<SupplierSummaryDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [modal, setModal] = useState<{ mode: 'create' | 'edit'; supplier?: SupplierSummaryDto } | null>(null);
  const [deleting, setDeleting] = useState<SupplierSummaryDto | null>(null);

  const load = useCallback(async () => {
    setError(null);
    setLoading(true);
    try {
      const data = await suppliersApi.listSuppliers();
      setRows(data);
    } catch {
      setError('Não foi possível carregar os fornecedores.');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    void load();
  }, [load]);

  const canCreate = can(PERMISSIONS.supplierCreate);
  const canUpdate = can(PERMISSIONS.supplierUpdate);
  const canDelete = can(PERMISSIONS.supplierDelete);

  return (
    <>
      <div className="pdv-main__header">
        <h1>Fornecedores</h1>
        <div className="pdv-toolbar">
          {canCreate && (
            <button type="button" className="pdv-btn pdv-btn--primary" onClick={() => setModal({ mode: 'create' })}>
              Novo fornecedor
            </button>
          )}
        </div>
      </div>

      {error && <p className="pdv-error">{error}</p>}

      <div className="pdv-card pdv-table-wrap">
        {loading ? (
          <p className="pdv-empty">Carregando…</p>
        ) : rows.length === 0 ? (
          <p className="pdv-empty">Nenhum fornecedor cadastrado.</p>
        ) : (
          <table className="pdv-table">
            <thead>
              <tr>
                <th>Nome</th>
                <th>Documento</th>
                <th>E-mail</th>
                <th>Telefone</th>
                <th>Ativo</th>
                <th style={{ width: '10rem' }}>Ações</th>
              </tr>
            </thead>
            <tbody>
              {rows.map((s) => (
                <tr key={s.id}>
                  <td>{s.name}</td>
                  <td>{s.document ?? '—'}</td>
                  <td>{s.email ?? '—'}</td>
                  <td>{s.phone ?? '—'}</td>
                  <td>
                    <span className={`pdv-chip ${s.isActive ? 'pdv-chip--ok' : 'pdv-chip--off'}`}>
                      {s.isActive ? 'Sim' : 'Não'}
                    </span>
                  </td>
                  <td>
                    {canUpdate && (
                      <button
                        type="button"
                        className="pdv-btn pdv-btn--ghost pdv-btn--sm"
                        style={{ marginRight: 6 }}
                        onClick={() => setModal({ mode: 'edit', supplier: s })}
                      >
                        Editar
                      </button>
                    )}
                    {canDelete && (
                      <button type="button" className="pdv-btn pdv-btn--danger pdv-btn--sm" onClick={() => setDeleting(s)}>
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
        <SupplierModal
          mode={modal.mode}
          initial={modal.supplier}
          onClose={() => setModal(null)}
          onSaved={async () => {
            setModal(null);
            await load();
          }}
        />
      )}

      {deleting && (
        <ConfirmDelete
          title="Excluir fornecedor"
          message={`Excluir o fornecedor «${deleting.name}»?`}
          onCancel={() => setDeleting(null)}
          onConfirm={async () => {
            try {
              await suppliersApi.deleteSupplier(deleting.id);
              setDeleting(null);
              await load();
            } catch {
              setError('Falha ao excluir fornecedor.');
              setDeleting(null);
            }
          }}
        />
      )}
    </>
  );
}

/**
 * SupplierModal renders the Create/Edit form inside a dialog.
 */
function SupplierModal({
  mode,
  initial,
  onClose,
  onSaved,
}: {
  mode: 'create' | 'edit';
  initial?: SupplierSummaryDto;
  onClose: () => void;
  onSaved: () => Promise<void>;
}) {
  const [err, setErr] = useState<string | null>(null);
  const { register, handleSubmit, reset } = useForm<SupplierForm>({
    defaultValues: {
      name: initial?.name ?? '',
      document: initial?.document ?? '',
      email: initial?.email ?? '',
      phone: initial?.phone ?? '',
      isActive: initial?.isActive ?? true,
    },
  });

  useEffect(() => {
    reset({
      name: initial?.name ?? '',
      document: initial?.document ?? '',
      email: initial?.email ?? '',
      phone: initial?.phone ?? '',
      isActive: initial?.isActive ?? true,
    });
  }, [initial, reset]);

  const submit = handleSubmit(async (values) => {
    setErr(null);
    try {
      const payload = {
        name: values.name,
        document: values.document.trim() || null,
        email: values.email.trim() || null,
        phone: values.phone.trim() || null,
        isActive: values.isActive,
      };

      if (mode === 'create') {
        await suppliersApi.createSupplier(payload);
      } else if (initial) {
        await suppliersApi.updateSupplier(initial.id, payload);
      }
      await onSaved();
    } catch (e: unknown) {
      const ax = e as { response?: { data?: { detail?: string; errors?: Record<string, string[]> } } };
      if (ax.response?.data?.detail) {
        setErr(ax.response.data.detail);
      } else if (ax.response?.data?.errors) {
        const firstErr = Object.values(ax.response.data.errors)[0]?.[0];
        setErr(firstErr ?? 'Erro ao salvar fornecedor.');
      } else {
        setErr('Não foi possível salvar.');
      }
    }
  });

  return (
    <div className="pdv-modal-overlay" role="dialog" aria-modal="true" aria-labelledby="supplier-modal-title">
      <div className="pdv-modal">
        <h2 id="supplier-modal-title">{mode === 'create' ? 'Novo fornecedor' : 'Editar fornecedor'}</h2>
        <form onSubmit={submit}>
          <div className="pdv-field">
            <label htmlFor="s-name">Nome / Razão Social</label>
            <input id="s-name" type="text" autoComplete="off" {...register('name', { required: true })} />
          </div>
          <div className="pdv-field">
            <label htmlFor="s-document">CNPJ / CPF</label>
            <input id="s-document" type="text" autoComplete="off" {...register('document')} />
          </div>
          <div className="pdv-field">
            <label htmlFor="s-email">E-mail</label>
            <input id="s-email" type="email" autoComplete="off" {...register('email')} />
          </div>
          <div className="pdv-field">
            <label htmlFor="s-phone">Telefone</label>
            <input id="s-phone" type="text" autoComplete="off" {...register('phone')} />
          </div>
          <div className="pdv-field pdv-toggle">
            <input id="s-active" type="checkbox" {...register('isActive')} />
            <label htmlFor="s-active">Ativo</label>
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

/**
 * ConfirmDelete displays confirmation modal to delete item.
 */
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
