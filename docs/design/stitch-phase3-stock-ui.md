# Stitch — referência de UI Fase 3 (Estoque / Ajuste + Histórico)

Tela refinada via MCP Stitch (`edit_screens`) no mesmo projeto ERP/POS das fases anteriores.

| Item | Valor |
|------|--------|
| **Stitch projectId** | `17300137737028814945` |
| Tela **Ajuste de Estoque e Histórico** | screen `9c4f87cae6de4adcbc554df97c658779` |
| Tela anterior **Gestão de Estoque** (baseline) | screen `a84bef7b51424b038f3aa6583af48d8a` |

Tokens visuais alinhados ao PDV (cf. [stitch-phase2-pdv-ui.md](./stitch-phase2-pdv-ui.md)):

- Sidebar: `#091426`
- Superfície principal: `#fbf8fa`
- Ações primárias: `#0058be` / `#2170e4`
- Tipografia: **Inter**
- Cards e tabelas: contraste tipo ERP (cabeçalho de tabela navy + texto claro)

O frontend replica esses valores em [`frontend/pdv-web/src/pages/pdvTheme.css`](../../frontend/pdv-web/src/pages/pdvTheme.css) e na página **Estoque** (`/stock`).
