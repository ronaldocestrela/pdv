# Stitch — referência de UI Fase 2 (Produtos / Variações)

## Nota de contexto (multitenancy)

- Nesta fase do projeto, a experiência de tela permanece igual, mas os dados exibidos/resolvidos já seguem escopo por tenant no backend.
- Sessão de frontend inclui `tenantId`; não há seletor visual de tenant no MVP atual.

Screens geradas via MCP Stitch no projeto existente **Responsive UI Builder** (estilo ERP/POS já alinhado ao PDV).

| Item | Valor |
|------|--------|
| **Stitch projectId** | `17300137737028814945` |
| Tela **Produtos — Catálogo e Cadastro** | screen `82dee6fb6ee949438f5295e420c2f1d2` |
| Tela **Variações do Produto** | screen `e52be30670c3486b82920ba629d68f6d` |

Tokens visuais replicados no web app (referência):

- Sidebar: `#091426`
- Superfície principal: `#fbf8fa`
- Botões de ação: azul ~`#0058be` / `#2170e4`
- Tipografia: **Inter**
- Cards e tabelas: cantos ~`0.375rem–0.5rem`, cabeçalho de tabela em contraste (navy + texto branco)

O frontend usa esses valores em [`frontend/pdv-web/src/pages/pdvTheme.css`](../../frontend/pdv-web/src/pages/pdvTheme.css) e nos componentes das páginas de produtos.
