import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { useNavigate, Link } from 'react-router-dom';
import { isAxiosError } from 'axios';
import { registerTenant } from '../services/tenants';
import '../styles/Login.css';

/**
 * Campos do formulário de cadastro de novo tenant.
 */
type FormValues = {
  name: string;
  adminEmail: string;
  adminPassword: string;
  confirmPassword: string;
};

/**
 * RegisterTenantPage — página pública de auto-registro de um novo tenant.
 * Coleta nome da empresa, e-mail e senha do administrador inicial,
 * chama POST /api/tenants/register e redireciona para o login em caso de sucesso.
 */
export function RegisterTenantPage() {
  const navigate = useNavigate();
  const [showPassword, setShowPassword] = useState(false);
  const [submitError, setSubmitError] = useState<string | null>(null);
  const [success, setSuccess] = useState(false);

  const {
    register,
    handleSubmit,
    watch,
    formState: { errors, isSubmitting },
  } = useForm<FormValues>({
    defaultValues: { name: '', adminEmail: '', adminPassword: '', confirmPassword: '' },
  });

  const passwordValue = watch('adminPassword');

  const onSubmit = async (values: FormValues) => {
    setSubmitError(null);
    try {
      await registerTenant({
        name: values.name.trim(),
        adminEmail: values.adminEmail.trim(),
        adminPassword: values.adminPassword,
      });
      setSuccess(true);
      setTimeout(() => navigate('/login', { replace: true }), 2500);
    } catch (e: unknown) {
      if (isAxiosError(e)) {
        const detail =
          (e.response?.data as { errors?: Record<string, string[]>; detail?: string } | null)?.detail ??
          (e.response?.data as { errors?: Record<string, string[]>; title?: string } | null)?.title ??
          e.message;

        const errors =
          (e.response?.data as { errors?: Record<string, string[]> } | null)?.errors;
        if (errors) {
          const msgs = Object.values(errors).flat();
          setSubmitError(msgs.join(' '));
        } else {
          setSubmitError(typeof detail === 'string' ? detail : 'Não foi possível criar o tenant.');
        }
      } else {
        setSubmitError('Erro inesperado.');
      }
    }
  };

  if (success) {
    return (
      <div className="login-page">
        <div className="login-card" style={{ textAlign: 'center' }}>
          <h1 className="login-title" style={{ color: '#1a7a4a' }}>🎉 Conta criada!</h1>
          <p className="login-subtitle">
            Seu tenant foi cadastrado com sucesso. Você será redirecionado para o login em instantes…
          </p>
        </div>
      </div>
    );
  }

  return (
    <div className="login-page">
      <div className="login-card">
        <h1 className="login-title">Criar conta</h1>
        <p className="login-subtitle">
          Cadastre sua empresa e comece a usar o PDV + Estoque
        </p>

        <form className="login-form" onSubmit={handleSubmit(onSubmit)} noValidate>
          {/* Nome da empresa */}
          <label className="login-label" htmlFor="register-name">
            Nome da empresa
          </label>
          <input
            id="register-name"
            type="text"
            autoComplete="organization"
            placeholder="Minha Loja LTDA"
            className="login-input"
            aria-invalid={!!errors.name}
            {...register('name', {
              required: 'O nome da empresa é obrigatório.',
              maxLength: { value: 100, message: 'Máximo de 100 caracteres.' },
            })}
          />
          {errors.name && (
            <p className="login-error" role="alert">{errors.name.message}</p>
          )}

          {/* E-mail do administrador */}
          <label className="login-label" htmlFor="register-email">
            E-mail do administrador
          </label>
          <input
            id="register-email"
            type="email"
            autoComplete="email"
            placeholder="admin@minhaloja.com"
            className="login-input"
            aria-invalid={!!errors.adminEmail}
            {...register('adminEmail', {
              required: 'O e-mail é obrigatório.',
              pattern: {
                value: /^[^\s@]+@[^\s@]+\.[^\s@]+$/,
                message: 'Informe um e-mail válido.',
              },
            })}
          />
          {errors.adminEmail && (
            <p className="login-error" role="alert">{errors.adminEmail.message}</p>
          )}

          {/* Senha */}
          <label className="login-label" htmlFor="register-password">
            Senha
          </label>
          <div className="login-password-row">
            <input
              id="register-password"
              type={showPassword ? 'text' : 'password'}
              autoComplete="new-password"
              className="login-input"
              aria-invalid={!!errors.adminPassword}
              {...register('adminPassword', {
                required: 'A senha é obrigatória.',
                minLength: { value: 6, message: 'A senha deve ter no mínimo 6 caracteres.' },
              })}
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
          {errors.adminPassword && (
            <p className="login-error" role="alert">{errors.adminPassword.message}</p>
          )}

          {/* Confirmação de senha */}
          <label className="login-label" htmlFor="register-confirm">
            Confirmar senha
          </label>
          <input
            id="register-confirm"
            type={showPassword ? 'text' : 'password'}
            autoComplete="new-password"
            className="login-input"
            aria-invalid={!!errors.confirmPassword}
            {...register('confirmPassword', {
              required: 'Confirme sua senha.',
              validate: (v) => v === passwordValue || 'As senhas não coincidem.',
            })}
          />
          {errors.confirmPassword && (
            <p className="login-error" role="alert">{errors.confirmPassword.message}</p>
          )}

          {/* Erro global */}
          {submitError && (
            <p className="login-error" role="alert">{submitError}</p>
          )}

          <button
            type="submit"
            id="register-submit"
            className="login-submit"
            disabled={isSubmitting}
          >
            {isSubmitting ? 'Criando conta…' : 'Criar conta'}
          </button>
        </form>

        <p className="login-hint">
          Já tem uma conta?{' '}
          <Link to="/login">Entrar</Link>
        </p>
      </div>
    </div>
  );
}
