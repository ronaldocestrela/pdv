# Stitch — referência Fase 7 (QA / testes e fluxos de UI cobertos)

## Nota de contexto (multitenancy)

- Além dos cenários já listados, a suíte backend também deve evoluir para cobrir isolamento entre tenants (leitura/escrita/relatórios/auth).
- A fase atual já inclui a base técnica de tenant em sessão (`tenantId`) para progressão desses testes.

A Fase 7 não introduz telas novas no Stitch: os fluxos já seguem tokens e layouts documentados nas fases 1–6 ([stitch-phase2-pdv-ui.md](./stitch-phase2-pdv-ui.md), [stitch-phase3-stock-ui.md](./stitch-phase3-stock-ui.md), [stitch-phase4-pdv-ui.md](./stitch-phase4-pdv-ui.md), [stitch-phase5-reports-ui.md](./stitch-phase5-reports-ui.md), [stitch-phase6-users-roles-ui.md](./stitch-phase6-users-roles-ui.md)).

Este arquivo **documenta correspondência cenário ↔ teste automatizado** e critérios de comportamento/UI verificados, em linha com o MCP Stitch como **fonte de verdade visual** das fases anteriores.

## Tokens e layout

- Continuidade visual: [`frontend/pdv-web/src/pages/pdvTheme.css`](../../frontend/pdv-web/src/pages/pdvTheme.css).
- O que os testes verificam: textos principais dos cartões/buttons Stitch (PDV Checkout, Estoque, Login), hierarquia de headings e mensagens de erro/sucesso (classes `pdv-error`, `pdv-empty`), sem capturar assets novos no Stitch.

## Mapa cenário ↔ arquivos de teste (frontend — Vitest + Testing Library)

| Fluxo Stitch / produto | O que o teste valida | Arquivo |
|------------------------|---------------------|---------|
| **Login** (Fase 1) | Redirect após sucesso usando `state.from`; mensagem amigável em 401 (`E-mail ou senha inválidos`) | [`frontend/pdv-web/src/pages/LoginPage.test.tsx`](../../frontend/pdv-web/src/pages/LoginPage.test.tsx) |
| **Rotas protegidas** | Sem `accessToken` → `/login`; com sessão → conteúdo protegido | [`frontend/pdv-web/src/components/ProtectedRoute.test.tsx`](../../frontend/pdv-web/src/components/ProtectedRoute.test.tsx) |
| **Estoque** (Fase 3) | Bloqueio sem `stock.adjust`/`stock.view`; formulário **Registrar entrada** chama API; histórico com colunas Tipo/Qtd quando `stock.view` | [`frontend/pdv-web/src/pages/StockAdjustPage.test.tsx`](../../frontend/pdv-web/src/pages/StockAdjustPage.test.tsx) |
| **PDV / Checkout** (Fase 4) | Heading “PDV — Checkout”; adicionar ao carrinho + **Finalizar venda** com payload de pagamento; erro de estoque no cliente; falha na API ao finalizar; checkout desabilitado sem `sale.create` | [`frontend/pdv-web/src/pages/PdvPage.test.tsx`](../../frontend/pdv-web/src/pages/PdvPage.test.tsx) |

## Integração HTTP (backend — ASP.NET Core)

| Área | Cenários | Arquivo |
|------|----------|---------|
| **Auth** | Login 401/sucesso; validação login; refresh inválido/válido | [`backend/tests/Pdv.Tests/Integration/AuthIntegrationTests.cs`](../../backend/tests/Pdv.Tests/Integration/AuthIntegrationTests.cs) |
| **Stock** | 401 sem token; entrada + lista de movimentos | [`backend/tests/Pdv.Tests/Integration/StockIntegrationTests.cs`](../../backend/tests/Pdv.Tests/Integration/StockIntegrationTests.cs) |
| **Vendas** | 401 sem token; venda com sucesso + listagem recente; estoque insuficiente → 400 | [`backend/tests/Pdv.Tests/Integration/SalesIntegrationTests.cs`](../../backend/tests/Pdv.Tests/Integration/SalesIntegrationTests.cs) |

Infraestrutura compartilhada: [`PdvWebApplicationFactory`](../../backend/tests/Pdv.Tests/Integration/PdvWebApplicationFactory.cs) (API in-memory isolada por instância, ambiente `Testing`, sem redirect HTTPS forçado em testes).

## Como executar

```bash
# Backend (inclui integração HTTP)
cd backend && dotnet test tests/Pdv.Tests/Pdv.Tests.csproj

# Frontend
cd frontend/pdv-web && npm test
```

## Observações Stitch / MCP

- Não há `screenId`/export novo obrigatório para a Fase 7: usar este doc como checklist de regressão visual + comportamento alinhado às refs das fases 2–6.
- Ao evoluir copy ou hierarquia de uma tela já desenhada no Stitch, atualizar primeiro o Markdown da fase correspondente e depois espelhar expectativas nos testes citados acima.
