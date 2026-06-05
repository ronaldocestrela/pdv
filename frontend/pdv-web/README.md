# Frontend PDV Web

Aplicação React + TypeScript + Vite para operação do PDV.

## Requisitos

- Node.js 20.19+ (ou 22 LTS)
- npm

## Comandos

| Ação | Comando |
|------|---------|
| Instalar dependências | `npm install` |
| Rodar em desenvolvimento | `npm run dev` |
| Build de produção | `npm run build` |
| Testes | `npm test` |

## Configuração de ambiente

Use `.env` (ou `.env.local`) com:

- `VITE_API_URL` (default: `http://localhost:5190`)

## Autenticação e sessão

- Login e refresh em `POST /api/auth/login` e `POST /api/auth/refresh`.
- Sessão persistida via Zustand (`pdv-auth`).
- Dados de sessão incluem `accessToken`, `refreshToken`, permissões, usuário e `tenantId`.

## Multitenancy (fase atual)

- Frontend já está preparado para manter `tenantId` na sessão.
- Operação MVP atual: 1 tenant por sessão/login.
- Não há seletor de tenant na UI nesta fase.

## Referências de UI

- Login: `docs/STITCH_LOGIN.md`
- Referências gerais de telas e fluxos: `../../docs/design/`
