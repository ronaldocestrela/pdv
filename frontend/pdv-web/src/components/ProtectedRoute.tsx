import { Navigate, useLocation } from 'react-router-dom';
import { useAuthStore } from '../store/auth';

/**
 * TODO: Describe ProtectedRoute.
 */
export function ProtectedRoute({ children }: { children: React.ReactNode }) {
  const accessToken = useAuthStore((s) => s.accessToken);
  const location = useLocation();

  if (!accessToken) {
    return <Navigate to="/login" state={{ from: location.pathname }} replace />;
  }

  return <>{children}</>;
}
