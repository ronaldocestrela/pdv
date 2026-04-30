# 🚀 Roadmap de Desenvolvimento — Sistema PDV + Estoque (MVP)

## 🎯 Objetivo
Construir um sistema de vendas (PDV) com controle de estoque, permissões granulares e arquitetura escalável usando .NET + React.

---

## 🔰 Fase 0 — Fundação do Projeto

### Backend
- Criar solução .NET
- Estruturar camadas:
  - Domain
  - Application
  - Infrastructure
  - API
- Configurar MediatR
- Configurar FluentValidation
- Configurar SQL Server + DbContext
- Criar migrations iniciais

### Frontend
- Criar projeto React + Vite
- Estrutura base:
  - services
  - hooks
  - store
- Configurar client HTTP

### ✅ Entrega
- Backend e frontend rodando
- Conexão com banco funcionando

**Implementação:** ver [`README.md`](README.md) na raiz (`backend/`, `frontend/pdv-web/`).

---

## 🔐 Fase 1 — Autenticação + Autorização

### Backend
- Entidades:
  - User
  - Role
  - Permission
  - UserRole
  - RolePermission
- Login com JWT
- Refresh Token
- Seed inicial:
  - Super Admin
  - Permissões base

### Frontend
- Tela de login
- Armazenamento de token
- Middleware de autenticação
- Helper de permissão (`can()`)

### ✅ Entrega
- Login funcional
- Controle de acesso ativo

---

## 📦 Fase 2 — Produtos e Variações

### Backend
- Entidades:
  - Product
  - ProductVariation
- CQRS completo:
  - Produtos: Create, Update, Delete; queries de listagem e detalhe (com variações)
  - Variações: Create, Update, Delete

### Regras
- Estoque por variação (`StockQuantity`)
- Barcode opcional (único quando preenchido)

### Frontend
- Tela **Catálogo de produtos** (`/products`) — listagem + CRUD
- Tela **Variações do produto** (`/products/:productId/variations`) — CRUD por produto

### ✅ Entrega (**concluída**)
- CRUD ponta a ponta de produtos e variações com permissões no backend (`product.*`, `variation.*`) e uso de `can()` no frontend
- Referência Stitch documentada em [`docs/design/stitch-phase2-pdv-ui.md`](docs/design/stitch-phase2-pdv-ui.md); UI alinhada ao layout/tokens Stitch (`pdvTheme.css`)

**Validação rápida (manual):** login com usuário Super Admin ou role com permissões Fase 2 → **Produtos** no menu → CRUD produto → **Gerenciar variações** → CRUD variação (estoque/barcode opcional).

---

## 📊 Fase 3 — Estoque

### Backend
- Entidade: StockMovement (`Type` IN/OUT, `Quantity`, `CreatedAtUtc`, `Reason` opcional)
- Command: AddStock (`AddStockCommand` / `POST /api/stock/adjust`)
- Query: histórico `GET /api/stock/movements` (filtro opcional `variationId`, `take`)

### Regras
- Entrada via AddStock: incrementa `StockQuantity` e grava movimento IN (histórico obrigatório)
- Sem estoque negativo em entradas (quantidade sempre positiva)
- Edição direta de `stockQuantity` em variação (`PUT /api/variations/{id}`) permanece; não gera `StockMovement` (compatibilidade Fase 2)

### Frontend
- Tela **Estoque** (`/stock`) — entrada de estoque + tabela de histórico; permissões `stock.adjust` / `stock.view`

### ✅ Entrega (**concluída**)
- API + UI ponta a ponta; policies `stock.adjust` e `stock.view`
- Referência Stitch: [`docs/design/stitch-phase3-stock-ui.md`](docs/design/stitch-phase3-stock-ui.md); tokens em `pdvTheme.css`

**Validação rápida (manual):** usuário com `stock.adjust` + `product.view` → **Estoque** → selecionar produto/variação → registrar entrada → histórico atualiza.

---

## 💰 Fase 4 — PDV (Vendas)

### Backend
- Entidades:
  - Sale
  - SaleItem
  - CashFlow

- Command: CreateSale

### Fluxo
1. Validar estoque
2. Criar venda
3. Criar itens
4. Baixar estoque
5. Registrar movimentação
6. Registrar entrada no caixa

### Frontend
- Tela de PDV:
  - Busca de produtos
  - Seleção de variação
  - Carrinho
  - Finalização

### Pagamento
- Dinheiro
- Cartão
- PIX

### ✅ Entrega (**concluída**)
- `CreateSale` + entidades `Sale`, `SaleItem`, `CashFlow`; baixa de estoque + `StockMovement` OUT + entrada no caixa; policies `sale.create` / `sale.view`
- UI **PDV** em `/pdv` com tokens Stitch (`pdvTheme.css`); referência [`docs/design/stitch-phase4-pdv-ui.md`](docs/design/stitch-phase4-pdv-ui.md)

**Validação rápida (manual):** usuário com `sale.create` + `product.view` → **PDV** → montar carrinho → escolher pagamento → **Finalizar venda** → estoque e últimas vendas atualizam.

---

## 📈 Fase 5 — Relatórios

### Backend (Queries)
- Vendas por período
- Produtos mais vendidos
- Fluxo de caixa
- Estoque atual

### Frontend
- Tela de relatórios

### ✅ Entrega (**concluída**)
- API `GET /api/reports/*` + CQRS + policies `report.view` / `cashflow.view`
- UI **Relatórios** em `/reports`; permissões espelhadas no frontend (`PERMISSIONS`)
- Referência Stitch: [`docs/design/stitch-phase5-reports-ui.md`](docs/design/stitch-phase5-reports-ui.md); tokens em `pdvTheme.css`

**Validação rápida (manual):** usuário com `report.view` (+ `cashflow.view` para fluxo de caixa) → **Relatórios** no menu → ajustar período → **Atualizar período** → KPI + tabelas; **Atualizar estoque** para listagem de variações.

---

## 👥 Fase 6 — Usuários e Permissões

### Backend
- CRUD de roles
- Atribuição de permissões
- Criação de usuários e atribuição de roles

### Frontend
- Tela de usuários (criação + roles)
- Tela de roles

### ✅ Entrega (**concluída**)
- API CQRS: `GET /api/permissions`, `GET|POST|PUT|DELETE /api/roles`, `PUT /api/roles/{id}/permissions`, `GET /api/users`, `POST /api/users`, `PUT /api/users/{id}/roles`; policies `user.manage`, `role.manage`, `admin.roles.read` (JWT com claim `permission`)
- UI **Usuários** (`/users`) e **Roles** (`/roles`); `PERMISSIONS.userManage` / `PERMISSIONS.roleManage` no menu
- Referência Stitch: [`docs/design/stitch-phase6-users-roles-ui.md`](docs/design/stitch-phase6-users-roles-ui.md); tokens em `pdvTheme.css`
- Regras: role **Super Admin** não renomeável, não excluível, permissões não editáveis manualmente (mantidas pelo seed)

**Validação rápida (manual):** Super Admin → **Usuários** → **Novo usuário** (e-mail, senha ≥6, ativo) → **Criar usuário** → atribuir roles → **Salvar roles** → em **Roles**, CRUD/permissões conforme necessário → usuário alvo faz login ou refresh para ver claims atualizados.

---

## 🧪 Fase 7 — Testes (TDD)

### Backend
- Testes de:
  - Venda
  - Estoque
  - Autenticação

### Regra
- Nenhuma feature sem teste

### ✅ Entrega (**concluída**)
- Integração HTTP com `WebApplicationFactory` sobre a API (`Pdv.Tests/Integration`): `Auth`, `Stock`, `Sales` + host de testes in-memory isolado (`PdvWebApplicationFactory`, `Program` partial em `Pdv.API`, HTTPS redirect omitido em `Testing`, nome de DB InMemory opcional via `Database:InMemoryDatabaseName`).
- Frontend: Vitest cobrindo **Login**, **ProtectedRoute**, **Estoque** e fluxos extras do **PDV** (`LoginPage.test.tsx`, `ProtectedRoute.test.tsx`, `StockAdjustPage.test.tsx`, testes expandidos em `PdvPage.test.tsx`).
- Referência Stitch / rastreabilidade QA: [`docs/design/stitch-phase7-tests-ui.md`](docs/design/stitch-phase7-tests-ui.md).

**Validação rápida (manual):**
- `cd backend && dotnet test tests/Pdv.Tests/Pdv.Tests.csproj`
- `cd frontend/pdv-web && npm test`

---

## 🧹 Fase 8 — Refino

- Melhorias de UX no PDV
- Otimização de queries
- Tratamento de erros
- Logs básicos

### ✅ Entrega
- Sistema pronto para uso real

---

## 📌 Ordem Recomendada

1. Fundação
2. Auth + Permissões
3. Produtos
4. Estoque
5. PDV
6. Relatórios
7. Usuários/Roles
8. Testes + Refino

---

## 🚧 Próximas Evoluções (Pós-MVP)

- Integração com gateway de pagamento
- Impressão térmica
- Multi-tenant
- NFC-e
- Offline-first

---

## ⚠️ Diretrizes

- Seguir CQRS rigorosamente
- Não misturar leitura e escrita
- Controllers sem regra de negócio
- Sempre validar regras de domínio
- Sempre manter testes atualizados

---

## ✅ Definição de Pronto (DoD)

Uma feature só está pronta quando:
- Código implementado
- Testes passando
- Permissões aplicadas
- Documentação atualizada
