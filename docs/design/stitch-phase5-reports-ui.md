# Stitch — referência de UI Fase 5 (Relatórios gerenciais)

## Nota de contexto (multitenancy)

- Os componentes visuais da tela permanecem os mesmos.
- As consultas de relatórios agora respeitam escopo lógico por `tenantId` (dados filtrados por tenant).

Tela gerada via MCP Stitch (`generate_screen_from_text`) no mesmo projeto ERP/POS das fases anteriores.

| Item | Valor |
|------|--------|
| **Stitch projectId** | `17300137737028814945` |
| Tela **Relatórios Gerenciais — PDV** | screen `6edfbb283d50483e97093aae0b08f43f` |
| **sessionId** (sessão de geração) | `11410552205549543276` |

## Prompt (resumo)

Desktop ERP POS: sidebar navy `#091426`, superfície `#fbf8fa`, cards brancos ~8px, ações `#0058be` / `#2170e4`, Inter; barra de período com **De** / **Até**, botões **Atualizar período**, **Atualizar estoque**, **Atualizar tudo**; KPI **Vendas no período**; tabelas navy header — **Produtos mais vendidos**, **Fluxo de caixa** (chips Entrada/Saída), **Estoque atual**.

## Tokens

Alinhados a [stitch-phase2-pdv-ui.md](./stitch-phase2-pdv-ui.md) e implementação em [`frontend/pdv-web/src/pages/pdvTheme.css`](../../frontend/pdv-web/src/pages/pdvTheme.css).

## Implementação no app

Tela funcional: [`frontend/pdv-web/src/pages/ReportsPage.tsx`](../../frontend/pdv-web/src/pages/ReportsPage.tsx) (`/reports`).

API: `GET /api/reports/sales`, `GET /api/reports/top-products`, `GET /api/reports/cashflow`, `GET /api/reports/stock` (ver [`agents.md`](../../agents.md)).

## Assets Stitch

- Screenshot: recurso `projects/17300137737028814945/files/b6346f9385f84ea4a67d3ced77bcd662`
- HTML export: recurso `projects/17300137737028814945/files/00bdc380fda54bc09efa052e57ba90c8`
