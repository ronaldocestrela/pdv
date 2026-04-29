import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { useNavigate, useLocation } from 'react-router-dom';
import { login } from '../services/auth';
import { isAxiosError } from 'axios';
import '../styles/Login.css';

type FormValues = {
  email: string;
  password: string;
};

export function LoginPage() {
  const navigate = useNavigate();
  const location = useLocation();
  const from = (location.state as { from?: string } | null)?.from ?? '/';

  const [showPassword, setShowPassword] = useState(false);
  const [submitError, setSubmitError] = useState<string | null>(null);

  const {
    register,
    handleSubmit,
    formState: { isSubmitting },
  } = useForm<FormValues>({ defaultValues: { email: '', password: '' } });

  const onSubmit = async (values: FormValues) => {
    setSubmitError(null);
    try {
      await login(values.email.trim(), values.password);
      navigate(from, { replace: true });
    } catch (e: unknown) {
      if (isAxiosError(e)) {
        const msg =
          e.response?.status === 401
            ? 'E-mail ou senha inválidos.'
            : (e.response?.data as { message?: string })?.message ??
              e.response?.data ??
              e.message;
        setSubmitError(typeof msg === 'string' ? msg : 'Não foi possível entrar.');
      } else {
        setSubmitError('Erro inesperado.');
      }
    }
  };

  return (
    <div className="login-page">
      <div className="login-card">
        <h1 className="login-title">PDV + Estoque</h1>
        <p className="login-subtitle">Acesso rápido ao sistema de vendas</p>

        <form className="login-form" onSubmit={handleSubmit(onSubmit)} noValidate>
          <label className="login-label" htmlFor="email">
            E-mail
          </label>
          <input
            id="email"
            type="email"
            autoComplete="email"
            placeholder="seu@email.com"
            className="login-input"
            {...register('email', { required: true })}
          />

          <label className="login-label" htmlFor="password">
            Senha
          </label>
          <div className="login-password-row">
            <input
              id="password"
              type={showPassword ? 'text' : 'password'}
              autoComplete="current-password"
              className="login-input"
              {...register('password', { required: true })}
            />
            <button
              type="button"
              className="login-toggle"
              onClick={() => setShowPassword((v) => !v)}
              aria-pressed={showPassword}
            >
              {showPassword ? 'Ocultar' : 'Mostrar'}
            </button>
          </div>

          {submitError && (
            <p className="login-error" role="alert">
              {submitError}
            </p>
          )}

          <button type="submit" className="login-submit" disabled={isSubmitting}>
            {isSubmitting ? 'Entrando…' : 'Entrar'}
          </button>
        </form>

        <button type="button" className="login-forgot" disabled>
          Esqueci minha senha
        </button>

        <p className="login-hint">Sistema MVP — Fase 1</p>
      </div>
    </div>
  );
}
