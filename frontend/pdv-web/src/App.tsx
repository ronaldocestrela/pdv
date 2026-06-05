import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom';
import { AppShell } from './components/AppShell';
import { ProtectedRoute } from './components/ProtectedRoute';
import { LoginPage } from './pages/Login';
import { HomePage } from './pages/Home';
import { ProductsPage } from './pages/ProductsPage';
import { ProductVariationsPage } from './pages/ProductVariationsPage';
import { StockAdjustPage } from './pages/StockAdjustPage';
import { PdvPage } from './pages/PdvPage';
import { ReportsPage } from './pages/ReportsPage';
import { RolesPage } from './pages/RolesPage';
import { UsersPage } from './pages/UsersPage';
import { RegisterTenantPage } from './pages/RegisterTenantPage';
import './App.css';

export default function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/login" element={<LoginPage />} />
        <Route path="/register" element={<RegisterTenantPage />} />
        <Route
          element={
            <ProtectedRoute>
              <AppShell />
            </ProtectedRoute>
          }
        >
          <Route index element={<HomePage />} />
          <Route path="products" element={<ProductsPage />} />
          <Route path="products/:productId/variations" element={<ProductVariationsPage />} />
          <Route path="stock" element={<StockAdjustPage />} />
          <Route path="pdv" element={<PdvPage />} />
          <Route path="reports" element={<ReportsPage />} />
          <Route path="roles" element={<RolesPage />} />
          <Route path="users" element={<UsersPage />} />
        </Route>
        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </BrowserRouter>
  );
}
