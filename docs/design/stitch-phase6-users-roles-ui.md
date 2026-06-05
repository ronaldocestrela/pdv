# Stitch — referência de UI Fase 6 (Usuários e Roles)

## Nota de contexto (multitenancy)

- A base visual permanece igual, mas Users/Roles operam em escopo de tenant no backend.
- Administração cross-tenant é reservada a casos de Super Admin e não exposta como fluxo padrão de UI nesta fase.

Telas geradas via MCP Stitch (`generate_screen_from_text`) no mesmo projeto ERP/POS das fases anteriores.

| Item | Valor |
|------|--------|
| **Stitch projectId** | `17300137737028814945` |
| Tela **Usuários — atribuir roles** | screen `129aa36c84d94c1fb0384b964c6d9834` |
| **sessionId** (usuários) | `10311599406754942082` |
| Tela **Roles e permissões** | screen `b4e2cde9f35f4ab7a68fb3597bf2f14c` |
| **sessionId** (roles) | `14047651184525320453` |

## Prompt (resumo)

- **Usuários:** Desktop ERP POS; sidebar navy `#091426`, superfície `#fbf8fa`, ações `#0058be` / `#2170e4`, Inter; duas colunas — tabela de usuários (e-mail, ativo) + checklist de roles e botão **Salvar roles**; nota sobre refresh de token; botão **Atualizar**.
- **Roles:** Mesmos tokens; card **Nova role**; coluna esquerda com lista de roles; detalhe com nome, **Salvar nome**, **Excluir**, grade de checkboxes de permissões (`product.view`, …), **Salvar permissões**; nota para Super Admin.

## Tokens

Alinhados a [stitch-phase2-pdv-ui.md](./stitch-phase2-pdv-ui.md) e [`frontend/pdv-web/src/pages/pdvTheme.css`](../../frontend/pdv-web/src/pages/pdvTheme.css).

## Implementação no app

- **Usuários:** [`frontend/pdv-web/src/pages/UsersPage.tsx`](../../frontend/pdv-web/src/pages/UsersPage.tsx) (`/users`). API: `GET /api/users`, `POST /api/users` (e-mail, senha, `isActive`), `PUT /api/users/{id}/roles` (policy `user.manage`).
- **Roles:** [`frontend/pdv-web/src/pages/RolesPage.tsx`](../../frontend/pdv-web/src/pages/RolesPage.tsx) (`/roles`). API: `GET/POST/PUT/DELETE /api/roles`, `PUT /api/roles/{id}/permissions` (mutações com `role.manage`; leitura de lista/detalhe com `admin.roles.read` — `user.manage` **ou** `role.manage`). Catálogo: `GET /api/permissions`.

## Assets Stitch (usuários)

- Screenshot: recurso `projects/17300137737028814945/files/e96bb2a78ea44c00a04f4a7d812bf315`
- HTML export: recurso `projects/17300137737028814945/files/e72eee204d3f473099d9facb7e0af3e2`

## Assets Stitch (roles)

- Screenshot: recurso `projects/17300137737028814945/files/0396bbf99c5b4b61aaf5fc8965ab39f2`
- HTML export: recurso `projects/17300137737028814945/files/ed62dc3114ab4653bdb8b5a0c9d49055`
