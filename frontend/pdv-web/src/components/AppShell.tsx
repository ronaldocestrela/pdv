import { useCallback, useEffect, useId, useState } from 'react';
import { NavLink, Outlet, useLocation, useNavigate } from 'react-router-dom';
import { useAuthStore } from '../store/auth';
import { usePermission } from '../hooks/usePermission';
import { PERMISSIONS } from '../constants/permissions';
import { useMediaQuery } from '../hooks/useMediaQuery';
import '../pages/pdvTheme.css';

const DRAWER_QUERY = '(max-width: 899px)';

const linkClass = ({ isActive }: { isActive: boolean }) =>
  `pdv-sidebar__link${isActive ? ' pdv-sidebar__link--active' : ''}`;

/**
 * TODO: Describe AppShell.
 */
export function AppShell() {
  const logout = useAuthStore((s) => s.logout);
  const navigate = useNavigate();
  const location = useLocation();
  const { can } = usePermission();
  const isDrawerLayout = useMediaQuery(DRAWER_QUERY);
  const [navOpen, setNavOpen] = useState(false);
  const sidebarId = useId();

  const showStock = can(PERMISSIONS.stockView) || can(PERMISSIONS.stockAdjust);
  const showSuppliers = can(PERMISSIONS.supplierView);
  const showPdv = can(PERMISSIONS.saleCreate) || can(PERMISSIONS.saleView);
  const showReports = can(PERMISSIONS.reportView) || can(PERMISSIONS.cashflowView);
  const showUsers = can(PERMISSIONS.userManage);
  const showRoles = can(PERMISSIONS.roleManage) || can(PERMISSIONS.userManage);

  const closeNav = useCallback(() => {
    setNavOpen(false);
  }, []);

  // Fecha o drawer ao mudar de rota (ex.: voltar no navegador).
  useEffect(() => {
    if (!isDrawerLayout) return;
    // eslint-disable-next-line react-hooks/set-state-in-effect -- sincronizar drawer com a rota
    closeNav();
  }, [location.pathname, isDrawerLayout, closeNav]);

  useEffect(() => {
    if (!isDrawerLayout || !navOpen) return;
    const onKey = (e: KeyboardEvent) => {
      if (e.key !== 'Escape') return;
      e.preventDefault();
      e.stopPropagation();
      closeNav();
    };
    window.addEventListener('keydown', onKey, true);
    return () => window.removeEventListener('keydown', onKey, true);
  }, [isDrawerLayout, navOpen, closeNav]);

  const onNavClick = () => {
    if (isDrawerLayout) closeNav();
  };

  return (
    <div
      className="pdv-theme pdv-app"
      data-nav-open={isDrawerLayout && navOpen ? 'true' : undefined}
    >
      {isDrawerLayout && navOpen ? (
        <button
          type="button"
          className="pdv-nav-backdrop"
          aria-label="Fechar menu de navegação"
          tabIndex={-1}
          onClick={closeNav}
        />
      ) : null}

      <aside
        id={sidebarId}
        className="pdv-sidebar"
        aria-label="Navegação principal"
        inert={isDrawerLayout && !navOpen ? true : undefined}
      >
        <div className="pdv-sidebar__brand">PDV + Estoque</div>
        <nav className="pdv-sidebar__nav" aria-label="Principal">
          <NavLink className={linkClass} to="/" end onClick={onNavClick}>
            Início
          </NavLink>
          <NavLink className={linkClass} to="/products" onClick={onNavClick}>
            Produtos
          </NavLink>
          {showSuppliers && (
            <NavLink className={linkClass} to="/suppliers" onClick={onNavClick}>
              Fornecedores
            </NavLink>
          )}
          {showPdv && (
            <NavLink className={linkClass} to="/pdv" onClick={onNavClick}>
              PDV
            </NavLink>
          )}
          {showStock && (
            <NavLink className={linkClass} to="/stock" onClick={onNavClick}>
              Estoque
            </NavLink>
          )}
          {showReports && (
            <NavLink className={linkClass} to="/reports" onClick={onNavClick}>
              Relatórios
            </NavLink>
          )}
          {showUsers && (
            <NavLink className={linkClass} to="/users" onClick={onNavClick}>
              Usuários
            </NavLink>
          )}
          {showRoles && (
            <NavLink className={linkClass} to="/roles" onClick={onNavClick}>
              Roles
            </NavLink>
          )}
        </nav>
        <div style={{ marginTop: 'auto', padding: '1rem 1.25rem' }}>
          <button
            type="button"
            className="pdv-btn pdv-btn--ghost"
            style={{ width: '100%', color: '#fff', borderColor: 'rgba(255,255,255,0.25)' }}
            onClick={() => {
              logout();
              navigate('/login', { replace: true });
            }}
          >
            Sair
          </button>
        </div>
      </aside>

      <div className="pdv-main">
        <header className="pdv-main__topbar">
          <button
            type="button"
            className="pdv-nav-toggle"
            aria-expanded={navOpen}
            aria-controls={sidebarId}
            onClick={() => setNavOpen((o) => !o)}
          >
            <span className="pdv-nav-toggle__icon" aria-hidden>
              <span className="pdv-nav-toggle__bar" />
              <span className="pdv-nav-toggle__bar" />
              <span className="pdv-nav-toggle__bar" />
            </span>
            <span className="pdv-nav-toggle__label">Menu</span>
          </button>
        </header>
        <div className="pdv-main__body">
          <Outlet />
        </div>
      </div>
    </div>
  );
}
