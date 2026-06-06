import { useCallback, useEffect, useMemo, useState } from 'react';
import { can } from '../hooks/usePermission';
import { PERMISSIONS } from '../constants/permissions';
import * as rolesApi from '../services/roles';
import * as permissionsApi from '../services/permissions';
import type { RoleAdminDto } from '../types/roles';

const SUPER_ADMIN = 'Super Admin';

/**
 * TODO: Describe RolesPage.
 */
export function RolesPage() {
  const canRead = can(PERMISSIONS.roleManage) || can(PERMISSIONS.userManage);
  const canMutate = can(PERMISSIONS.roleManage);

  const [roles, setRoles] = useState<RoleAdminDto[]>([]);
  const [catalog, setCatalog] = useState<string[]>([]);
  const [selectedId, setSelectedId] = useState<string | null>(null);
  const [newName, setNewName] = useState('');
  const [editName, setEditName] = useState('');
  const [permSelections, setPermSelections] = useState<Record<string, boolean>>({});
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const selected = useMemo(() => roles.find((r) => r.id === selectedId) ?? null, [roles, selectedId]);
  const isSuperAdmin = selected?.name === SUPER_ADMIN;

  const loadAll = useCallback(async () => {
    if (!canRead) return;
    setLoading(true);
    setError(null);
    try {
      const [r, c] = await Promise.all([rolesApi.listRoles(), permissionsApi.getPermissionCatalog()]);
      setRoles(r);
      setCatalog(c);
    } catch {
      setError('Não foi possível carregar roles ou catálogo de permissões.');
    } finally {
      setLoading(false);
    }
  }, [canRead]);

  useEffect(() => {
    void loadAll();
  }, [loadAll]);

  useEffect(() => {
    if (selectedId === null && roles.length > 0) setSelectedId(roles[0].id);
  }, [roles, selectedId]);

  useEffect(() => {
    if (!selected) {
      setEditName('');
      setPermSelections({});
      return;
    }
    setEditName(selected.name);
    const next: Record<string, boolean> = {};
    for (const p of catalog) next[p] = selected.permissions.includes(p);
    setPermSelections(next);
  }, [selected, catalog]);

  async function handleCreate() {
    if (!canMutate || !newName.trim()) return;
    setError(null);
    try {
      await rolesApi.createRole(newName.trim());
      setNewName('');
      await loadAll();
    } catch {
      setError('Não foi possível criar a role.');
    }
  }

  async function handleSaveName() {
    if (!canMutate || !selected) return;
    setError(null);
    try {
      await rolesApi.updateRole(selected.id, editName.trim());
      await loadAll();
    } catch {
      setError('Não foi possível atualizar o nome da role.');
    }
  }

  async function handleDelete() {
    if (!canMutate || !selected || isSuperAdmin) return;
    if (!window.confirm(`Excluir role "${selected.name}"?`)) return;
    setError(null);
    try {
      await rolesApi.deleteRole(selected.id);
      setSelectedId(null);
      await loadAll();
    } catch {
      setError('Não foi possível excluir a role.');
    }
  }

  async function handleSavePermissions() {
    if (!canMutate || !selected || isSuperAdmin) return;
    const names = Object.entries(permSelections)
      .filter(([, v]) => v)
      .map(([k]) => k);
    setError(null);
    try {
      await rolesApi.setRolePermissions(selected.id, names);
      await loadAll();
    } catch {
      setError('Não foi possível salvar permissões.');
    }
  }

  if (!canRead) {
    return (
      <div className="pdv-main__header">
        <h1>Roles e permissões</h1>
        <p className="pdv-error">Você não tem permissão para acessar esta área.</p>
      </div>
    );
  }

  return (
    <>
      <div className="pdv-main__header">
        <div>
          <h1>Roles e permissões</h1>
          <p style={{ margin: '0.25rem 0 0', color: 'var(--pdv-muted, #45474c)', fontSize: '0.9rem' }}>
            Fase 6 — administrar roles e permissões. Referência Stitch:{' '}
            <code style={{ fontSize: '0.85rem' }}>docs/design/stitch-phase6-users-roles-ui.md</code>
          </p>
        </div>
        <button type="button" className="pdv-btn pdv-btn--ghost" onClick={() => void loadAll()} disabled={loading}>
          Atualizar
        </button>
      </div>

      {error && <p className="pdv-error">{error}</p>}

      {canMutate && (
        <div className="pdv-card" style={{ marginBottom: '1rem', padding: '1rem 1.25rem' }}>
          <h2 style={{ marginTop: 0, marginBottom: '1rem', fontSize: '1.05rem', fontWeight: 600 }}>Nova role</h2>
          <div className="pdv-toolbar" style={{ flexWrap: 'wrap', gap: '0.75rem', alignItems: 'flex-end' }}>
            <div className="pdv-field" style={{ marginBottom: 0, flex: '1 1 12rem', minWidth: 0 }}>
              <label htmlFor="role-new-name">Nome</label>
              <input
                id="role-new-name"
                value={newName}
                onChange={(e) => setNewName(e.target.value)}
                placeholder="Ex.: Caixa"
              />
            </div>
            <button type="button" className="pdv-btn pdv-btn--primary" onClick={() => void handleCreate()}>
              Criar role
            </button>
          </div>
        </div>
      )}

      <div className="pdv-master-detail">
        <div className="pdv-card pdv-table-wrap" style={{ marginBottom: 0 }}>
          <h2 style={{ margin: 0, padding: '1rem 1.25rem', fontSize: '1.05rem', fontWeight: 600 }}>Roles</h2>
          {loading && roles.length === 0 ? (
            <p className="pdv-empty">Carregando…</p>
          ) : roles.length === 0 ? (
            <p className="pdv-empty">Nenhuma role.</p>
          ) : (
            <ul style={{ listStyle: 'none', margin: 0, padding: '0.5rem 0' }}>
              {roles.map((r) => (
                <li key={r.id}>
                  <button
                    type="button"
                    className={`pdv-sidebar__link${selectedId === r.id ? ' pdv-sidebar__link--active' : ''}`}
                    style={{
                      width: '100%',
                      textAlign: 'left',
                      border: 'none',
                      background: selectedId === r.id ? 'rgba(0, 88, 190, 0.12)' : 'transparent',
                      cursor: 'pointer',
                      padding: '0.65rem 1.25rem',
                      color: 'inherit',
                      font: 'inherit',
                    }}
                    onClick={() => setSelectedId(r.id)}
                  >
                    {r.name}
                  </button>
                </li>
              ))}
            </ul>
          )}
        </div>

        <div className="pdv-card" style={{ padding: '1rem 1.25rem' }}>
          {!selected ? (
            <p className="pdv-empty">Selecione uma role.</p>
          ) : (
            <>
              <h2 style={{ marginTop: 0, marginBottom: '1rem', fontSize: '1.05rem', fontWeight: 600 }}>Detalhe</h2>
              {canMutate ? (
                <div className="pdv-toolbar" style={{ flexWrap: 'wrap', gap: '0.75rem', marginBottom: '1rem', alignItems: 'flex-end' }}>
                  <div className="pdv-field" style={{ marginBottom: 0, flex: '1 1 12rem', minWidth: 0 }}>
                    <label htmlFor="role-edit-name">Nome</label>
                    <input
                      id="role-edit-name"
                      value={editName}
                      onChange={(e) => setEditName(e.target.value)}
                      disabled={isSuperAdmin}
                    />
                  </div>
                  <button type="button" className="pdv-btn pdv-btn--primary" onClick={() => void handleSaveName()} disabled={isSuperAdmin}>
                    Salvar nome
                  </button>
                  <button type="button" className="pdv-btn pdv-btn--ghost" onClick={() => void handleDelete()} disabled={isSuperAdmin}>
                    Excluir
                  </button>
                </div>
              ) : (
                <p style={{ color: 'var(--pdv-muted)', fontSize: '0.9rem', marginTop: 0 }}>Somente leitura — sem permissão para alterar roles.</p>
              )}

              <h3 style={{ fontSize: '0.95rem', fontWeight: 600, margin: '1rem 0 0.75rem' }}>Permissões</h3>
              {isSuperAdmin ? (
                <p className="pdv-empty" style={{ padding: '0.5rem 0' }}>
                  Super Admin: permissões geridas automaticamente pelo sistema.
                </p>
              ) : (
                <>
                  <div className="pdv-perm-grid">
                    {catalog.map((p) => (
                      <label key={p} style={{ display: 'flex', gap: '0.5rem', alignItems: 'center', fontSize: '0.875rem' }}>
                        <input
                          type="checkbox"
                          checked={!!permSelections[p]}
                          disabled={!canMutate}
                          onChange={(e) => setPermSelections((prev) => ({ ...prev, [p]: e.target.checked }))}
                        />
                        <code style={{ fontSize: '0.8rem' }}>{p}</code>
                      </label>
                    ))}
                  </div>
                  {canMutate && (
                    <button type="button" className="pdv-btn pdv-btn--primary" style={{ marginTop: '1rem' }} onClick={() => void handleSavePermissions()}>
                      Salvar permissões
                    </button>
                  )}
                </>
              )}
            </>
          )}
        </div>
      </div>
    </>
  );
}
