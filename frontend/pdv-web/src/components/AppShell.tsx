import { NavLink, Outlet, useNavigate } from 'react-router-dom';
import { useAuthStore } from '../store/auth';
import { usePermission } from '../hooks/usePermission';
import { PERMISSIONS } from '../constants/permissions';
import '../pages/pdvTheme.css';

const linkClass = ({ isActive }: { isActive: boolean }) =>
  `pdv-sidebar__link${isActive ? ' pdv-sidebar__link--active' : ''}`;

export function AppShell() {
  const logout = useAuthStore((s) => s.logout);
  const navigate = useNavigate();
  const { can } = usePermission();
  const showStock = can(PERMISSIONS.stockView) || can(PERMISSIONS.stockAdjust);
  const showPdv = can(PERMISSIONS.saleCreate) || can(PERMISSIONS.saleView);

  return (
    <div className="pdv-theme pdv-app">
      <aside className="pdv-sidebar">
        <div className="pdv-sidebar__brand">PDV + Estoque</div>
        <nav className="pdv-sidebar__nav" aria-label="Principal">
          <NavLink className={linkClass} to="/" end>
            Início
          </NavLink>
          <NavLink className={linkClass} to="/products">
            Produtos
          </NavLink>
          {showPdv && (
            <NavLink className={linkClass} to="/pdv">
              PDV
            </NavLink>
          )}
          {showStock && (
            <NavLink className={linkClass} to="/stock">
              Estoque
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
        <Outlet />
      </div>
    </div>
  );
}
