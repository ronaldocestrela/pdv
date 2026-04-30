import { useCallback, useEffect, useState } from 'react';
import { can } from '../hooks/usePermission';
import { PERMISSIONS } from '../constants/permissions';
import * as usersApi from '../services/users';
import * as rolesApi from '../services/roles';
import type { UserAdminDto } from '../types/users';
import type { RoleAdminDto } from '../types/roles';

export function UsersPage() {
  const canManage = can(PERMISSIONS.userManage);

  const [users, setUsers] = useState<UserAdminDto[]>([]);
  const [roles, setRoles] = useState<RoleAdminDto[]>([]);
  const [selectedId, setSelectedId] = useState<number | null>(null);
  const [roleChecks, setRoleChecks] = useState<Record<number, boolean>>({});
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [savedMsg, setSavedMsg] = useState<string | null>(null);
  const [newEmail, setNewEmail] = useState('');
  const [newPassword, setNewPassword] = useState('');
  const [newIsActive, setNewIsActive] = useState(true);

  const selected = users.find((u) => u.id === selectedId) ?? null;

  const loadAll = useCallback(async () => {
    if (!canManage) return;
    setLoading(true);
    setError(null);
    try {
      const [u, r] = await Promise.all([usersApi.listUsers(), rolesApi.listRoles()]);
      setUsers(u);
      setRoles(r);
    } catch {
      setError('Não foi possível carregar usuários ou roles.');
    } finally {
      setLoading(false);
    }
  }, [canManage]);

  useEffect(() => {
    void loadAll();
  }, [loadAll]);

  useEffect(() => {
    if (selectedId === null && users.length > 0) setSelectedId(users[0].id);
  }, [users, selectedId]);

  useEffect(() => {
    if (!selected) {
      setRoleChecks({});
      return;
    }
    const next: Record<number, boolean> = {};
    for (const r of roles) next[r.id] = selected.roleIds.includes(r.id);
    setRoleChecks(next);
  }, [selected, roles]);

  async function handleSaveRoles() {
    if (!selected) return;
    const roleIds = Object.entries(roleChecks)
      .filter(([, v]) => v)
      .map(([id]) => Number(id));
    setError(null);
    setSavedMsg(null);
    try {
      await usersApi.setUserRoles(selected.id, roleIds);
      setSavedMsg('Roles atualizadas. O usuário precisa renovar o token (login ou refresh) para refletir novas permissões.');
      await loadAll();
    } catch {
      setError('Não foi possível salvar roles do usuário.');
    }
  }

  async function handleCreateUser() {
    if (!newEmail.trim() || !newPassword) return;
    setError(null);
    setSavedMsg(null);
    try {
      const id = await usersApi.createUser(newEmail.trim(), newPassword, newIsActive);
      setNewEmail('');
      setNewPassword('');
      setNewIsActive(true);
      setSavedMsg('Usuário criado. Atribua roles e informe a senha ao usuário.');
      await loadAll();
      setSelectedId(id);
    } catch {
      setError('Não foi possível criar o usuário (e-mail duplicado ou dados inválidos).');
    }
  }

  if (!canManage) {
    return (
      <div className="pdv-main__header">
        <h1>Usuários</h1>
        <p className="pdv-error">Você não tem permissão para gerenciar usuários.</p>
      </div>
    );
  }

  return (
    <>
      <div className="pdv-main__header">
        <div>
          <h1>Usuários</h1>
          <p style={{ margin: '0.25rem 0 0', color: 'var(--pdv-muted, #45474c)', fontSize: '0.9rem' }}>
            Criar usuários, atribuir roles (Fase 6). Referência Stitch:{' '}
            <code style={{ fontSize: '0.85rem' }}>docs/design/stitch-phase6-users-roles-ui.md</code>
          </p>
        </div>
        <button type="button" className="pdv-btn pdv-btn--ghost" onClick={() => void loadAll()} disabled={loading}>
          Atualizar
        </button>
      </div>

      {error && <p className="pdv-error">{error}</p>}
      {savedMsg && <p style={{ color: 'var(--pdv-muted)', fontSize: '0.9rem' }}>{savedMsg}</p>}

      <div className="pdv-card" style={{ marginBottom: '1rem', padding: '1rem 1.25rem' }}>
        <h2 style={{ marginTop: 0, marginBottom: '1rem', fontSize: '1.05rem', fontWeight: 600 }}>Novo usuário</h2>
        <div className="pdv-toolbar" style={{ flexWrap: 'wrap', gap: '0.75rem', alignItems: 'flex-end' }}>
          <div className="pdv-field" style={{ marginBottom: 0, flex: '1 1 12rem', minWidth: 0 }}>
            <label htmlFor="user-new-email">E-mail</label>
            <input
              id="user-new-email"
              type="email"
              autoComplete="off"
              value={newEmail}
              onChange={(e) => setNewEmail(e.target.value)}
              placeholder="email@empresa.com"
            />
          </div>
          <div className="pdv-field" style={{ marginBottom: 0, flex: '1 1 10rem', minWidth: 0 }}>
            <label htmlFor="user-new-password">Senha (mín. 6 caracteres)</label>
            <input
              id="user-new-password"
              type="password"
              autoComplete="new-password"
              value={newPassword}
              onChange={(e) => setNewPassword(e.target.value)}
            />
          </div>
          <label style={{ display: 'flex', gap: '0.5rem', alignItems: 'center', marginBottom: '0.35rem', fontSize: '0.9rem' }}>
            <input type="checkbox" checked={newIsActive} onChange={(e) => setNewIsActive(e.target.checked)} />
            Ativo
          </label>
          <button
            type="button"
            className="pdv-btn pdv-btn--primary"
            onClick={() => void handleCreateUser()}
            disabled={!newEmail.trim() || newPassword.length < 6}
          >
            Criar usuário
          </button>
        </div>
      </div>

      <div className="pdv-master-detail">
        <div className="pdv-card pdv-table-wrap">
          <h2 style={{ margin: 0, padding: '1rem 1.25rem', fontSize: '1.05rem', fontWeight: 600 }}>Usuários</h2>
          {loading && users.length === 0 ? (
            <p className="pdv-empty">Carregando…</p>
          ) : users.length === 0 ? (
            <p className="pdv-empty">Nenhum usuário.</p>
          ) : (
            <table className="pdv-table">
              <thead>
                <tr>
                  <th>E-mail</th>
                  <th>Ativo</th>
                </tr>
              </thead>
              <tbody>
                {users.map((u) => (
                  <tr
                    key={u.id}
                    style={{ cursor: 'pointer', background: selectedId === u.id ? 'rgba(0, 88, 190, 0.08)' : undefined }}
                    onClick={() => {
                      setSelectedId(u.id);
                      setSavedMsg(null);
                    }}
                  >
                    <td>{u.email}</td>
                    <td>{u.isActive ? 'Sim' : 'Não'}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        </div>

        <div className="pdv-card" style={{ padding: '1rem 1.25rem' }}>
          {!selected ? (
            <p className="pdv-empty">Selecione um usuário.</p>
          ) : (
            <>
              <h2 style={{ marginTop: 0, marginBottom: '0.5rem', fontSize: '1.05rem', fontWeight: 600 }}>{selected.email}</h2>
              <p style={{ margin: '0 0 1rem', color: 'var(--pdv-muted)', fontSize: '0.875rem' }}>Marque as roles e salve.</p>
              <div style={{ display: 'flex', flexDirection: 'column', gap: '0.65rem' }}>
                {roles.map((r) => (
                  <label key={r.id} style={{ display: 'flex', gap: '0.5rem', alignItems: 'center', fontSize: '0.9rem' }}>
                    <input
                      type="checkbox"
                      checked={!!roleChecks[r.id]}
                      onChange={(e) => setRoleChecks((prev) => ({ ...prev, [r.id]: e.target.checked }))}
                    />
                    <span>{r.name}</span>
                  </label>
                ))}
              </div>
              <button type="button" className="pdv-btn pdv-btn--primary" style={{ marginTop: '1.25rem' }} onClick={() => void handleSaveRoles()}>
                Salvar roles
              </button>
            </>
          )}
        </div>
      </div>
    </>
  );
}
