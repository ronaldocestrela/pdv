# Stitch — referência de UI Fase 4 (PDV / Checkout)

## Nota de contexto (multitenancy)

- O design de checkout não muda nesta fase, porém catálogo/carrinho/finalização já são servidos sob escopo de tenant no backend.
- Sessão de autenticação carrega `tenantId` e define o escopo da operação.

Tela gerada via MCP Stitch (`generate_screen_from_text`) no mesmo projeto ERP/POS das fases anteriores.

| Item | Valor |
|------|--------|
| **Stitch projectId** | `17300137737028814945` |
| Tela **Checkout PDV — Venda direta** | screen `f7e2dbc4a69445e2853f75115e8665a0` |
| **sessionId** (sessão de geração) | `5548793230796229642` |

## Prompt (resumo)

Desktop ERP POS: sidebar navy `#091426`, superfície `#fbf8fa`, ações `#0058be` / `#2170e4`, Inter; duas colunas — busca + tabela com variação em dropdown; painel carrinho com steppers, total, segmentado Dinheiro/Cartão/PIX, Finalizar venda / Limpar carrinho; cards ~12px.

## Tokens

Alinhados a [stitch-phase2-pdv-ui.md](./stitch-phase2-pdv-ui.md) e implementação em [`frontend/pdv-web/src/pages/pdvTheme.css`](../../frontend/pdv-web/src/pages/pdvTheme.css).

## Implementação no app

Tela funcional: [`frontend/pdv-web/src/pages/PdvPage.tsx`](../../frontend/pdv-web/src/pages/PdvPage.tsx) (`/pdv`).
